using System;
using System.IO;
using MelonLoader;
using UpgradeableMixStationMod.Data;
using ScheduleOne;
using ScheduleOne.DevUtilities;
using ScheduleOne.ItemFramework;
using ScheduleOne.Product;
using UnityEngine;

namespace UpgradeableMixStationMod.Items
{
    public static class UpgradeParts
    {
        public static PropertyItemDefinition MixingGear { get; private set; }
        public static PropertyItemDefinition PrecisionComponent { get; private set; }
        public static PropertyItemDefinition PrototypeCore { get; private set; }

        private static bool _registered;

        private static readonly string IconFolder = Path.Combine(
            Directory.GetCurrentDirectory(), "Mods", "UpgradeableMixStationMod", "Icons");

        public static void Register()
        {
            if (_registered) return;

            var registry = Singleton<Registry>.Instance;
            if (registry == null)
            {
                Melon<Mod>.Logger.Error("Registry not found.");
                return;
            }

            var template = Registry.GetItem<PropertyItemDefinition>("iodine");
            if (template == null)
            {
                Melon<Mod>.Logger.Error("Could not find iodine template.");
                return;
            }

            MixingGear = CreatePart(template, registry,
                UpgradeConfig.MixingGearID, "Mixing Gear",
                "MixingGear.png", new Color(0.5f, 0.7f, 0.9f));

            PrecisionComponent = CreatePart(template, registry,
                UpgradeConfig.PrecisionComponentID, "Precision Component",
                "PrecisionComponent.png", new Color(0.9f, 0.6f, 0.2f));

            PrototypeCore = CreatePart(template, registry,
                UpgradeConfig.PrototypeCoreID, "Prototype Core",
                "PrototypeCore.png", new Color(0.8f, 0.2f, 1f));

            _registered = true;
        }

        private static PropertyItemDefinition CreatePart(
            PropertyItemDefinition template, Registry registry,
            string id, string name, string iconFile, Color fallbackColor)
        {
            var def = UnityEngine.Object.Instantiate(template);
            def.name = id;
            def.Name = name;
            def.ID = id;
            def.Properties = new System.Collections.Generic.List<ScheduleOne.Effects.Effect>();

            var icon = LoadIcon(iconFile);
            def.Icon = icon ?? GenerateFallbackIcon(fallbackColor);

            registry.AddToRegistry(def);
            return def;
        }

        private static Sprite LoadIcon(string fileName)
        {
            try
            {
                string path = Path.Combine(IconFolder, fileName);
                if (!File.Exists(path))
                {
                    Melon<Mod>.Logger.Warning($"Icon not found: {path} — using fallback color.");
                    return null;
                }

                byte[] data = File.ReadAllBytes(path);
                var tex = new Texture2D(2, 2);
                tex.LoadImage(data);
                return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            }
            catch (Exception ex)
            {
                Melon<Mod>.Logger.Warning($"Icon load failed: {ex.Message}");
                return null;
            }
        }

        private static Sprite GenerateFallbackIcon(Color color)
        {
            var tex = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            var pixels = new Color[64 * 64];

            for (int y = 0; y < 64; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    float dx = Mathf.Abs(x - 31.5f);
                    float dy = Mathf.Abs(y - 31.5f);
                    float corner = Mathf.Sqrt(Mathf.Max(0, dx - 24) * Mathf.Max(0, dx - 24) +
                                              Mathf.Max(0, dy - 24) * Mathf.Max(0, dy - 24));
                    float alpha = corner <= 7 ? 1f : 0f;
                    float brightness = 0.85f + 0.15f * (y / 64f);
                    pixels[y * 64 + x] = new Color(color.r * brightness, color.g * brightness, color.b * brightness, alpha);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f));
        }
    }
}