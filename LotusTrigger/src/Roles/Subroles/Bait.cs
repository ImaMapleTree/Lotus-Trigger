using Lotus.API.Odyssey;
using Lotus.API.Player;
using Lotus.API.Reactive.Actions;
using Lotus.Roles.Builtins.Base;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Internals.Enums;
using UnityEngine;
using VentLib.Options.Game;
using VentLib.Utilities.Optionals;

namespace LotusTrigger.Roles.Subroles;

public class Bait: Subrole
{
    private bool triggered;

    [RoleAction(LotusActionType.MyDeath)]
    private void BaitDies(PlayerControl killer, Optional<FrozenPlayer> realKiller)
    {
        if (triggered) return;
        if (Game.State is not GameState.Roaming) return;
        triggered = true;
        realKiller.FlatMap(rk => new UnityOptional<PlayerControl>(rk.MyPlayer)).OrElse(killer).ReportDeadBody(MyPlayer.Data);
    }

    public override string Identifier() => "★";

    protected override RoleModifier Modify(RoleModifier roleModifier) => base.Modify(roleModifier).RoleColor(new Color(0f, 0.7f, 0.7f));

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        AddRestrictToCrew(base.RegisterOptions(optionStream));
}