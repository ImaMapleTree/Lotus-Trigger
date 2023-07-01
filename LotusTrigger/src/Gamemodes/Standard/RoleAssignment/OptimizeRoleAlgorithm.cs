using Il2CppSystem;
using LotusTrigger.Options;
using LotusTrigger.Options.Roles;

namespace LotusTrigger.Gamemodes.Standard.RoleAssignment;

public class OptimizeRoleAlgorithm
{
    public static RoleDistribution OptimizeDistribution()
    {
        int impostorsMax = GameOptionsManager.Instance.CurrentGameOptions.NumImpostors;
        int totalPlayers = PlayerControl.AllPlayerControls.Count;
        int impostorCount = totalPlayers switch
        {
            <= 6 => 1,
            <= 11 => 2,
            _ => 3
        };
        impostorCount = Math.Min(impostorCount, impostorsMax);


        return new RoleDistribution
        {
            Impostors = impostorCount,
            MinimumNeutralPassive = NeutralOptions.Instance.MinimumNeutralPassiveRoles,
            MaximumNeutralPassive = NeutralOptions.Instance.MaximumNeutralPassiveRoles,
            MinimumNeutralKilling = NeutralOptions.Instance.MinimumNeutralKillingRoles,
            MaximumNeutralKilling = NeutralOptions.Instance.MaximumNeutralKillingRoles,
            MinimumMadmates = MadmateOptions.Instance.MadmatesTakeImpostorSlots ? 0 : MadmateOptions.Instance.MinimumMadmates,
            MaximumMadmates = MadmateOptions.Instance.MaximumMadmates
        };
    }

    public static RoleDistribution NonOptimizedDistribution()
    {
        return new RoleDistribution
        {
            Impostors = GameOptionsManager.Instance.CurrentGameOptions.NumImpostors,
            MinimumNeutralPassive = NeutralOptions.Instance.MinimumNeutralPassiveRoles,
            MaximumNeutralPassive = NeutralOptions.Instance.MaximumNeutralPassiveRoles,
            MinimumNeutralKilling = NeutralOptions.Instance.MinimumNeutralKillingRoles,
            MaximumNeutralKilling = NeutralOptions.Instance.MaximumNeutralKillingRoles,
            MinimumMadmates = MadmateOptions.Instance.MadmatesTakeImpostorSlots ? 0 : MadmateOptions.Instance.MinimumMadmates,
            MaximumMadmates = MadmateOptions.Instance.MaximumMadmates
        };
    }
}