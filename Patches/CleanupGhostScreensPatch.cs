using HarmonyLib;
using ScheduleOne;

namespace UpgradeableMixStationMod.Patches
{
    [HarmonyPatch(typeof(UIScreenManager), "Update")]
    public static class CleanupGhostScreensPatch
    {
        public static void Prefix(UIScreenManager __instance)
        {
            if (!__instance.IsAnyScreenActive()) return;

            UIScreen top = __instance.TopScreen;
            
            if (top == null || !top.gameObject.activeInHierarchy)
            {
                var stack = Traverse.Create(__instance).Field("screenStack").GetValue<System.Collections.Generic.Stack<UIScreenManager.UIScreenInfo>>();
                if (stack != null && stack.Count > 0)
                {
                    stack.Pop();
                }
            }
        }
    }
}