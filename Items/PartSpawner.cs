using System;
using System.Collections.Generic;
using System.Linq;
using MelonLoader;
using UpgradeableMixStationMod.Data;
using ScheduleOne.DevUtilities;
using ScheduleOne.Economy;
using ScheduleOne.ItemFramework;
using UnityEngine;

namespace UpgradeableMixStationMod.Items
{
    public static class PartSpawner
    {
        private const int SpawnsPerDay = 3;
        private static readonly List<DeadDrop> _trackedDrops = new List<DeadDrop>();
        private static bool _hooked;
        private static int _lastHintDay = -1;

        public static IReadOnlyList<DeadDrop> TrackedDrops => _trackedDrops;

        public static bool HasGivenHintToday => _lastHintDay == GetCurrentDay();

        public static void MarkHintGiven()
        {
            _lastHintDay = GetCurrentDay();
        }

        public static DeadDrop GetHintDrop()
        {
            if (_trackedDrops.Count == 0) return null;
            int index = GetCurrentDay() % _trackedDrops.Count;
            return _trackedDrops[index];
        }

        private static int GetCurrentDay()
        {
            try
            {
                return NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance.ElapsedDays;
            }
            catch { return 0; }
        }

        public static void Initialize()
        {
            if (_hooked) return;

            try
            {
                var timeMgr = NetworkSingleton<ScheduleOne.GameTime.TimeManager>.Instance;
                if (timeMgr != null)
                {
                    timeMgr.onDayPass = (Action)Delegate.Combine(timeMgr.onDayPass, new Action(OnNewDay));
                    
                    timeMgr.onSleepEnd = (Action)Delegate.Combine(timeMgr.onSleepEnd, new Action(OnNewDay));
                    
                    _hooked = true;
                    OnNewDay();
                }
            }
            catch (Exception ex)
            {
                Melon<Mod>.Logger.Error($"PartSpawner init failed: {ex}");
            }
        }

        private static int _lastSpawnDay = -1;

        private static void OnNewDay()
        {
            int today = GetCurrentDay();
            if (today == _lastSpawnDay) return;
            _lastSpawnDay = today;
            
            CleanupOldCores();
            DistributeCores();
        }
        private static void CleanupOldCores()
        {
            foreach (var drop in _trackedDrops)
            {
                if (drop == null || drop.Storage == null) continue;

                try
                {
                    var slots = drop.Storage.ItemSlots;
                    if (slots == null) continue;

                    foreach (var slot in slots)
                    {
                        if (slot?.ItemInstance != null &&
                            slot.ItemInstance.ID.ToLower() == UpgradeConfig.PrototypeCoreID)
                        {
                            slot.ClearStoredInstance();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Melon<Mod>.Logger.Warning($"Cleanup failed for drop: {ex.Message}");
                }
            }

            _trackedDrops.Clear();
        }

        private static void DistributeCores()
        {
            if (UpgradeParts.PrototypeCore == null)
            {
                Melon<Mod>.Logger.Warning("PrototypeCore definition not registered yet.");
                return;
            }

            var emptyDrops = DeadDrop.DeadDrops
                .Where(d => d != null && d.Storage != null && d.Storage.ItemCount == 0)
                .ToList();

            if (emptyDrops.Count == 0)
            {
                return;
            }

            for (int i = emptyDrops.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                var temp = emptyDrops[i];
                emptyDrops[i] = emptyDrops[j];
                emptyDrops[j] = temp;
            }

            int placed = 0;
            int target = Mathf.Min(SpawnsPerDay, emptyDrops.Count);

            for (int i = 0; i < target; i++)
            {
                var drop = emptyDrops[i];
                try
                {
                    var slots = drop.Storage.ItemSlots;
                    if (slots == null) continue;

                    var emptySlot = slots.FirstOrDefault(s => s.ItemInstance == null);
                    if (emptySlot == null) continue;

                    var instance = UpgradeParts.PrototypeCore.GetDefaultInstance(1);
                    emptySlot.SetStoredItem(instance);

                    _trackedDrops.Add(drop);
                    placed++;
                }
                catch (Exception ex)
                {
                    Melon<Mod>.Logger.Warning($"Failed to place core in dead drop: {ex.Message}");
                }
            }
        }
    }
}