using System;
using System.Collections.Generic;
using System.IO;
using MelonLoader;
using Newtonsoft.Json;
using ScheduleOne.DevUtilities;
using ScheduleOne.Money;
using ScheduleOne.Persistence;
using ScheduleOne.PlayerScripts;

namespace UpgradeableMixStationMod.Data
{
    [Serializable]
    public class SaveFile
    {
        public Dictionary<string, StationUpgradeData> Stations = new Dictionary<string, StationUpgradeData>();
    }

    public class UpgradeManager
    {
        private const string FileName = "UpgradeableMixStationMod.json";
        private Dictionary<string, StationUpgradeData> _data = new Dictionary<string, StationUpgradeData>();

        public StationUpgradeData GetData(Guid stationGuid)
        {
            string key = stationGuid.ToString();
            if (!_data.TryGetValue(key, out var data))
            {
                data = new StationUpgradeData();
                _data[key] = data;
            }
            return data;
        }

        public bool HasRequiredItems(UpgradeTier tier)
        {
            var inv = PlayerSingleton<PlayerInventory>.Instance;
            if (inv == null) return false;

            foreach (var req in tier.Items)
            {
                if (inv.GetAmountOfItem(req.ItemID) < req.Amount)
                    return false;
            }
            return true;
        }

        public int GetPlayerItemCount(string itemID)
        {
            var inv = PlayerSingleton<PlayerInventory>.Instance;
            if (inv == null) return 0;
            return (int)inv.GetAmountOfItem(itemID);
        }

        public bool TryUpgradeSpeed(Guid stationGuid, out string failReason)
        {
            var data = GetData(stationGuid);
            if (!data.CanUpgradeSpeed)
            {
                failReason = "Already at max level.";
                return false;
            }
            var tier = data.NextSpeedTier;

            if (!HasRequiredItems(tier))
            {
                failReason = "Missing required parts.";
                return false;
            }
            if (!TryDeductCash(tier.Cost, out failReason))
                return false;

            ConsumeItems(tier);
            data.SpeedLevel++;
            SaveNow();
            return true;
        }

        public bool TryUpgradeQuality(Guid stationGuid, out string failReason)
        {
            var data = GetData(stationGuid);
            if (!data.CanUpgradeQuality)
            {
                failReason = "Already at max level.";
                return false;
            }
            var tier = data.NextQualityTier;

            if (!HasRequiredItems(tier))
            {
                failReason = "Missing required parts.";
                return false;
            }
            if (!TryDeductCash(tier.Cost, out failReason))
                return false;

            ConsumeItems(tier);
            data.QualityLevel++;
            SaveNow();
            return true;
        }

        private void ConsumeItems(UpgradeTier tier)
        {
            var inv = PlayerSingleton<PlayerInventory>.Instance;
            if (inv == null) return;

            foreach (var req in tier.Items)
            {
                for (int i = 0; i < req.Amount; i++)
                    inv.RemoveAmountOfItem(req.ItemID, 1u);
            }
        }

        private bool TryDeductCash(float cost, out string failReason)
        {
            failReason = "";
            try
            {
                var money = NetworkSingleton<MoneyManager>.Instance;
                if (money == null)
                {
                    failReason = "Money system unavailable.";
                    return false;
                }
                if (money.cashBalance < cost)
                {
                    failReason = $"Not enough cash. Need {MoneyManager.FormatAmount(cost)}.";
                    return false;
                }
                money.ChangeCashBalance(-cost);
                return true;
            }
            catch (Exception ex)
            {
                failReason = "Error accessing money system.";
                Melon<Mod>.Logger.Error($"TryDeductCash failed: {ex}");
                return false;
            }
        }

        private void SaveNow()
        {
            try
            {
                var path = Singleton<LoadManager>.Instance?.LoadedGameFolderPath;
                if (!string.IsNullOrEmpty(path))
                    Save(path);
            }
            catch { }
        }

        public void Save(string saveFolderPath)
        {
            try
            {
                var save = new SaveFile();
                foreach (var kvp in _data)
                {
                    if (kvp.Value.SpeedLevel > 0 || kvp.Value.QualityLevel > 0)
                        save.Stations[kvp.Key] = kvp.Value;
                }
                string filePath = Path.Combine(saveFolderPath, FileName);
                File.WriteAllText(filePath, JsonConvert.SerializeObject(save, Formatting.Indented));
            }
            catch (Exception ex)
            {
                Melon<Mod>.Logger.Error($"Failed to save: {ex}");
            }
        }

        public void Load(string saveFolderPath)
        {
            try
            {
                string filePath = Path.Combine(saveFolderPath, FileName);
                if (!File.Exists(filePath))
                {
                    _data.Clear();
                    return;
                }
                var save = JsonConvert.DeserializeObject<SaveFile>(File.ReadAllText(filePath));
                _data = save?.Stations ?? new Dictionary<string, StationUpgradeData>();
            }
            catch (Exception ex)
            {
                Melon<Mod>.Logger.Error($"Failed to load: {ex}");
                _data.Clear();
            }
        }
    }
}