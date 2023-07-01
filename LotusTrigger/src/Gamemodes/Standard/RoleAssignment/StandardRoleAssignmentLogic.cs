extern alias JBAnnotations;
using System.Collections.Generic;
using System.Linq;
using Lotus;
using Lotus.API.Odyssey;
using Lotus.API.Player;
using Lotus.Extensions;
using Lotus.Factions.Impostors;
using Lotus.Options;
using Lotus.Roles;
using Lotus.Roles.Builtins;
using Lotus.Roles.Builtins.Base;
using Lotus.Roles.Builtins.Vanilla;
using Lotus.Roles.Interfaces;
using Lotus.Utilities;
using LotusTrigger.Gamemodes.Standard.Lotteries;
using LotusTrigger.Options;
using LotusTrigger.Options.General;
using LotusTrigger.Options.Roles;
using VentLib.Utilities.Extensions;

namespace LotusTrigger.Gamemodes.Standard.RoleAssignment;

public class StandardRoleAssignmentLogic
{
    private static List<IAdditionalAssignmentLogic> _additionalAssignmentLogics = new();
    
    public static void AddAdditionalAssignmentLogic(IAdditionalAssignmentLogic logic) => _additionalAssignmentLogics.Add(logic);

    public static void AssignRoles(List<PlayerControl> allPlayers)
    {
        List<PlayerControl> unassignedPlayers = new(allPlayers);
        unassignedPlayers.Shuffle();

        RoleDistribution distribution = GameplayOptions.Instance.OptimizeRoleAssignment
            ? OptimizeRoleAlgorithm.OptimizeDistribution()
            : OptimizeRoleAlgorithm.NonOptimizedDistribution();


        int j = 0;
        while (GeneralOptions.DebugOptions.NameBasedRoleAssignment && j < unassignedPlayers.Count)
        {
            PlayerControl player = unassignedPlayers[j];
            CustomRole? role = ProjectLotus.RoleManager.AllRoles.FirstOrDefault(r => r.RoleName.RemoveHtmlTags().ToLower().StartsWith(player.name.ToLower() ?? "HEHXD"));
            if (role != null && role.GetType() != typeof(Crewmate))
            {
                MatchData.AssignRole(player, role);
                unassignedPlayers.Pop(j);
            }
            else j++;
        }

        // ASSIGN IMPOSTOR ROLES
        ImpostorLottery impostorLottery = new();
        int impostorCount = 0;
        int madmateCount = 0;

        while ((impostorCount < distribution.Impostors || madmateCount < distribution.MinimumMadmates) && unassignedPlayers.Count > 0)
        {
            CustomRole role = impostorLottery.Next();
            if (role.GetType() == typeof(Impostor) && impostorLottery.HasNext()) continue;

            if (role.Faction is Madmates)
            {
                if (madmateCount >= distribution.MaximumMadmates) continue;
                MatchData.AssignRole(unassignedPlayers.PopRandom(), IVariableRole.PickAssignedRole(role));
                madmateCount++;
                if (MadmateOptions.Instance.MadmatesTakeImpostorSlots) impostorCount++;
                continue;
            }

            if (impostorCount >= distribution.Impostors)
            {
                if (!impostorLottery.HasNext()) break;
                continue;
            }

            MatchData.AssignRole(unassignedPlayers.PopRandom(), IVariableRole.PickAssignedRole(role));
            impostorCount++;
        }

        // =====================

        // ASSIGN NEUTRAL ROLES
        NeutralKillingLottery neutralKillingLottery = new();
        int nkRoles = 0;
        int loops = 0;
        while (unassignedPlayers.Count > 0 && nkRoles < distribution.MaximumNeutralKilling)
        {
            if (loops > 0 && nkRoles >= distribution.MinimumNeutralKilling) break;
            CustomRole role = neutralKillingLottery.Next();
            if (role is IllegalRole)
            {
                if (nkRoles > distribution.MaximumNeutralKilling || loops >= 10) break;
                loops++;
                if (!neutralKillingLottery.HasNext())
                    neutralKillingLottery = new NeutralKillingLottery(); // Refresh the lottery again to fulfill the minimum requirement
                continue;
            }
            MatchData.AssignRole(unassignedPlayers.PopRandom(), IVariableRole.PickAssignedRole(role));
            nkRoles++;
        }
        // --------------------------

        // ASSIGN NEUTRAL ROLES
        NeutralLottery neutralLottery = new();
        int neutralRoles = 0;
        loops = 0;
        while (unassignedPlayers.Count > 0 && neutralRoles < distribution.MaximumNeutralPassive)
        {
            if (loops > 0 && neutralRoles >= distribution.MaximumNeutralPassive) break;
            CustomRole role = neutralLottery.Next();
            if (role is IllegalRole)
            {
                if (neutralRoles > distribution.MinimumNeutralPassive || loops >= 10) break;
                loops++;
                if (!neutralLottery.HasNext())
                    neutralLottery = new NeutralLottery(); // Refresh the lottery again to fulfill the minimum requirement
                continue;
            }
            MatchData.AssignRole(unassignedPlayers.PopRandom(), IVariableRole.PickAssignedRole(role));
            neutralRoles++;
        }
        // --------------------------

        // DO ADDITIONAL LOGIC (ADDONS)
        foreach (IAdditionalAssignmentLogic additionalAssignmentLogic in _additionalAssignmentLogics)
            additionalAssignmentLogic.AssignRoles(allPlayers, unassignedPlayers);
        // ===================

        // ASSIGN CREWMATE ROLES
        CrewmateLottery crewmateLottery = new();
        while (unassignedPlayers.Count > 0)
        {
            CustomRole role = crewmateLottery.Next();
            if (role.GetType() == typeof(Crewmate) && crewmateLottery.HasNext()) continue;
            MatchData.AssignRole(unassignedPlayers.PopRandom(), role);
        }
        // ====================

        // ASSIGN SUB-ROLES
        AssignSubroles();
        // ================

        Players.GetPlayers().Sorted(p => p.IsHost() ? 0 : 1).ForEach(p => p.GetCustomRole().Assign());
    }

