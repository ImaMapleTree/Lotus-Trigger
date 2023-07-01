using Lotus;
using Lotus.API;
using Lotus.API.Odyssey;
using Lotus.API.Player;
using Lotus.API.Reactive.Actions;
using Lotus.Extensions;
using Lotus.Roles.Builtins.Base;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Internals.Enums;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Options.Game;
using VentLib.Utilities.Optionals;
using static LotusTrigger.TriggerAddon;

namespace LotusTrigger.Roles.Subroles;

public class Oblivious: Subrole
{
    public override string Identifier() => "⁈";

    private bool passOnDeath;

    [RoleAction(LotusActionType.MyDeath)]
    private void ObliviousDies(PlayerControl killer, Optional<FrozenPlayer> realKiller)
    {
        if (!passOnDeath) return;
        killer = realKiller.FlatMap(k => new UnityOptional<PlayerControl>(k.MyPlayer)).OrElse(killer);
        if (killer.GetSubrole<Oblivious>() == null) MatchData.AssignSubrole(killer, AddonInstance.Modifiers.Oblivious);
    }

    [RoleAction(LotusActionType.SelfReportBody, priority: Priority.VeryLow)]
    private void CancelReportBody(ActionHandle handle) => handle.Cancel();

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub.KeyName("Pass on Death", Translations.Options.PassOnDeath)
                .AddOnOffValues(false)
                .BindBool(b => passOnDeath = b)
                .Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier)
            .RoleColor(new Color(0.49f, 0.28f, 0.5f));

    [Localized(nameof(Oblivious))]
    private static class Translations
    {
        [Localized(ModConstants.Options)]
        public static class Options
        {
            [Localized(nameof(PassOnDeath))]
            public static string PassOnDeath = "Pass on Death";
        }
    }
}
