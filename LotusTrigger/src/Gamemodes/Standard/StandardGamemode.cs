using System.Collections.Generic;
using System.Linq;
using Lotus.API;
using Lotus.API.Odyssey;
using Lotus.API.Player;
using Lotus.API.Reactive;
using Lotus.API.Reactive.HookEvents;
using Lotus.Chat.Commands;
using Lotus.Extensions;
using Lotus.Factions;
using Lotus.Factions.Neutrals;
using Lotus.Gamemodes;
using Lotus.Options;
using Lotus.Options.LotusImpl.Roles;
using Lotus.Victory;
using Lotus.Victory.Conditions;
using LotusTrigger.Gamemodes.Standard.RoleAssignment;
using LotusTrigger.Gamemodes.Standard.WinCons;
using LotusTrigger.Options.General;
using LotusTrigger.Roles.Undead;
using VentLib.Localization.Attributes;
using VentLib.Logging;
using VentLib.Options.Game.Tabs;
using VentLib.Utilities.Extensions;
using GeneralOptions = LotusTrigger.Options.GeneralOptions;

namespace LotusTrigger.Gamemodes.Standard;

[Localized("Gamemode")]
public partial class StandardGamemode: Gamemode
{
    private const string StandardGamemodeHookKey = nameof(StandardGamemodeHookKey);

    [Localized(nameof(Name))]
    private static string _gamemodeName = "Standard";
    
    public override string Name { get; set; } = _gamemodeName;
    
    public override void Setup()
    {
        Game.GetWinDelegate().AddSubscriber(FixNeutralTeamingWinners);
    }

    public override void AssignRoles(List<PlayerControl> players)
    {
        StandardRoleAssignmentLogic.AssignRoles(players);
    }

    public override IEnumerable<GameOptionTab> EnabledTabs() => DefaultTabs.All;

    public override void SetupWinConditions(WinDelegate winDelegate)
    {
        winDelegate.AddWinCondition(new VanillaCrewmateWin());
        winDelegate.AddWinCondition(new VanillaImpostorWin());
        winDelegate.AddWinCondition(new SabotageWin());
        winDelegate.AddWinCondition(new StandardWinConditions.LoversWin());
        winDelegate.AddWinCondition(new SoloKillingWinCondition());
        winDelegate.AddWinCondition(new StandardWinConditions.SoloRoleWin());
        winDelegate.AddWinCondition(new UndeadWinCondition());
    }

    public override void Activate()
    {
        Hooks.PlayerHooks.PlayerDeathHook.Bind(StandardGamemodeHookKey, ShowInformationToGhost, priority: Priority.VeryLow);
    }

    public override void Deactivate()
    {
        Hooks.UnbindAll(StandardGamemodeHookKey);
    }

    public static void ShowInformationToGhost(PlayerDeathHookEvent hookEvent)
    {
        PlayerControl player = hookEvent.Player;
        ShowInformationToGhost(player);
    }

    public static void ShowInformationToGhost(PlayerControl player)
    {
        if (player == null) return;

        VentLogger.Trace($"Showing all name components to ghost {player.name}", "GhostNameViewer");
        if (MiscellaneousOptions.Instance.AutoDisplayCOD)
        {
            FrozenPlayer? fp = Game.MatchData.FrozenPlayers.GetValueOrDefault(player.GetGameID());
            if (fp != null) DeathCommand.ShowMyDeath(player, fp);
        }


        Players.GetPlayers().Where(p => p.PlayerId != player.PlayerId)
            .SelectMany(p => p.NameModel().ComponentHolders())
            .ForEach(holders =>
                {
                    holders.AddListener(component => component.AddViewer(player));
                    holders.Components().ForEach(components => components.AddViewer(player));
                }
            );

        player.NameModel().Render(force: true);
    }

    private static void FixNeutralTeamingWinners(WinDelegate winDelegate)
    {
        if (RoleOptions.NeutralOptions.NeutralTeamingMode is NeutralTeaming.Disabled) return;
        if (winDelegate.GetWinners().Count != 1) return;
        List<PlayerControl> winners = winDelegate.GetWinners();
        PlayerControl winner = winners[0];
        if (winner.GetCustomRole().Faction is not Neutral) return;

        winners.AddRange(Players.GetPlayers()
            .Where(p => p.PlayerId != winner.PlayerId)
            .Where(p => winner.Relationship(p) is Relation.SharedWinners or Relation.FullAllies));
    }
}