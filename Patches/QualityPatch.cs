using HarmonyLib;
using ScheduleOne.ItemFramework;
using ScheduleOne.ObjectScripts;
using ScheduleOne.UI.Stations;

namespace UpgradeableMixStationMod.Patches
{
    [HarmonyPatch(typeof(MixingStationCanvas), nameof(MixingStationCanvas.StartMixOperation))]
    public static class QualityPatch
    {
        [HarmonyPrefix]
        public static void Prefix(MixingStationCanvas __instance, MixOperation mixOperation)
        {
            if (__instance.MixingStation is not MixingStationMk2 mk2)
                return;

            var data = Mod.Upgrades.GetData(mk2.GUID);
            if (data.QualityLevel <= 0)
                return;

            int floor = data.QualityFloorValue;
            if ((int)mixOperation.ProductQuality < floor)
                mixOperation.ProductQuality = (EQuality)floor;
        }
    }
}