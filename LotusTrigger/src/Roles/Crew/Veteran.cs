using AmongUs.GameOptions;
using Lotus;
using Lotus.API.Odyssey;
using Lotus.API.Reactive.Actions;
using Lotus.Extensions;
using Lotus.Factions;
using Lotus.GUI;
using Lotus.GUI.Name;
using Lotus.Managers.History.Events;
using Lotus.Options;
using Lotus.Roles;
using Lotus.Roles.Builtins.Vanilla;
using Lotus.Roles.Interactions;
using Lotus.Roles.Interactions.Interfaces;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Internals.Enums;
using UnityEngine;
using VentLib.Options.Game;
using VentLib.Utilities;

namespace LotusTrigger.Roles.Crew;

public class Veteran : Crewmate
{
    [UIComponent(UI.Cooldown)]
    private Cooldown veteranCooldown;
    private Cooldown veteranDuration;

    private int totalAlerts;
    private int remainingAlerts;
    private bool canKillCrewmates;
    private bool canKillWhileTransported;
    private bool canKillRangedAttackers;

    protected override void Setup(PlayerControl player)
    {
        base.Setup(player);
        remainingAlerts = totalAlerts;
    }

    [UIComponent(UI.Counter)]
    private string VeteranAlertCounter() => RoleUtils.Counter(remainingAlerts, totalAlerts);

    [UIComponent(UI.Indicator)]
    private string GetAlertedString() => veteranDuration.IsReady() ? "" : RoleColor.Colorize("♣");

    [RoleAction(LotusActionType.OnPet)]
    public void AssumeAlert()
    {
        if (remainingAlerts <= 0 || veteranCooldown.NotReady() || veteranDuration.NotReady()) return;
        VeteranAlertCounter().DebugLog("Veteran Alert Counter: ");
        veteranDuration.Start();
        Async.Schedule(() => veteranCooldown.Start(), veteranDuration.Duration);
        remainingAlerts--;
    }

    [RoleAction(LotusActionType.Interaction)]
    private void VeteranInteraction(PlayerControl actor, Interaction interaction, ActionHandle handle)
    {
        if (veteranDuration.IsReady()) return;

        switch (interaction)
        {
            case Transporter.TransportInteraction when !canKillWhileTransported:
            case IRangedInteraction when !canKillRangedAttackers:
            case IDelayedInteraction:
            case IndirectInteraction:
                return;
        }

        if (actor.GetCustomRole().Faction.Relationship(this.Faction) is Relation.FullAllies && !canKillCrewmates) return;
        handle.Cancel();
        Game.MatchData.GameHistory.AddEvent(new VettedEvent(MyPlayer, actor));
        IDeathEvent deathEvent = new CustomDeathEvent(actor, MyPlayer, ModConstants.DeathNames.Parried);
        MyPlayer.InteractWith(actor, new LotusInteraction(new FatalIntent(interaction is not LotusInteraction, () => deathEvent), this));
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream).Color(RoleColor)
            .SubOption(sub => sub.Name("Number of Alerts")
                .Bind(v => totalAlerts = (int)v)
                .AddIntRange(1, 10, 1, 9).Build())
            .SubOption(sub => sub.Name("Alert Cooldown")
                .Bind(v => veteranCooldown.Duration = (float)v)
                .AddFloatRange(2.5f, 120, 2.5f, 5, GeneralOptionTranslations.SecondsSuffix)
                .Build())
            .SubOption(sub => sub.Name("Alert Duration")
                .Bind(v => veteranDuration.Duration = (float)v)
                .AddFloatRange(1, 20, 0.25f, 10, GeneralOptionTranslations.SecondsSuffix).Build())
            .SubOption(sub => sub.Name("Kill Crewmates")
                .Bind(v => canKillCrewmates = (bool)v)
                .AddOnOffValues().Build())
            .SubOption(sub => sub.Name("Kill While Transported")
                .Bind(v => canKillWhileTransported = (bool)v)
                .AddOnOffValues().Build())
            .SubOption(sub => sub.Name("Kill Ranged Attackers")
                .BindBool(v => canKillRangedAttackers = v)
                .AddOnOffValues().Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier)
            .VanillaRole(RoleTypes.Crewmate)
            .RoleColor(new Color(0.6f, 0.5f, 0.25f));





    private class VettedEvent : KillEvent, IRoleEvent
    {
        public VettedEvent(PlayerControl killer, PlayerControl victim) : base(killer, victim)
        {
        }
    }
}