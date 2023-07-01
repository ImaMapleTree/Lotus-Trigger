using HarmonyLib;
using Lotus.API.Vanilla.Sabotages;
using LotusTrigger.Options;
using LotusTrigger.Options.General;

namespace LotusTrigger.Patches.Systems;

[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CloseDoorsOfType))]
public class CloseDoorPatch
{
    public static bool Prefix(ShipStatus __instance, [HarmonyArgument(0)] SystemTypes room)
    {
        return !SabotageOptions.Instance.DisabledSabotages.HasFlag(SabotageType.Door);
    }
}