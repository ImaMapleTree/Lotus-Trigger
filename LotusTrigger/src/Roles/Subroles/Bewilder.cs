using Lotus;
using Lotus.API.Odyssey;
using Lotus.API.Player;
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
using VentLib.Utilities.Optionals;

namespace LotusTrigger.Roles.Subroles;

public class Bewilder: Subrole
{
    private float visionMultiplier;
    public override string Identifier() => "<size=2.3>⁂</size>";

    [RoleAction(LotusActionType.MyDeath)]
    private void BewilderDies(PlayerControl killer, Optional<FrozenPlayer> realKiller)
    {
        if (realKiller.Exists()) killer = realKiller.Get().MyPlayer;

        GameOptionOverride optionOverride = killer.GetVanillaRole().IsImpostor()
            ? new MultiplicativeOverride(Override.ImpostorLightMod, visionMultiplier)
            : new MultiplicativeOverride(Override.CrewLightMod, visionMultiplier);

        Game.MatchData.Roles.AddOverride(killer.PlayerId, optionOverride);
        killer.GetCustomRole().SyncOptions();
        killer.NameModel().GCH<IndicatorHolder>().Add(new SimpleIndicatorComponent(Identifier(), RoleColor, Game.IgnStates, killer));

        CustomStatus status = CustomStatus.Of(Translations.BewilderedStatus).Description(Translations.BewilderedDescription).Color(RoleColor).StatusFlags(StatusFlag.Hidden).Build();
        MatchData.AddStatus(killer, status, MyPlayer);
    }


    protected override RoleModifier Modify(RoleModifier roleModifier) => base.Modify(roleModifier).RoleColor(new Color(0.42f, 0.28f, 0.2f));

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        AddRestrictToCrew(base.RegisterOptions(optionStream))
            .SubOption(sub => sub.KeyName("Vision Multiplier", Translations.Options.VisionMultiplier)
                .AddFloatRange(0.1f, 1f, 0.05f, 9, "x")
                .BindFloat(f => visionMultiplier = f)
                .Build());


    [Localized(nameof(Bewilder))]
    private static class Translations
    {
        [Localized(nameof(BewilderedStatus))]
        public static string BewilderedStatus = "Bewildered";

        [Localized(nameof(BewilderedDescription))]
        public static string BewilderedDescription = "The Bewildered status reduces your vision by a specific multiplier.";

        [Localized(ModConstants.Options)]
        public static class Options
        {
            public static string VisionMultiplier = "Vision Multiplier";
        }
    }

}