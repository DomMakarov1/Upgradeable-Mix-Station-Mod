using HarmonyLib;
using ScheduleOne;
using UpgradeableMixStationMod.Shop;

namespace UpgradeableMixStationMod.Patches
{
    [HarmonyPatch(typeof(UIScreenManager), "get_IsBackTriggeredThisFrame")]
    public static class BlockPauseMenuForCustomShopPatch
    {
        public static void Postfix(ref bool __result)
        {
            if (PartsVendor.IsShopOpen)
            {
                __result = true;
            }
        }
    }
}