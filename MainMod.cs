using MelonLoader;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Il2CppScheduleOne.UI.Stations;
using Il2CppScheduleOne.Money;
using Il2CppScheduleOne.ObjectScripts;
using System.Collections.Generic;
using System.IO;
using System;
using System.Text.Json;
using MelonLoader.Utils;

[assembly: MelonInfo(typeof(UpgradeableMixStationMod.MixStationUpgradeMod), UpgradeableMixStationMod.BuildInfo.Name, UpgradeableMixStationMod.BuildInfo.Version, UpgradeableMixStationMod.BuildInfo.Author)]
[assembly: MelonGame("TVGS", "Schedule I")]
[assembly: MelonAdditionalDependencies("Unity.TextMeshPro")]

namespace UpgradeableMixStationMod
{
    public static class BuildInfo
    {
        public const string Name = "Dom's Upgradeable Mix Station Mod";
        public const string Description = "Adds upgrades to the Mixing Station Mk2";
        public const string Author = "Dom";
        public const string Version = "1.0.1";
    }

    public class StationUpgradeData
    {
        public int Level = 1;
        public int Cost = 500;
        public bool Enhanced = false;
        public Text LevelText;
        public GameObject UpgradePanel;
    }

    public class GlobalUpgradeState
    {
        public int Level { get; set; } = 1;
        public bool Enhanced { get; set; } = false;
    }

    public class MixStationUpgradeMod : MelonMod
    {
        public static Dictionary<MixingStationCanvas, StationUpgradeData> stationData = new();
        private static readonly string SavePath = Path.Combine(MelonEnvironment.UserDataDirectory, "MixStationUpgradeSave.json");
        private static GlobalUpgradeState globalState = new();

        public override void OnInitializeMelon()
        {
            LoadUpgradeData();
            MelonLogger.Msg("Dom's Upgradeable Mix Station Mod loaded.");
        }