    private static void AssignSubroles()
    {
        SubRoleLottery subRoleLottery = new();

        int evenDistribution = SubroleOptions.Instance.EvenlyDistributeModifiers ? 0 : 9999;

        foreach (CustomRole role in subRoleLottery)
        {
            if (role is IllegalRole) continue;
            CustomRole variant = role is Subrole sr ? IVariantSubrole.PickAssignedRole(sr) : IVariableRole.PickAssignedRole(role);
            List<PlayerControl> players = Players.GetPlayers().Where(CanAssignTo).ToList();
            List<PlayerControl> otherPlayers = Players.GetPlayers().Where(p => !CanAssignTo(p)).ToList();
            if (players.Count == 0)
            {
                evenDistribution++;
                if (!SubroleOptions.Instance.UncappedModifiers && evenDistribution >= SubroleOptions.Instance.ModifierLimits) break;
                players = Players.GetPlayers().Where(p => p.GetSubroles().Count <= evenDistribution).ToList();;
                if (players.Count == 0) break;
            }

            bool assigned = false;
            while (players.Count > 0 && !assigned)
            {
                PlayerControl victim = players.PopRandom();
                if (victim.GetSubroles().Any(r => r.GetType() == variant.GetType())) continue;
                if (variant is ISubrole subrole && !(assigned = subrole.IsAssignableTo(victim))) continue;
                MatchData.AssignSubrole(victim, variant);
            }

            while (!assigned && otherPlayers.Count > 0)
            {
                PlayerControl victim = otherPlayers.PopRandom();
                if (victim.GetSubroles().Any(r => r.GetType() == variant.GetType())) continue;
                if (variant is ISubrole subrole && !(assigned = subrole.IsAssignableTo(victim))) continue;
                MatchData.AssignSubrole(victim, variant);
            }
        }

        return;

        bool CanAssignTo(PlayerControl player)
        {
            int count = player.GetSubroles().Count;
            if (count > evenDistribution) return false;
            return SubroleOptions.Instance.UncappedModifiers || count < SubroleOptions.Instance.ModifierLimits;
        }
    }
}