using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using Lotus;
using Lotus.API.Odyssey;
using Lotus.API.Reactive.Actions;
using Lotus.Extensions;
using Lotus.Factions;
using Lotus.GUI.Name;
using Lotus.GUI.Name.Components;
using Lotus.GUI.Name.Holders;
using Lotus.Logging;
using Lotus.Managers.History.Events;
using Lotus.Options.LotusImpl.Roles;
using Lotus.Roles;
using Lotus.Roles.Builtins.Base;
using Lotus.Roles.Interactions;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Internals.Enums;
using LotusTrigger.Options;
using LotusTrigger.Options.Roles;
using LotusTrigger.Roles.Crew;
using LotusTrigger.Roles.Neutral;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Options.Game;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;

namespace LotusTrigger.Roles.Subroles;

public class Bloodlust: Subrole
{
    public static Type[] IncompatibleRoles =
    {
        typeof(Crusader),
        typeof(Observer),
        typeof(Snitch),
        typeof(Speedrunner),
        typeof(Amnesiac),
        typeof(Copycat),
        typeof(Phantom)
    };

    private bool restrictedToCompatibleRoles;
    private static ColorGradient _psychoGradient = new(new Color(0.41f, 0.1f, 0.18f), new Color(0.85f, 0.77f, 0f));
    private bool requiresBaseKillMethod;


    public override string Identifier() => "";

    [RoleAction(LotusActionType.Attack)]
    private bool TryKill(PlayerControl target)
    {
        if (!requiresBaseKillMethod) return false;
        InteractionResult result = MyPlayer.InteractWith(target, LotusInteraction.FatalInteraction.Create(this));
        Game.MatchData.GameHistory.AddEvent(new KillEvent(MyPlayer, target, result is InteractionResult.Proceed));
        return result is InteractionResult.Proceed;
    }

    protected override void PostSetup()
    {
        CustomRole role = MyPlayer.GetCustomRole();
        RoleHolder roleHolder = MyPlayer.NameModel().GetComponentHolder<RoleHolder>();
        string newRoleName = _psychoGradient.Apply(role.RoleName);
        roleHolder.Add(new RoleComponent(new LiveString(newRoleName), Game.IgnStates, ViewMode.Replace, MyPlayer));
        role.Faction = FactionInstances.Neutral;
        if (role.RealRole.IsCrewmate())
        {
            // TODO make this abstracted so that people don't do dumb things
            role.DesyncRole = RoleTypes.Impostor;
            MyPlayer.GetTeamInfo().MyRole = role.DesyncRole.Value;
        }
        requiresBaseKillMethod = !role.GetActions(LotusActionType.Attack).Any();
    }

    public override bool IsAssignableTo(PlayerControl player)
    {
        DevLogger.Log($"Checking assignable conditions: {player.name}");
        // If the role is NOT a neutral killing role, then we immediately pass and it's legal
        if (player.GetCustomRole().SpecialType is not SpecialType.NeutralKilling) return base.IsAssignableTo(player);
        NeutralTeaming teaming = NeutralOptions.Instance.NeutralTeamingMode;

        // If neutral teaming is disabled we return false, because solo NKs should never get bloodlust
        // if neutral teaming is NP + NK or ALL then we pass as chances are, there'll be teaming there
        if (teaming is not NeutralTeaming.SameRole) return teaming is not NeutralTeaming.Disabled && base.IsAssignableTo(player);

        // This means neutral teaming is Same Role, so now we check if Max is 1. If max is 1, there'll never be a team so bloodlust is useless, return false
        return player.GetCustomRole().Count > 1 && base.IsAssignableTo(player);
    }

    public override HashSet<Type>? RestrictedRoles()
    {
        HashSet<Type>? restrictedRoles = base.RestrictedRoles();
        if (!restrictedToCompatibleRoles) return restrictedRoles;
        IncompatibleRoles.ForEach(r => restrictedRoles?.Add(r));
        return restrictedRoles;
    }

    public override CompatabilityMode RoleCompatabilityMode => CompatabilityMode.Blacklisted;

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        AddRestrictToCrew(base.RegisterOptions(optionStream))
            .SubOption(sub => sub.KeyName("Restrict to Compatible Roles", Translations.Options.RestrictToCompatbileRoles)
                .BindBool(b => restrictedToCompatibleRoles = b)
                .AddOnOffValues()
                .Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) => base.Modify(roleModifier).RoleColor(new Color(0.41f, 0.1f, 0.18f));

    [Localized(nameof(Bloodlust))]
    private static class Translations
    {
        [Localized(ModConstants.Options)]
        internal static class Options
        {
            public static string RestrictToCompatbileRoles = "Restrict to Compatible Roles";
        }
    }
}