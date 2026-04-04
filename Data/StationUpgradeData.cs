using System;
using System.Collections.Generic;

namespace UpgradeableMixStationMod.Data
{
    [Serializable]
    public class ItemRequirement
    {
        public string ItemID;
        public int Amount;
        public string DisplayName;

        public ItemRequirement(string itemId, int amount, string displayName)
        {
            ItemID = itemId;
            Amount = amount;
            DisplayName = displayName;
        }
    }

    [Serializable]
    public class UpgradeTier
    {
        public string Name;
        public float Cost;
        public float Value;
        public string Description;
        public List<ItemRequirement> Items;

        public UpgradeTier(string name, float cost, float value, string description, List<ItemRequirement> items = null)
        {
            Name = name;
            Cost = cost;
            Value = value;
            Description = description;
            Items = items ?? new List<ItemRequirement>();
        }
    }

    public static class UpgradeConfig
    {
        public const string MixingGearID = "mixinggear";
        public const string PrecisionComponentID = "precisioncomponent";
        public const string PrototypeCoreID = "prototypecore";

        public static readonly List<UpgradeTier> SpeedTiers = new List<UpgradeTier>
        {
            new UpgradeTier("Quick Mix", 3000f, 0.80f, "80% of original time", new List<ItemRequirement>
            {
                new ItemRequirement(MixingGearID, 2, "Mixing Gear")
            }),
            new UpgradeTier("Rapid Mix", 6000f, 0.60f, "60% of original time", new List<ItemRequirement>
            {
                new ItemRequirement(MixingGearID, 3, "Mixing Gear"),
                new ItemRequirement(PrecisionComponentID, 1, "Precision Component")
            }),
            new UpgradeTier("Swift Mix", 8000f, 0.40f, "40% of original time", new List<ItemRequirement>
            {
                new ItemRequirement(MixingGearID, 4, "Mixing Gear"),
                new ItemRequirement(PrecisionComponentID, 2, "Precision Component")
            }),
            new UpgradeTier("Blitz Mix", 10000f, 0.20f, "20% of original time", new List<ItemRequirement>
            {
                new ItemRequirement(PrecisionComponentID, 3, "Precision Component"),
                new ItemRequirement(PrototypeCoreID, 1, "Prototype Core")
            }),
            new UpgradeTier("Instant Mix", 20000f, 0f, "Completes in 1 minute", new List<ItemRequirement>
            {
                new ItemRequirement(PrecisionComponentID, 5, "Precision Component"),
                new ItemRequirement(PrototypeCoreID, 3, "Prototype Core")
            }),
        };

        public static readonly List<UpgradeTier> QualityTiers = new List<UpgradeTier>
        {
            new UpgradeTier("Standard Floor", 5000f, 2f, "Minimum quality: Standard", new List<ItemRequirement>
            {
                new ItemRequirement(MixingGearID, 3, "Mixing Gear"),
                new ItemRequirement(PrecisionComponentID, 1, "Precision Component")
            }),
            new UpgradeTier("Premium Floor", 12000f, 3f, "Minimum quality: Premium", new List<ItemRequirement>
            {
                new ItemRequirement(PrecisionComponentID, 4, "Precision Component"),
                new ItemRequirement(PrototypeCoreID, 1, "Prototype Core")
            }),
            new UpgradeTier("Heavenly Floor", 20000f, 4f, "Minimum quality: Heavenly", new List<ItemRequirement>
            {
                new ItemRequirement(PrecisionComponentID, 5, "Precision Component"),
                new ItemRequirement(PrototypeCoreID, 3, "Prototype Core")
            }),
        };
    }

    [Serializable]
    public class StationUpgradeData
    {
        public int SpeedLevel;
        public int QualityLevel;

        public StationUpgradeData()
        {
            SpeedLevel = 0;
            QualityLevel = 0;
        }

        public bool CanUpgradeSpeed => SpeedLevel < UpgradeConfig.SpeedTiers.Count;
        public bool IsInstantSpeed => SpeedLevel > 0 && UpgradeConfig.SpeedTiers[SpeedLevel - 1].Value == 0f;
        public float SpeedMultiplier => SpeedLevel > 0 ? UpgradeConfig.SpeedTiers[SpeedLevel - 1].Value : 1f;
        public UpgradeTier NextSpeedTier => CanUpgradeSpeed ? UpgradeConfig.SpeedTiers[SpeedLevel] : null;
        public string CurrentSpeedName => SpeedLevel > 0 ? UpgradeConfig.SpeedTiers[SpeedLevel - 1].Name : "None";

        public bool CanUpgradeQuality => QualityLevel < UpgradeConfig.QualityTiers.Count;
        public int QualityFloorValue => QualityLevel > 0 ? (int)UpgradeConfig.QualityTiers[QualityLevel - 1].Value : 0;
        public UpgradeTier NextQualityTier => CanUpgradeQuality ? UpgradeConfig.QualityTiers[QualityLevel] : null;
        public string CurrentQualityName => QualityLevel > 0 ? UpgradeConfig.QualityTiers[QualityLevel - 1].Name : "None";
    }
}