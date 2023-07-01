using Lotus.API.Reactive.Actions;
using Lotus.Factions;
using Lotus.Roles;
using Lotus.Roles.Builtins.Vanilla;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Internals.Enums;
using UnityEngine;
using VentLib.Options.Game;

namespace LotusTrigger.Roles.Madmates.Roles;

public class Parasite : Shapeshifter
{
    [RoleAction(LotusActionType.Attack)]
    public override bool TryKill(PlayerControl target) => base.TryKill(target);

    public override bool CanSabotage() => true;

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) => AddKillCooldownOptions(base.RegisterOptions(optionStream));

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier)
            .RoleFlags(RoleFlag.CannotWinAlone)
            .SpecialType(SpecialType.Madmate)
            .RoleColor(new Color(0.73f, 0.18f, 0.02f))
            .Faction(FactionInstances.Madmates);
}