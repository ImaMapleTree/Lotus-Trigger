using AmongUs.GameOptions;
using Lotus.API.Odyssey;
using Lotus.API.Reactive.Actions;
using Lotus.Factions;
using Lotus.Managers.History.Events;
using Lotus.Roles;
using Lotus.Roles.Builtins.Base;
using Lotus.Roles.Interactions;
using Lotus.Roles.Interfaces;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Internals.Enums;
using UnityEngine;

namespace LotusTrigger.Roles.Impostors;

public class Assassin: GuesserRoleBase, ISabotagerRole
{
    public bool CanSabotage() => true;

    [RoleAction(LotusActionType.Attack)]
    public virtual bool TryKill(PlayerControl target)
    {
        InteractionResult result = MyPlayer.InteractWith(target, LotusInteraction.FatalInteraction.Create(this));
        Game.MatchData.GameHistory.AddEvent(new KillEvent(MyPlayer, target, result is InteractionResult.Proceed));
        return result is InteractionResult.Proceed;
    }

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        roleModifier
            .RoleColor(Color.red)
            .Faction(FactionInstances.Impostors)
            .RoleAbilityFlags(RoleAbilityFlag.IsAbleToKill)
            .VanillaRole(RoleTypes.Impostor);
}