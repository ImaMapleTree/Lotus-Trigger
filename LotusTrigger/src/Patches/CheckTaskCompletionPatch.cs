using HarmonyLib;
using LotusTrigger.Options;
using LotusTrigger.Options.General;

namespace LotusTrigger.Patches;

[HarmonyPriority(Priority.Low)]
[HarmonyPatch(typeof(GameManager), nameof(GameManager.CheckTaskCompletion))]
class CheckTaskCompletionPatch
{
    public static bool Prefix(ref bool __result)
    {
        if (!GameplayOptions.Instance.DisableTaskWin) return true;

        __result = false;

        return false;
    }
}