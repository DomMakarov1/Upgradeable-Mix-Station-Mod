using HarmonyLib;
using ScheduleOne.ObjectScripts;
using ScheduleOne.UI.Stations;
using UpgradeableMixStationMod.UI;

namespace UpgradeableMixStationMod.Patches
{
    [HarmonyPatch(typeof(MixingStationCanvas), nameof(MixingStationCanvas.Open))]
    public static class CanvasOpenPatch
    {
        [HarmonyPostfix]
        public static void Postfix(MixingStationCanvas __instance, MixingStation station)
        {
            if (station is MixingStationMk2 mk2)
                UpgradePanel.Show(__instance, mk2);
        }
    }

    [HarmonyPatch(typeof(MixingStationCanvas), nameof(MixingStationCanvas.Close))]
    public static class CanvasClosePatch
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            UpgradePanel.Hide();
        }
    }
}