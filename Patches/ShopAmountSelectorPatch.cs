using System.Collections;
using HarmonyLib;
using MelonLoader;
using ScheduleOne.UI.Shop;

namespace UpgradeableMixStationMod.Patches
{
    [HarmonyPatch(typeof(ShopAmountSelector), nameof(ShopAmountSelector.Open))]
    public static class ShopAmountSelectorOpenPatch
    {
        [HarmonyPostfix]
        public static void Postfix(ShopAmountSelector __instance)
        {
            MelonCoroutines.Start(ActivateNextFrame(__instance));
        }

        private static IEnumerator ActivateNextFrame(ShopAmountSelector selector)
        {
            yield return null;
            if (selector != null && selector.IsOpen && selector.InputField != null)
                selector.InputField.ActivateInputField();
        }
    }
}