        public static void SaveUpgradeData()
        {
            try
            {
                var json = JsonSerializer.Serialize(globalState, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(SavePath, json);
                MelonLogger.Msg("[Save] Upgrade data saved.");
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[Save] Failed to save upgrade data: {ex}");
            }
        }

        public static void LoadUpgradeData()
        {
            try
            {
                if (File.Exists(SavePath))
                {
                    var json = File.ReadAllText(SavePath);
                    globalState = JsonSerializer.Deserialize<GlobalUpgradeState>(json) ?? new GlobalUpgradeState();
                    MelonLogger.Msg($"[Load] Loaded upgrade level {globalState.Level}, enhanced: {globalState.Enhanced}");
                }
                else
                {
                    MelonLogger.Msg("[Load] No saved data found. Using defaults.");
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"[Load] Failed to load upgrade data: {ex}");
            }
        }

        public static void Upgrade(MixingStationCanvas stationCanvas)
        {
            if (!stationData.TryGetValue(stationCanvas, out var data)) return;

            float currentBalance = MoneyManager.Instance.cashBalance;
            if (currentBalance < data.Cost)
            {
                MelonLogger.Msg("[Upgrade] Not enough money.");
                return;
            }

            MoneyManager.Instance.ChangeCashBalance(-data.Cost, true, true);
            data.Level++;
            globalState.Level = data.Level;
            SaveUpgradeData();

            if (data.Level == 5)
            {
                data.Cost = 30000;
                UpdateUIToEnhance(data, stationCanvas);
            }
            else
            {
                data.Cost *= 2;
                UpdateLevelText(data);
            }

            MelonLogger.Msg($"[Upgrade] Upgraded to level {data.Level}");
        }

        public static void Enhance(MixingStationCanvas stationCanvas, bool fromLoad = false)
        {
            if (!stationData.TryGetValue(stationCanvas, out var data)) return;

            // Skip the money checks and enhancements if already enhanced (unless fromLoad is true)
            if (data.Enhanced && !fromLoad)
            {
                MelonLogger.Msg("[Enhance] Already enhanced.");
                return;
            }

            // Perform the UI update part of enhancement, but don't subtract money or re-check conditions
            data.Enhanced = true;
            globalState.Enhanced = true;

            // Now apply the UI changes like the original Enhance() method
            var upgradeButton = data.UpgradePanel.GetComponentInChildren<Button>();
            var buttonText = upgradeButton.GetComponentInChildren<Text>();
            if (upgradeButton != null && buttonText != null)
            {
                buttonText.text = "Max Level";
                upgradeButton.interactable = false;
                var buttonImage = upgradeButton.GetComponent<Image>();
                if (buttonImage != null)
                    buttonImage.color = new Color(0.75f, 0.75f, 0.75f, 0.5f);
            }

            // Update the level text to reflect enhanced state
            if (data.LevelText != null)
                data.LevelText.text = "Mixing Table: Enhanced";

            // If fromLoad is true, save the state immediately to reflect the change
            if (fromLoad)
            {
                SaveUpgradeData();
                MelonLogger.Msg("[Enhance] Enhancement complete (loaded state).");
            }
            else
            {
                MelonLogger.Msg("[Enhance] Enhancement complete.");
            }
        }


        public static void CreateLevelUI(MixingStationCanvas stationCanvas)
        {
            if (stationData.ContainsKey(stationCanvas)) return;

            var upgradeData = new StationUpgradeData
            {
                Level = globalState.Level,
                Enhanced = globalState.Enhanced,
                Cost = globalState.Level >= 5 ? 30000 : 500 * (int)Mathf.Pow(2, globalState.Level - 1)
            };

            Transform parent = stationCanvas.transform;

            GameObject panelGO = new GameObject("UpgradePanel");
            upgradeData.UpgradePanel = panelGO;
            panelGO.transform.SetParent(parent, false);
            RectTransform panelRect = panelGO.AddComponent<RectTransform>();
            panelRect.sizeDelta = new Vector2(260, 130);
            panelRect.localPosition = new Vector3(-400f, 0f, 0f);

            panelGO.AddComponent<Image>().color = new Color(0, 0, 0, 0.6f);
            panelGO.AddComponent<Outline>().effectColor = Color.white;

            GameObject textGO = new GameObject("LevelText");
            textGO.transform.SetParent(panelGO.transform, false);
            var levelText = textGO.AddComponent<Text>();
            levelText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            levelText.fontSize = 18;
            levelText.alignment = TextAnchor.MiddleCenter;
            upgradeData.LevelText = levelText;
            UpdateLevelText(upgradeData);

            var textRect = textGO.GetComponent<RectTransform>();
            textRect.anchoredPosition = new Vector2(0, 30);
            textRect.sizeDelta = new Vector2(240, 50);

            GameObject buttonGO = new GameObject("UpgradeButton");
            buttonGO.transform.SetParent(panelGO.transform, false);
            var upgradeButton = buttonGO.AddComponent<Button>();

            var buttonImage = buttonGO.AddComponent<Image>();
            buttonImage.color = new Color(0.85f, 0.85f, 0.85f);

            var buttonRect = buttonGO.GetComponent<RectTransform>();
            buttonRect.anchoredPosition = new Vector2(0, -25);
            buttonRect.sizeDelta = new Vector2(180, 45);

            GameObject buttonTextGO = new GameObject("ButtonText");
            buttonTextGO.transform.SetParent(buttonGO.transform, false);
            var buttonText = buttonTextGO.AddComponent<Text>();
            buttonText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            buttonText.fontSize = 18;
            buttonText.alignment = TextAnchor.MiddleCenter;
            var textRect2 = buttonTextGO.GetComponent<RectTransform>();
            textRect2.anchoredPosition = Vector2.zero;
            textRect2.sizeDelta = buttonRect.sizeDelta;

            if (upgradeData.Level == 5 && !upgradeData.Enhanced)
            {
                buttonText.text = "Enhance";
                upgradeButton.onClick.AddListener((UnityAction)(() => Enhance(stationCanvas)));
            }
            else if (upgradeData.Enhanced)
            {
                buttonText.text = "Max Level";
                upgradeButton.interactable = false;
                buttonImage.color = new Color(0.75f, 0.75f, 0.75f, 0.5f);
            }
            else
            {
                buttonText.text = "Upgrade";
                upgradeButton.onClick.AddListener((UnityAction)(() => Upgrade(stationCanvas)));
            }

            stationData[stationCanvas] = upgradeData;
        }

        private static void UpdateLevelText(StationUpgradeData data)
        {
            if (data.LevelText != null)
                data.LevelText.text = $"Mixing Table Level: {data.Level}\nNext Upgrade: ${data.Cost}";
        }

        private static void UpdateUIToEnhance(StationUpgradeData data, MixingStationCanvas stationCanvas)
        {
            UpdateLevelText(data);

            var upgradeButton = data.UpgradePanel.GetComponentInChildren<Button>();
            var buttonText = upgradeButton.GetComponentInChildren<Text>();
            if (upgradeButton != null && buttonText != null)
            {
                buttonText.text = "Enhance";
                upgradeButton.onClick.RemoveAllListeners();
                upgradeButton.onClick.AddListener((UnityAction)(() => Enhance(stationCanvas)));
            }
        }
    }

