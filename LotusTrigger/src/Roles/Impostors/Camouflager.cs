using System;
using System.Linq;
using HarmonyLib;
using Lotus.API.Player;
using Lotus.API.Reactive.Actions;
using Lotus.API.Vanilla.Meetings;
using Lotus.Extensions;
using Lotus.Options;
using Lotus.Roles.Builtins.Vanilla;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Internals.Enums;
using Lotus.RPC;
using VentLib.Options.Game;
using VentLib.Utilities;
using VentLib.Utilities.Optionals;

namespace LotusTrigger.Roles.Impostors;

public class Camouflager: Shapeshifter
{
    private bool canVent;
    private DateTime lastShapeshift;
    private DateTime lastUnshapeshift;
    private bool camouflaged;

    [RoleAction(LotusActionType.Attack)]
    public new bool TryKill(PlayerControl target) => base.TryKill(target);

    [RoleAction(LotusActionType.Shapeshift)]
    private void CamouflagerShapeshift(PlayerControl target)
    {
        if (camouflaged) return;
        camouflaged = true;
        Players.GetPlayers(PlayerFilter.Alive).Where(p => p.PlayerId != MyPlayer.PlayerId).Do(p => p.CRpcShapeshift(target, true));
    }

    [RoleAction(LotusActionType.Unshapeshift)]
    private void CamouflagerUnshapeshift(ActionHandle handle)
    {
        if (!camouflaged) return;
        camouflaged = false;
        Players.GetPlayers(PlayerFilter.Alive).Where(p => p.PlayerId != MyPlayer.PlayerId).Do(p => p.CRpcRevertShapeshift(true));
    }

    [RoleAction(LotusActionType.MeetingCalled)]
    private void HandleMeetingCall(PlayerControl reporter, Optional<GameData.PlayerInfo> reported, ActionHandle handle)
    {
        if (!camouflaged) return;
        camouflaged = false;
        Players.GetPlayers(PlayerFilter.Alive).Where(p => p.PlayerId != MyPlayer.PlayerId).Do(p => p.CRpcRevertShapeshift(true));
        handle.Cancel();
        Async.Schedule(() => MeetingPrep.PrepMeeting(reporter, reported.OrElse(null!)), 0.5f);
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub
                .Name("Camouflage Cooldown")
                .Bind(v => ShapeshiftCooldown = (float)v)
                .AddFloatRange(5, 120, 2.5f, 5, GeneralOptionTranslations.SecondsSuffix)
                .Build())
            .SubOption(sub => sub
                .Name("Camouflage Duration")
                .Bind(v => ShapeshiftDuration = (float)v)
                .AddFloatRange(5, 60, 2.5f, 5, GeneralOptionTranslations.SecondsSuffix)
                .Build())
            .SubOption(sub => sub
                .Name("Can Vent")
                .Bind(v => canVent = (bool)v)
                .AddOnOffValues()
                .Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier).CanVent(canVent);
}