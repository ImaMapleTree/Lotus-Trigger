using HarmonyLib;
using Lotus.API.Vanilla.Sabotages;
using Lotus.Patches.Systems;
using LotusTrigger.Options;
using LotusTrigger.Options.General;

namespace LotusTrigger.Patches.Systems;

[HarmonyPatch(typeof(ReactorSystemType), nameof(ReactorSystemType.Detoriorate))]
public static class DeteriorateReactorPatch
{
    public static void Prefix(ReactorSystemType __instance)
    {
        if (SabotagePatch.CurrentSabotage?.SabotageType() is SabotageType.Reactor)
            SabotagePatch.SabotageCountdown = __instance.Countdown;

        if (!__instance.IsActive) return;

        switch (ShipStatus.Instance.Type)
        {
            case ShipStatus.MapType.Ship when SabotageOptions.Instance.CustomSkeldReactorCountdown:
                if (__instance.Countdown > SabotageOptions.Instance.SkeldReactorCountdown)
                    __instance.Countdown = SabotageOptions.Instance.SkeldReactorCountdown;
                break;
            case ShipStatus.MapType.Hq when SabotageOptions.Instance.CustomMiraReactorCountdown:
                if (__instance.Countdown > SabotageOptions.Instance.MiraReactorCountdown)
                    __instance.Countdown = SabotageOptions.Instance.MiraReactorCountdown;
                break;
            case ShipStatus.MapType.Pb when SabotageOptions.Instance.CustomPolusReactorCountdown:
                if (__instance.Countdown > SabotageOptions.Instance.PolusReactorCountdown)
                    __instance.Countdown = SabotageOptions.Instance.PolusReactorCountdown;
                break;
        }
    }
}