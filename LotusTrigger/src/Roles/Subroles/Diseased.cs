using Lotus;
using Lotus.API.Odyssey;
using Lotus.API.Reactive.Actions;
using Lotus.Extensions;
using Lotus.GUI.Name.Components;
using Lotus.GUI.Name.Holders;
using Lotus.Roles.Builtins.Base;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Internals.Enums;
using Lotus.Roles.Overrides;
using Lotus.Statuses;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Options.Game;

namespace LotusTrigger.Roles.Subroles;

public class Diseased: Subrole
{
    private int cooldownIncrease;

    public override string Identifier() => "§";

    [RoleAction(LotusActionType.MyDeath)]
    private void DiseasedDies(PlayerControl killer)
    {
        float multiplier = (cooldownIncrease / 100f) + 1f;
        Game.MatchData.Roles.AddOverride(killer.PlayerId, new MultiplicativeOverride(Override.KillCooldown, multiplier));
        killer.GetCustomRole().SyncOptions();
        killer.NameModel().GCH<IndicatorHolder>().Add(new SimpleIndicatorComponent(Identifier(), RoleColor, Game.IgnStates, killer));

        CustomStatus status = CustomStatus.Of(RoleName).Description(Translations.DiseasedAffectDescription).Color(RoleColor).Build();
        MatchData.AddStatus(killer, status, MyPlayer);
    }

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier).RoleColor(new Color(0.42f, 0.4f, 0.16f));

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        AddRestrictToCrew(base.RegisterOptions(optionStream))
            .SubOption(sub => sub.KeyName("Cooldown Increase", Translations.Options.CooldownIncrease)
                .AddIntRange(0, 100, 5, 20, "%")
                .BindInt(i => cooldownIncrease = i)
                .Build());

    [Localized(nameof(Diseased))]
    private static class Translations
    {
        [Localized(nameof(DiseasedAffectDescription))]
        public static string DiseasedAffectDescription = "The Diseased status reduces your vision by a specific multiplier.";

        [Localized(ModConstants.Options)]
        public static class Options
        {
            [Localized(nameof(CooldownIncrease))]
            public static string CooldownIncrease = "Cooldown Increase";
        }
    }
}