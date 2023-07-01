using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Lotus;
using Lotus.API.Odyssey;
using Lotus.API.Player;
using Lotus.API.Reactive.Actions;
using Lotus.Extensions;
using Lotus.Factions.Crew;
using Lotus.Factions.Impostors;
using Lotus.GUI.Name;
using Lotus.GUI.Name.Components;
using Lotus.GUI.Name.Holders;
using Lotus.Roles;
using Lotus.Roles.Builtins.Vanilla;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Internals.Enums;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Options.Game;
using VentLib.Utilities.Collections;
using VentLib.Utilities.Extensions;
using VentLib.Utilities.Optionals;
using static LotusTrigger.Roles.Crew.Snitch.SnitchTranslations.SnitchOptionTranslations;
using static Lotus.Utilities.TranslationUtil;

namespace LotusTrigger.Roles.Crew;

public class Snitch : Crewmate
{

    public bool SnitchCanTrackNk;

    public bool EvilHaveArrow;
    public bool SnitchHasArrow;
    public bool ArrowIsColored;

    public int SnitchWarningTasks = 2;

    [NewOnSetup] private List<Remote<IndicatorComponent>> indicatorComponents = null!;

    [RoleAction(LotusActionType.MyDeath)]
    private void ClearComponents() => indicatorComponents.ForEach(c => c.Delete());



    protected override void OnTaskComplete(Optional<NormalPlayerTask> _)
    {
        int remainingTasks = TotalTasks - TasksComplete;
        if (remainingTasks == SnitchWarningTasks)
        {
            PlayerControl[] trackablePlayers = Players.GetPlayers().Where(IsTrackable).ToArray();
            MyPlayer.NameModel().GetComponentHolder<IndicatorHolder>().Add(new IndicatorComponent(new LiveString("⚠", RoleColor), Game.IgnStates, viewers: trackablePlayers.AddItem(MyPlayer).ToArray()));
            if (EvilHaveArrow)
                trackablePlayers.ForEach(p =>
                {
                    LiveString liveString = new(() => RoleUtils.CalculateArrow(p, MyPlayer, RoleColor));
                    var remote = p.NameModel().GetComponentHolder<IndicatorHolder>().Add(new IndicatorComponent(liveString, GameState.Roaming, viewers: p));
                    indicatorComponents.Add(remote);
                });
        }

        if (remainingTasks != 0) return;
        Players.GetPlayers().Where(IsTrackable).ForEach(p =>
        {
            p.NameModel().GetComponentHolder<RoleHolder>().Components().ForEach(rc => rc.AddViewer(MyPlayer));

            if (!SnitchHasArrow) return;

            Color color = ArrowIsColored ? p.GetCustomRole().RoleColor : Color.white;
            LiveString liveString = new(() => RoleUtils.CalculateArrow(MyPlayer, p, color));
            var remote = MyPlayer.NameModel().GetComponentHolder<IndicatorHolder>().Add(new IndicatorComponent(liveString, GameState.Roaming, viewers: MyPlayer));
            indicatorComponents.Add(remote);
        });
    }

    private bool IsTrackable(PlayerControl player)
    {
        CustomRole role = player.GetCustomRole();
        if (role.Faction is ImpostorFaction) return true;
        if (!SnitchCanTrackNk) return false;
        return role.Faction is not Crewmates && role.RealRole.IsImpostor();
    }

    protected override AbstractBaseRole.RoleModifier Modify(AbstractBaseRole.RoleModifier roleModifier) => base.Modify(roleModifier).RoleColor(new Color(0.72f, 0.98f, 0.31f));

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        AddTaskOverrideOptions(base.RegisterOptions(optionStream)
            .SubOption(s => s.KeyName("Remaining Task Warning", RemainingTaskWarning)
                .AddIntRange(0, 10, 1, 2)
                .BindInt(i => SnitchWarningTasks = i)
                .Build())
            .SubOption(s => s.KeyName("Evil Have Arrow to Snitch", Colorize(EvilArrowToSnitch, RoleColor))
                .AddOnOffValues()
                .BindBool(b => EvilHaveArrow = b)
                .Build())
            .SubOption(s => s.KeyName("Enable Arrow for Snitch", Colorize(SnitchArrow, RoleColor))
                .BindBool(v => SnitchHasArrow = v)
                .AddOnOffValues()
                .ShowSubOptionPredicate(o => (bool)o)
                .SubOption(arrow => arrow.KeyName("Colored Arrow", ColoredArrow)
                    .BindBool(v => ArrowIsColored = v)
                    .AddOnOffValues()
                    .Build())
                .Build())
            .SubOption(s => s.KeyName("Snitch Can Track Any Killing", Colorize(SnitchTracksAllKillers, RoleColor))
                .BindBool(v => SnitchCanTrackNk = v)
                .AddOnOffValues()
                .Build()));


    [Localized(nameof(Snitch))]
    internal static class SnitchTranslations
    {
        [Localized(ModConstants.Options)]
        internal static class SnitchOptionTranslations
        {
            [Localized(nameof(RemainingTaskWarning))]
            public static string RemainingTaskWarning = "Remaining Task Warning";

            [Localized(nameof(EvilArrowToSnitch))]
            public static string EvilArrowToSnitch = "Evil Have Arrow to Snitch::0";

            [Localized(nameof(SnitchArrow))]
            public static string SnitchArrow = "Enable Arrow for Snitch::0";

            [Localized(nameof(ColoredArrow))]
            public static string ColoredArrow = "Colored Arrow";

            [Localized(nameof(SnitchTracksAllKillers))]
            public static string SnitchTracksAllKillers = "Snitch::0 Can Track Any Killing";
        }
    }
}