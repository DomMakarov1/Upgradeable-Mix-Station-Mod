using MelonLoader;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Il2CppScheduleOne.UI.Stations;
using Il2CppScheduleOne.Money;
using Il2CppScheduleOne.ObjectScripts;

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
        public const string Version = "1.0.0";
    }

    public class StationUpgradeData
    {
        public int Level = 1;
        public int Cost = 500;
        public bool Enhanced = false;
        public Text LevelText;
        public GameObject UpgradePanel;
        public GameObject EnhancePanel;
    }

    public class MixStationUpgradeMod : MelonMod
    {
        public static Dictionary<MixingStationCanvas, StationUpgradeData> stationData = new();

        public override void OnInitializeMelon()
        {
            MelonLogger.Msg("Dom's Ugradeable Mix Station Mod loaded.");
        }

        public static void Upgrade(MixingStationCanvas stationCanvas)
        {
            if (!stationData.ContainsKey(stationCanvas)) return;

            var data = stationData[stationCanvas];

            float currentBalance = MoneyManager.Instance.cashBalance;
            if (currentBalance < data.Cost)
            {
                MelonLogger.Msg("[Upgrade] Not enough money to upgrade.");
                return;
            }

            MoneyManager.Instance.ChangeCashBalance(-data.Cost, true, true);
            data.Level++;

            if (data.Level == 5)
            {
                data.Cost = 30000;

                if (data.LevelText != null)
                    data.LevelText.text = $"Mixing Table Level: {data.Level}\nNext Upgrade: ${data.Cost}";

                var upgradeButton = data.UpgradePanel.GetComponentInChildren<Button>();
                var buttonText = upgradeButton.GetComponentInChildren<Text>();
                if (upgradeButton != null && buttonText != null)
                {
                    buttonText.text = "Enhance";
                    upgradeButton.onClick.RemoveAllListeners();
                    upgradeButton.onClick.AddListener((UnityAction)(() => Enhance(stationCanvas)));
                }
            }
            else
            {
                data.Cost *= 2;

                if (data.LevelText != null)
                    data.LevelText.text = $"Mixing Table Level: {data.Level}\nNext Upgrade: ${data.Cost}";
            }

            MelonLogger.Msg($"[Upgrade] Upgraded to level {data.Level}");
        }

        public static void Enhance(MixingStationCanvas stationCanvas)
        {
            if (!stationData.ContainsKey(stationCanvas)) return;

            var data = stationData[stationCanvas];

            if (data.Enhanced)
            {
                MelonLogger.Msg("[Enhance] Already enhanced.");
                return;
            }

            float currentBalance = MoneyManager.Instance.cashBalance;
            if (currentBalance < 30000)
            {
                MelonLogger.Msg("[Enhance] Not enough money to enhance.");
                return;
            }

            MoneyManager.Instance.ChangeCashBalance(-30000, true, true);
            data.Enhanced = true;

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

            if (data.LevelText != null)
                data.LevelText.text = "Mixing Table: Enhanced";

            MelonLogger.Msg("[Enhance] Enhancement complete. Max level reached.");
        }

        public static void CreateLevelUI(MixingStationCanvas stationCanvas)
        {
            if (stationData.ContainsKey(stationCanvas)) return;

            Transform parent = stationCanvas.transform;
            var upgradeData = new StationUpgradeData();

            GameObject panelGO = new GameObject("UpgradePanel");
            upgradeData.UpgradePanel = panelGO;
            panelGO.transform.SetParent(parent, false);
            RectTransform panelRect = panelGO.AddComponent<RectTransform>();
            panelRect.sizeDelta = new Vector2(260, 130);
            panelRect.localPosition = new Vector3(-400f, 0f, 0f);

            Image panelImage = panelGO.AddComponent<Image>();
            panelImage.color = new Color(0, 0, 0, 0.6f);

            Outline outline = panelGO.AddComponent<Outline>();
            outline.effectColor = Color.white;
            outline.effectDistance = new Vector2(3, 3);

            GameObject textGO = new GameObject("LevelText");
            textGO.transform.SetParent(panelGO.transform, false);
            Text levelText = textGO.AddComponent<Text>();
            levelText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            levelText.fontSize = 18;
            levelText.alignment = TextAnchor.MiddleCenter;
            levelText.text = $"Mixing Table Level: {upgradeData.Level}\nNext Upgrade: ${upgradeData.Cost}";
            upgradeData.LevelText = levelText;

            RectTransform textRect = textGO.GetComponent<RectTransform>();
            textRect.anchoredPosition = new Vector2(0, 30);
            textRect.sizeDelta = new Vector2(240, 50);

            GameObject buttonGO = new GameObject("UpgradeButton");
            buttonGO.transform.SetParent(panelGO.transform, false);
            Button upgradeButton = buttonGO.AddComponent<Button>();

            Image buttonImage = buttonGO.AddComponent<Image>();
            buttonImage.color = new Color(0.85f, 0.85f, 0.85f);

            RectTransform buttonRect = buttonGO.GetComponent<RectTransform>();
            buttonRect.anchoredPosition = new Vector2(0, -25);
            buttonRect.sizeDelta = new Vector2(180, 45);

            GameObject buttonTextGO = new GameObject("ButtonText");
            buttonTextGO.transform.SetParent(buttonGO.transform, false);
            Text buttonText = buttonTextGO.AddComponent<Text>();
            buttonText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            buttonText.fontSize = 18;
            buttonText.alignment = TextAnchor.MiddleCenter;
            buttonText.text = "Upgrade";

            RectTransform buttonTextRect = buttonTextGO.GetComponent<RectTransform>();
            buttonTextRect.anchoredPosition = Vector2.zero;
            buttonTextRect.sizeDelta = buttonRect.sizeDelta;

            upgradeButton.onClick.AddListener((UnityAction)(() => Upgrade(stationCanvas)));

            if (upgradeData.Level == 5)
            {
                upgradeButton.onClick.RemoveAllListeners();
                upgradeButton.onClick.AddListener((UnityAction)(() => Enhance(stationCanvas)));
                buttonText.text = "Enhance";
                upgradeData.Cost = 30000;
            }

            stationData[stationCanvas] = upgradeData;
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

            var quantityLabel = __instance.QuantityLabel;
            int quantity = 0;

            if (quantityLabel != null)
            {
                string labelText = quantityLabel.text;
                if (!string.IsNullOrEmpty(labelText))
                {
                    string digits = System.Text.RegularExpressions.Regex.Match(labelText, "\\d+").Value;
                    if (int.TryParse(digits, out int parsed))
                        quantity = parsed;
                }
            }

            if (quantity <= 0)
            {
                MelonLogger.Warning("[MixingStart Patch] Could not determine quantity. Aborting auto-finish.");
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
            MelonLogger.Msg($"[MixingStart Patch] Mix of {quantity} set to finish in {timeToFinish}s (Level {level})");
        }

        private static System.Collections.IEnumerator FinishEarly(MixingStationMk2 station, float totalTime)
        {
            float elapsed = 0f;
            var progressLabel = station.ProgressLabel;

            while (elapsed < totalTime)
            {
                float remaining = Mathf.Max(0f, totalTime - elapsed);
                string desiredText = $"{remaining:0.#}s remaining";

                if (progressLabel != null && progressLabel.text != desiredText)
                {
                    progressLabel.text = desiredText;
                }

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