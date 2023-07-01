using HarmonyLib;
using Lotus.API.Vanilla.Sabotages;
using Lotus.Patches.Systems;
using LotusTrigger.Options;
using LotusTrigger.Options.General;

namespace LotusTrigger.Patches.Systems;

[HarmonyPatch(typeof(LifeSuppSystemType), nameof(LifeSuppSystemType.Detoriorate))]
public static class DeteriorateOxygenPatch
{
    public static void Prefix(LifeSuppSystemType __instance)
    {
        if (SabotagePatch.CurrentSabotage?.SabotageType() is SabotageType.Oxygen)
            SabotagePatch.SabotageCountdown = __instance.Countdown;

        if (!__instance.IsActive) return;

        switch (ShipStatus.Instance.Type)
        {
            case ShipStatus.MapType.Ship when SabotageOptions.Instance.CustomSkeldOxygenCountdown:
                if (__instance.Countdown > SabotageOptions.Instance.SkeldOxygenCountdown)
                    __instance.Countdown = SabotageOptions.Instance.SkeldOxygenCountdown;
                break;
            case ShipStatus.MapType.Hq when SabotageOptions.Instance.CustomMiraOxygenCountdown:
                if (__instance.Countdown > SabotageOptions.Instance.MiraOxygenCountdown)
                    __instance.Countdown = SabotageOptions.Instance.MiraOxygenCountdown;
                break;
        }
    }
}