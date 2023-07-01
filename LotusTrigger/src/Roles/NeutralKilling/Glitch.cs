using System.Collections.Generic;
using System.Linq;
using Lotus.API.Odyssey;
using Lotus.API.Reactive.Actions;
using Lotus.Extensions;
using Lotus.GUI;
using Lotus.GUI.Name;
using Lotus.Options;
using Lotus.Roles;
using Lotus.Roles.Events;
using Lotus.Roles.Interactions;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Internals.Enums;
using LotusTrigger.Roles.Crew;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Options.Game;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;
using static LotusTrigger.Roles.Crew.Escort;

namespace LotusTrigger.Roles.NeutralKilling;

[Localized("Roles.Glitch")]
public class Glitch: NeutralKillingBase
{
    [Localized("ModeKilling")]
    private static string _glitchKillingMode = "Killing";

    [Localized("ModeHacking")]
    private static string _glitchHackingMode = "Hacking";

    private static Color textColor = new(0.17f, 0.68f, 0.15f);
    private float roleblockDuration;
    private bool hackingMode;

    [NewOnSetup] private Dictionary<byte, Escort.BlockDelegate> blockedPlayers;

    [UIComponent(UI.Text)]
    private string BlockingText() => textColor.Colorize(hackingMode ? _glitchHackingMode : _glitchKillingMode);

    [RoleAction(LotusActionType.OnPet)]
    private void SwitchModes() => hackingMode = !hackingMode;

    [RoleAction(LotusActionType.Attack)]
    public override bool TryKill(PlayerControl target)
    {
        if (!hackingMode) return base.TryKill(target);
        if (blockedPlayers.ContainsKey(target.PlayerId)) return false;

        if (MyPlayer.InteractWith(target, LotusInteraction.HostileInteraction.Create(this)) is InteractionResult.Halt) return false;

        blockedPlayers[target.PlayerId] = Escort.BlockDelegate.Block(target, MyPlayer, roleblockDuration);
        MyPlayer.RpcMark(target);
        Game.MatchData.GameHistory.AddEvent(new GenericTargetedEvent(MyPlayer, target, $"{RoleColor.Colorize(MyPlayer.name)} hacked {target.GetRoleColor().Colorize(target.name)}."));

        if (roleblockDuration > 0) Async.Schedule(() => blockedPlayers.Remove(target.PlayerId), roleblockDuration);
        return false;
    }

    [RoleAction(LotusActionType.RoundStart)]
    [RoleAction(LotusActionType.RoundEnd)]
    private void UnblockPlayers()
    {
        blockedPlayers.ToArray().ForEach(k =>
        {
            blockedPlayers.Remove(k.Key);
            k.Value.Delete();
        });
    }

    [RoleAction(LotusActionType.AnyPlayerAction)]
    private void BlockAction(PlayerControl source, ActionHandle handle, RoleAction action)
    {
        if (action.Blockable) Block(source, handle);
    }

    [RoleAction(LotusActionType.AnyEnterVent)]
    private void Block(PlayerControl source, ActionHandle handle)
    {
        BlockDelegate? blockDelegate = blockedPlayers.GetValueOrDefault(source.PlayerId);
        if (blockDelegate == null) return;

        handle.Cancel();
        blockDelegate.UpdateDelegate();
    }

    [RoleAction(LotusActionType.SabotageStarted)]
    private void BlockSabotage(PlayerControl caller, ActionHandle handle)
    {
        BlockDelegate? blockDelegate = blockedPlayers.GetValueOrDefault(caller.PlayerId);
        if (blockDelegate == null) return;

        handle.Cancel();
        blockDelegate.UpdateDelegate();
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub
                .Name("Hacking Duration")
                .BindFloat(v => roleblockDuration = v)
                .Value(v => v.Text("Until Meeting").Value(-1f).Build())
                .AddFloatRange(5, 120, 5, suffix: GeneralOptionTranslations.SecondsSuffix)
                .Build());

    protected override AbstractBaseRole.RoleModifier Modify(AbstractBaseRole.RoleModifier roleModifier) => base.Modify(roleModifier).RoleColor(Color.green);
}