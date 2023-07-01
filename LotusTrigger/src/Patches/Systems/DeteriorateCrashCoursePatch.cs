using HarmonyLib;
using LotusTrigger.Options;
using LotusTrigger.Options.General;

namespace LotusTrigger.Patches.Systems;

[HarmonyPatch(typeof(HeliSabotageSystem), nameof(HeliSabotageSystem.Detoriorate))]
public static class DeteriorateCrashCoursePatch
{
    public static void Prefix(HeliSabotageSystem __instance)
    {
        if (!__instance.IsActive) return;

        if (!SabotageOptions.Instance.CustomAirshipReactorCountdown) return;

        if (__instance.Countdown > SabotageOptions.Instance.AirshipReactorCountdown)
            __instance.Countdown = SabotageOptions.Instance.AirshipReactorCountdown;
    }
}