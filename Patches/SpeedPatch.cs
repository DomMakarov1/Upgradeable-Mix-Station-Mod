using HarmonyLib;
using ScheduleOne.ObjectScripts;
using UnityEngine;

namespace UpgradeableMixStationMod.Patches
{
    [HarmonyPatch(typeof(MixingStation), nameof(MixingStation.GetMixTimeForCurrentOperation))]
    public static class SpeedPatch
    {
        [HarmonyPostfix]
        public static void Postfix(MixingStation __instance, ref int __result)
        {
            if (__instance is not MixingStationMk2)
                return;

            var data = Mod.Upgrades.GetData(__instance.GUID);
            if (data.SpeedLevel <= 0)
                return;

            if (data.IsInstantSpeed)
            {
                __result = 1;
                return;
            }

            __result = Mathf.Max(1, Mathf.RoundToInt(__result * data.SpeedMultiplier));
        }
    }
}