    [HarmonyPatch(typeof(MixingStationCanvas), "Start")]
    public class MixingStationCanvas_Start_Patch
    {
        public static void Postfix(MixingStationCanvas __instance)
        {
            MixStationUpgradeMod.CreateLevelUI(__instance);
        }
    }

    [HarmonyPatch(typeof(MixingStationMk2), nameof(MixingStationMk2.MixingStart))]
    public class MixingStationMk2_MixingStart_Patch
    {
        public static void Postfix(MixingStationMk2 __instance)
        {
            MixingStationCanvas[] canvases = UnityEngine.Object.FindObjectsOfType<MixingStationCanvas>();
            StationUpgradeData upgradeData = null;

            foreach (var canvas in canvases)
            {
                if (canvas.MixingStation == __instance && MixStationUpgradeMod.stationData.TryGetValue(canvas, out var data))
                {
                    upgradeData = data;
                    break;
                }
            }

            if (upgradeData == null)
            {
                MelonLogger.Warning("[MixingStart Patch] No upgrade data found. Using default timing.");
                return;
            }

            int quantity = 0;
            var label = __instance.QuantityLabel?.text;
            if (!string.IsNullOrEmpty(label))
            {
                string digits = System.Text.RegularExpressions.Regex.Match(label, "\\d+").Value;
                int.TryParse(digits, out quantity);
            }

            if (quantity <= 0)
            {
                MelonLogger.Warning("[MixingStart Patch] Invalid quantity. Aborting.");
                return;
            }

            int level = Mathf.Clamp(upgradeData.Level, 1, 5);
            float speedMultiplier = 3.0f - (level - 1) * 0.5f;
            float timeToFinish = upgradeData.Enhanced ? 1f : quantity * speedMultiplier;

            var progressLabel = __instance.ProgressLabel;
            if (progressLabel != null)
            {
                progressLabel.text = $"{timeToFinish:0.#}s remaining";
            }

            MelonCoroutines.Start(FinishEarly(__instance, timeToFinish));
            MelonLogger.Msg($"[MixingStart Patch] Mix of {quantity} will finish in {timeToFinish}s (Level {level})");
        }

        private static System.Collections.IEnumerator FinishEarly(MixingStationMk2 station, float totalTime)
        {
            float elapsed = 0f;
            var progressLabel = station.ProgressLabel;

            while (elapsed < totalTime)
            {
                float remaining = Mathf.Max(0f, totalTime - elapsed);
                string display = $"{remaining:0.#}s remaining";

                if (progressLabel != null)
                    progressLabel.text = display;

                elapsed += Time.deltaTime;
                yield return null;
            }

            station.MixingDone();
            if (progressLabel != null)
                progressLabel.text = "Mixing Complete!";

            MelonLogger.Msg("[MixingStart Patch] MixingDone() called early.");
        }
    }
}