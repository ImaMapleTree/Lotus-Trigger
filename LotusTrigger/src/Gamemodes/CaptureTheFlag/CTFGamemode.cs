/*using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Lotus.API;
using Lotus.Extensions;
using Lotus.GUI;
using Lotus.GUI.Name;
using Lotus.Managers;
using Lotus.Roles;
using Lotus.Victory;
using UnityEngine;
using VentLib.Anticheat;
using VentLib.Logging;
using VentLib.Networking.RPC;
using VentLib.Options.Game;
using VentLib.Options.Game.Tabs;
using VentLib.Utilities;

namespace Lotus.Gamemodes.CaptureTheFlag;

public class CTFGamemode: Gamemode
{
    public static float GameDuration = 300;
    // Red Team // Blue Team
    public static Vector2[] SpawnLocations = { new(-20.625f, -5.5f), new(16.425f, -4.8f) };
    public static Vector2[] BodyLocations = { new(-20.5f, -5.5f), new(16.5f, -4.8f) };
    public static int[] TeamPoints = { 0, 0 };
    public static byte[] Carriers = { 255, 255 };

    public static GameOptionTab CTFTab = new("Capture The Flag", () => Utils.LoadSprite("Lotus.assets.Tabs.TabIcon_CaptureTheFlag.png"));
    public override string GetName() => "Capture The Flag";
    public override IEnumerable<GameOptionTab> EnabledTabs() => new[] { CTFTab };
    public override GameAction IgnoredActions() => GameAction.CallSabotage | GameAction.CallMeeting;

    public static Striker Striker = new Striker();

    public CTFGamemode()
    {
        this.BindAction(GameAction.GameStart, SetupNames);

        AddOption(new GameOptionBuilder()
            .Name("Game Length")
            .Tab(CTFTab)
            .IsHeader(true)
            .BindFloat(v => GameDuration = v)
            .AddFloatRange(60, 600, 15f, 3, GeneralOptionTranslations.SecondsSuffix)
            .Build());
    }

    public override void Activate()
    {
        CustomRoleManager.AddRole(Striker);
    }

    public override void Deactivate()
    {
        CustomRoleManager.AllRoles.Remove(Striker);
    }

    // new(16.5f, -4.8f) Navigation
    public override void AssignRoles(List<PlayerControl> players)
    {
        PlayerControl host = players.FirstOrDefault(p => p.IsHost())!;
        CTFAssignRoles.AssignRoles(players);

        int hostColor = host.cosmetics.bodyMatProperties.ColorId;

        for (int i = 0; i < 20; i++)
        {
            var i1 = i % 2;
            Async.Schedule(() =>
            {
                VentLogger.Fatal("Setting Color", "CTFAsync");
                SetColorBypass.SetColor(host, i1);
                Utils.Teleport(host.NetTransform, BodyLocations[i1]);

                Async.Schedule(() =>
                {
                    host.RpcMurderPlayer(host);
                    host.Data.IsDead = false;
                    GeneralRPC.SendGameData();
                }, NetUtils.DeriveDelay(0.1f));

            }, NetUtils.DeriveDelay(NetUtils.DeriveDelay(0.5f) + (NetUtils.DeriveDelay(0.08f) * i)));
        }

        Async.Schedule(() =>
        {
            VentLogger.Fatal("Reviving Host");
            SetColorBypass.SetColor(host, hostColor);
            Utils.Teleport(host.NetTransform, SpawnLocations[hostColor]);
            host.Revive();
            GeneralRPC.SendGameData();
            VentLogger.Fatal("CTF Async Setup Complete");
            Carriers = new byte[] { 255, 255 };
        }, 6f);
    }

    public static void GrabFlag(PlayerControl grabber)
    {
        int myTeam = grabber.cosmetics.bodyMatProperties.ColorId;
        Players.GetPlayers().Where(p => p.cosmetics.bodyMatProperties.ColorId != myTeam).Do(p =>
        {
            RoleUtils.PlayReactorsForPlayer(p);
            Async.Schedule(() => RoleUtils.EndReactorsForPlayer(p), 0.3f);
        });
        Carriers[myTeam] = grabber.PlayerId;

        string team = myTeam == 0 ? "Red" : "Blue";
        VentLogger.Info($"{grabber.name} (Team: {team}) Has stolen the flag");
    }

    public override void SetupWinConditions(WinDelegate winDelegate)
    {
        winDelegate.AddWinCondition(new CTFMostPointWinCondition());
    }

    private void SetupNames()
    {
        Players.GetPlayers().Do(p =>
        {
            byte playerId = p.PlayerId;
            int team = p.cosmetics.bodyMatProperties.ColorId;
            DynamicName name = p.GetDynamicName();
            name.AddRule(GameState.Roaming, UI.Role);
            name.AddRule(GameState.Roaming, UI.Subrole);
            name.AddRule(GameState.Roaming, UI.Counter);
            name.AddRule(GameState.Roaming, UI.Misc);
            name.SetComponentValue(UI.Subrole, new DynamicString(() => RoleUtils.Counter(TeamPoints[team])));
            name.SetComponentValue(UI.Name, new DynamicString(() => playerId == Carriers[team] ? Color.green.Colorize(name.RawName) : Color.white.Colorize(name.RawName)));
            Players.GetPlayers().Where(p2 => p2.cosmetics.bodyMatProperties.ColorId == team).Do(p2 =>
            {
                name.AddRule(GameState.Roaming, UI.Name, new DynamicString(() => playerId == Carriers[team] ? Color.green.Colorize("{0}") : ""), p2.PlayerId);
            });
            Players.GetPlayers().Where(p2 => p2.cosmetics.bodyMatProperties.ColorId != team).Do(p2 =>
            {
                name.AddRule(GameState.Roaming, UI.Name, new DynamicString(() => playerId == Carriers[team] ? Color.red.Colorize("{0}") : ""), p2.PlayerId);
            });
        });
    }

    /*public override void FixedUpdate()
    {
        PlayerControl? redCarrier = RoleUtils.GetPlayersWithinDistance(SpawnLocations[0], 2f).FirstOrDefault(p => Carriers[0] == p.PlayerId && p.cosmetics.bodyMatProperties.ColorId != 1);
        PlayerControl? redPlayerOntop = RoleUtils.GetPlayersWithinDistance(BodyLocations[0], 0.5f).FirstOrDefault(p => Carriers[0] == 255 && p.cosmetics.bodyMatProperties.ColorId != 0);
        if (redCarrier != null) TeamScoredPoint(0);
        if (redPlayerOntop != null) GrabFlag(redPlayerOntop);
        PlayerControl? blueCarrier = RoleUtils.GetPlayersWithinDistance(SpawnLocations[1], 2f).FirstOrDefault(p => Carriers[1] == p.PlayerId && p.cosmetics.bodyMatProperties.ColorId != 0);
        PlayerControl? bluePlayerOntop = RoleUtils.GetPlayersWithinDistance(BodyLocations[1], 0.5f).FirstOrDefault(p => Carriers[1] == 255 && p.cosmetics.bodyMatProperties.ColorId != 1);
        if (blueCarrier != null) TeamScoredPoint(1);
        if (bluePlayerOntop != null) GrabFlag(bluePlayerOntop);
    }#1#
}*/