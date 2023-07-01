using System;
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using Lotus.API.Odyssey;
using Lotus.API.Player;
using Lotus.API.Reactive.Actions;
using Lotus.Extensions;
using Lotus.GUI;
using Lotus.GUI.Name;
using Lotus.GUI.Name.Components;
using Lotus.Managers.History.Events;
using Lotus.Options;
using Lotus.Roles;
using Lotus.Roles.Builtins.Vanilla;
using Lotus.Roles.Events;
using Lotus.Roles.Interactions;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Internals.Enums;
using Lotus.Utilities;
using UnityEngine;
using VentLib.Logging;
using VentLib.Options.Game;
using VentLib.Utilities.Extensions;

namespace LotusTrigger.Roles.Crew;

public class Transporter : Crewmate
{
    private int totalTransports;
    private int transportsRemaining;
    private TextComponent textComponent;

    [UIComponent(UI.Cooldown)]
    private Cooldown transportCooldown;

    [UIComponent(UI.Counter)]
    private string RemainingTransportCounter() => RoleUtils.Counter(transportsRemaining, totalTransports);

    protected override void PostSetup()
    {
        VentLogger.Fatal($"Total Transports: {totalTransports}");
        transportsRemaining = totalTransports;
    }

    [RoleAction(LotusActionType.OnPet)]
    public void TransportSelect(ActionHandle handle)
    {
        if (this.transportsRemaining == 0 || !transportCooldown.IsReady()) return;

        List<PlayerControl> players = Players.GetPlayers(PlayerFilter.Alive).ToList();
        if (players.Count < 2) return;

        PlayerControl target1 = players.PopRandom();
        PlayerControl target2 = players.PopRandom();

        if (target1.PlayerId == target2.PlayerId) return;

        transportCooldown.Start();

        this.transportsRemaining--;
        if (target1.inVent) target1.MyPhysics.ExitAllVents();
        if (target2.inVent) target2.MyPhysics.ExitAllVents();

        target1.MyPhysics.ResetMoveState();
        target2.MyPhysics.ResetMoveState();

        Vector2 player1Position = target1.GetTruePosition();
        Vector2 player2Position = target2.GetTruePosition();

        if (target1.IsAlive())
            Utils.Teleport(target1.NetTransform, new Vector2(player2Position.x, player2Position.y + 0.3636f));
        if (target2.IsAlive())
            Utils.Teleport(target2.NetTransform, new Vector2(player1Position.x, player1Position.y + 0.3636f));

        target1.InteractWith(target2, new TransportInteraction(target1));
        target2.InteractWith(target1, new TransportInteraction(target2));

        target1.moveable = true;
        target2.moveable = true;
        target1.Collider.enabled = true;
        target2.Collider.enabled = true;
        target1.NetTransform.enabled = true;
        target2.NetTransform.enabled = true;

        Game.MatchData.GameHistory.AddEvent(new TransportedEvent(MyPlayer, target1, target2));
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .Tab(DefaultTabs.CrewmateTab)
            .SubOption(sub => sub.Name("Number of Transports")
                .Bind(v => this.totalTransports = (int)v)
                .Values(4, 5, 10, 15, 20, 25).Build())
            .SubOption(sub => sub.Name("Transport Cooldown")
                .Bind(v => this.transportCooldown.Duration = Convert.ToSingle((int)v))
                .Values(4, 10, 15, 20, 25, 30).Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier)
            .VanillaRole(RoleTypes.Crewmate)
            .RoleColor("#00EEFF");


    private class TransportedEvent : AbilityEvent, IMultiTargetEvent
    {
        private PlayerControl target1;
        private PlayerControl target2;

        public TransportedEvent(PlayerControl user, PlayerControl target1, PlayerControl target2) : base(user)
        {
            this.target1 = target1;
            this.target2 = target2;
        }

        public List<PlayerControl> Targets() => new() { target1, target2 };

        public PlayerControl Target1() => target1;

        public PlayerControl Target2() => target2;

        public override string Message() => $"{Game.GetName(target1)} and {Game.GetName(target2)} were transported by {Game.GetName(Player())}.";
    }

    public class TransportInteraction : LotusInteraction {
        public TransportInteraction(PlayerControl actor) : base(new NeutralIntent(), actor.GetCustomRole()) { }
    }
}