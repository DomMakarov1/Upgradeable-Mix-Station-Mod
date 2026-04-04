using System;
using System.IO;
using MelonLoader;
using UpgradeableMixStationMod.Data;
using ScheduleOne.Money;
using ScheduleOne.ObjectScripts;
using ScheduleOne.UI.Stations;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UpgradeableMixStationMod.UI
{
    public class PanelDragger : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public RectTransform Target;
        private Vector2 _dragOffset;

        public void OnBeginDrag(PointerEventData eventData)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                Target.parent as RectTransform, eventData.position, eventData.pressEventCamera, out var local);
            _dragOffset = (Vector2)Target.localPosition - local;
        }

        public void OnDrag(PointerEventData eventData)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                Target.parent as RectTransform, eventData.position, eventData.pressEventCamera, out var local);
            Target.localPosition = local + _dragOffset;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            UpgradePanel.SavePosition(Target.anchoredPosition);
        }
    }

    public static class UpgradePanel
    {
        private static GameObject _panelRoot;
        private static RectTransform _panelRect;

        private static TextMeshProUGUI _speedCurrentLabel;
        private static TextMeshProUGUI _speedNextLabel;
        private static TextMeshProUGUI _speedReqLabel;
        private static Button _speedButton;
        private static TextMeshProUGUI _speedButtonText;

        private static TextMeshProUGUI _qualityCurrentLabel;
        private static TextMeshProUGUI _qualityNextLabel;
        private static TextMeshProUGUI _qualityReqLabel;
        private static Button _qualityButton;
        private static TextMeshProUGUI _qualityButtonText;

        private static TextMeshProUGUI _statusLabel;

        private static MixingStationMk2 _currentStation;
        private static TMP_FontAsset _cachedFont;
        private static Sprite _roundedSprite;

        private static string PositionPath => Path.Combine(
            MelonLoader.Utils.MelonEnvironment.UserDataDirectory,
            "UpgradeableMixStationMod_UIPos.txt");

        public static void Show(MixingStationCanvas canvas, MixingStationMk2 station)
        {
            _currentStation = station;

            if (_cachedFont == null)
                _cachedFont = canvas.InstructionLabel?.font;

            if (_roundedSprite == null)
                _roundedSprite = CreateRoundedSprite(12, 64);

            if (_panelRoot == null)
                BuildPanel(canvas.Canvas.transform);

            if (_panelRoot.transform.parent != canvas.Canvas.transform)
                _panelRoot.transform.SetParent(canvas.Canvas.transform, false);

            _panelRoot.SetActive(true);
            RefreshUI();
        }

        public static void Hide()
        {
            if (_panelRoot != null)
                _panelRoot.SetActive(false);
            _currentStation = null;
        }

        public static void SavePosition(Vector2 pos)
        {
            try { File.WriteAllText(PositionPath, $"{pos.x},{pos.y}"); }
            catch { }
        }

        private static Vector2 LoadPosition()
        {
            try
            {
                if (!File.Exists(PositionPath)) return new Vector2(-20f, -10f);
                var parts = File.ReadAllText(PositionPath).Split(',');
                return new Vector2(float.Parse(parts[0]), float.Parse(parts[1]));
            }
            catch { return new Vector2(-20f, -10f); }
        }

        private static void BuildPanel(Transform canvasParent)
        {
            _panelRoot = new GameObject("MixUpgradePanel");
            _panelRoot.transform.SetParent(canvasParent, false);

            _panelRect = _panelRoot.AddComponent<RectTransform>();
            _panelRect.anchorMin = new Vector2(1f, 1f);
            _panelRect.anchorMax = new Vector2(1f, 1f);
            _panelRect.pivot = new Vector2(1f, 1f);
            _panelRect.anchoredPosition = LoadPosition();
            _panelRect.sizeDelta = new Vector2(280f, 0f);

            var bg = _panelRoot.AddComponent<Image>();
            bg.sprite = _roundedSprite;
            bg.type = Image.Type.Sliced;
            bg.color = new Color(0.08f, 0.08f, 0.12f, 0.92f);

            var dragger = _panelRoot.AddComponent<PanelDragger>();
            dragger.Target = _panelRect;

            var vlg = _panelRoot.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(16, 16, 16, 16);
            vlg.spacing = 8f;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            var csf = _panelRoot.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var title = CreateLabel(_panelRoot.transform, "UPGRADES", 18, FontStyles.Bold);
            SetPreferredHeight(title, 28f);
            CreateDivider(_panelRoot.transform);

            CreateLabel(_panelRoot.transform, "SPEED", 14, FontStyles.Bold, new Color(0.4f, 0.8f, 1f));
            _speedCurrentLabel = CreateLabel(_panelRoot.transform, "", 12);
            _speedNextLabel = CreateLabel(_panelRoot.transform, "", 11, FontStyles.Italic, new Color(0.7f, 0.7f, 0.7f));
            _speedReqLabel = CreateLabel(_panelRoot.transform, "", 10);
            _speedReqLabel.richText = true;
            _speedButton = CreateRoundedButton(_panelRoot.transform, "Upgrade", OnSpeedClicked, out _speedButtonText);
            CreateDivider(_panelRoot.transform);

            CreateLabel(_panelRoot.transform, "QUALITY FLOOR", 14, FontStyles.Bold, new Color(1f, 0.8f, 0.3f));
            _qualityCurrentLabel = CreateLabel(_panelRoot.transform, "", 12);
            _qualityNextLabel = CreateLabel(_panelRoot.transform, "", 11, FontStyles.Italic, new Color(0.7f, 0.7f, 0.7f));
            _qualityReqLabel = CreateLabel(_panelRoot.transform, "", 10);
            _qualityReqLabel.richText = true;
            _qualityButton = CreateRoundedButton(_panelRoot.transform, "Upgrade", OnQualityClicked, out _qualityButtonText);
            CreateDivider(_panelRoot.transform);

            _statusLabel = CreateLabel(_panelRoot.transform, "", 11, FontStyles.Italic, new Color(0.9f, 0.5f, 0.5f));
            SetPreferredHeight(_statusLabel, 20f);
        }

        private static void RefreshUI()
        {
            if (_currentStation == null) return;

            var data = Mod.Upgrades.GetData(_currentStation.GUID);

            _speedCurrentLabel.text = $"Current: {data.CurrentSpeedName}";
            if (data.CanUpgradeSpeed)
            {
                var next = data.NextSpeedTier;
                _speedNextLabel.text = $"Next: {next.Name} — {next.Description}";
                _speedReqLabel.text = BuildRequirementsText(next);
                _speedReqLabel.gameObject.SetActive(true);
                _speedButtonText.text = $"Upgrade ({MoneyManager.FormatAmount(next.Cost)})";
                _speedButton.gameObject.SetActive(true);
                _speedButton.interactable = Mod.Upgrades.HasRequiredItems(next);
            }
            else
            {
                _speedNextLabel.text = "MAX LEVEL";
                _speedReqLabel.gameObject.SetActive(false);
                _speedButton.gameObject.SetActive(false);
            }

            _qualityCurrentLabel.text = $"Current: {data.CurrentQualityName}";
            if (data.CanUpgradeQuality)
            {
                var next = data.NextQualityTier;
                _qualityNextLabel.text = $"Next: {next.Name} — {next.Description}";
                _qualityReqLabel.text = BuildRequirementsText(next);
                _qualityReqLabel.gameObject.SetActive(true);
                _qualityButtonText.text = $"Upgrade ({MoneyManager.FormatAmount(next.Cost)})";
                _qualityButton.gameObject.SetActive(true);
                _qualityButton.interactable = Mod.Upgrades.HasRequiredItems(next);
            }
            else
            {
                _qualityNextLabel.text = "MAX LEVEL";
                _qualityReqLabel.gameObject.SetActive(false);
                _qualityButton.gameObject.SetActive(false);
            }

            _statusLabel.text = "";
        }

        private static string BuildRequirementsText(UpgradeTier tier)
        {
            if (tier.Items == null || tier.Items.Count == 0)
                return "";

            var sb = new System.Text.StringBuilder();
            sb.Append("Requires: ");

            for (int i = 0; i < tier.Items.Count; i++)
            {
                var req = tier.Items[i];
                int have = Mod.Upgrades.GetPlayerItemCount(req.ItemID);
                bool enough = have >= req.Amount;
                string color = enough ? "#66FF66" : "#FF6666";

                if (i > 0) sb.Append(", ");
                sb.Append($"<color={color}>{req.Amount}x {req.DisplayName} ({have})</color>");
            }

            return sb.ToString();
        }

        private static void OnSpeedClicked()
        {
            if (_currentStation == null) return;
            if (Mod.Upgrades.TryUpgradeSpeed(_currentStation.GUID, out string fail))
            {
                _statusLabel.color = new Color(0.4f, 1f, 0.4f);
                _statusLabel.text = "Speed upgraded!";
                RefreshUI();
            }
            else
            {
                _statusLabel.color = new Color(1f, 0.4f, 0.4f);
                _statusLabel.text = fail;
            }
        }

        private static void OnQualityClicked()
        {
            if (_currentStation == null) return;
            if (Mod.Upgrades.TryUpgradeQuality(_currentStation.GUID, out string fail))
            {
                _statusLabel.color = new Color(0.4f, 1f, 0.4f);
                _statusLabel.text = "Quality floor upgraded!";
                RefreshUI();
            }
            else
            {
                _statusLabel.color = new Color(1f, 0.4f, 0.4f);
                _statusLabel.text = fail;
            }
        }

        private static Sprite CreateRoundedSprite(int radius, int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color32[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = 0, dy = 0;
                    if (x < radius) dx = radius - x;
                    else if (x > size - 1 - radius) dx = x - (size - 1 - radius);
                    if (y < radius) dy = radius - y;
                    else if (y > size - 1 - radius) dy = y - (size - 1 - radius);

                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    byte a = (byte)(dist <= radius - 1 ? 255 : dist <= radius ? (byte)(255 * (radius - dist)) : 0);
                    pixels[y * size + x] = new Color32(255, 255, 255, a);
                }
            }

            tex.SetPixels32(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size),
                new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect,
                new Vector4(radius, radius, radius, radius));
        }

        private static TextMeshProUGUI CreateLabel(
            Transform parent, string text, float fontSize,
            FontStyles style = FontStyles.Normal, Color? color = null)
        {
            var go = new GameObject("Label");
            go.transform.SetParent(parent, false);

            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.color = color ?? Color.white;
            tmp.alignment = TextAlignmentOptions.Left;
            tmp.enableWordWrapping = true;
            tmp.overflowMode = TextOverflowModes.Truncate;
            if (_cachedFont != null) tmp.font = _cachedFont;

            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = fontSize + 6f;
            return tmp;
        }

        private static void SetPreferredHeight(TextMeshProUGUI label, float height)
        {
            var le = label.GetComponent<LayoutElement>();
            if (le != null) le.preferredHeight = height;
        }

        private static Button CreateRoundedButton(
            Transform parent, string text, Action onClick, out TextMeshProUGUI buttonLabel)
        {
            var go = new GameObject("Button");
            go.transform.SetParent(parent, false);
            go.AddComponent<LayoutElement>().preferredHeight = 34f;

            var img = go.AddComponent<Image>();
            img.sprite = _roundedSprite;
            img.type = Image.Type.Sliced;
            img.color = new Color(0.2f, 0.5f, 0.3f, 1f);

            var btn = go.AddComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = new Color(0.2f, 0.5f, 0.3f);
            colors.highlightedColor = new Color(0.25f, 0.65f, 0.35f);
            colors.pressedColor = new Color(0.15f, 0.4f, 0.2f);
            colors.disabledColor = new Color(0.2f, 0.2f, 0.2f);
            btn.colors = colors;
            btn.onClick.AddListener((UnityEngine.Events.UnityAction)(() => onClick?.Invoke()));

            var textGo = new GameObject("Text");
            textGo.transform.SetParent(go.transform, false);
            var textRect = textGo.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.offsetMin = new Vector2(8f, 2f);
            textRect.offsetMax = new Vector2(-8f, -2f);

            buttonLabel = textGo.AddComponent<TextMeshProUGUI>();
            buttonLabel.text = text;
            buttonLabel.fontSize = 13;
            buttonLabel.fontStyle = FontStyles.Bold;
            buttonLabel.color = Color.white;
            buttonLabel.alignment = TextAlignmentOptions.Center;
            if (_cachedFont != null) buttonLabel.font = _cachedFont;

            return btn;
        }

        private static void CreateDivider(Transform parent)
        {
            var go = new GameObject("Divider");
            go.transform.SetParent(parent, false);
            var le = go.AddComponent<LayoutElement>();
            le.preferredHeight = 1f;
            le.flexibleHeight = 0f;
            go.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.15f);
        }
    }
}