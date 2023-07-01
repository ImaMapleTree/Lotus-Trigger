using System.Collections.Generic;
using System.Linq;
using Lotus.API.Odyssey;
using Lotus.API.Reactive.Actions;
using Lotus.Extensions;
using Lotus.GUI;
using Lotus.GUI.Name;
using Lotus.GUI.Name.Components;
using Lotus.GUI.Name.Holders;
using Lotus.Roles;
using Lotus.Roles.Builtins.Vanilla;
using Lotus.Roles.Interactions;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Internals.Enums;
using Lotus.Roles.Overrides;
using Lotus.Utilities;
using UnityEngine;
using VentLib.Localization;
using VentLib.Options.Game;
using VentLib.Utilities;
using VentLib.Utilities.Collections;

namespace LotusTrigger.Roles.Impostors;

public class YinYanger : Impostor
{
    [NewOnSetup] private Dictionary<byte, Remote<IndicatorComponent>> remotes;

    private PlayerControl? yinPlayer;
    private PlayerControl? yangPlayer;

    private bool lazyDefer;
    private FixedUpdateLock fixedUpdateLock = new();

    private bool InYinMode => yinPlayer == null || yangPlayer == null;

    [UIComponent(UI.Text)]
    private string ModeIndicator() => !InYinMode ? "" : Color.black.Colorize("Yin") + Color.white.Colorize("Yanging");

    [RoleAction(LotusActionType.Attack)]
    public override bool TryKill(PlayerControl target)
    {
        if (!InYinMode) return base.TryKill(target);
        if (MyPlayer.InteractWith(target, LotusInteraction.HostileInteraction.Create(this)) is InteractionResult.Halt) return false;
        if (yinPlayer != null && yinPlayer.PlayerId == target.PlayerId || yangPlayer != null && yangPlayer.PlayerId == target.PlayerId) return false;

        Color indicatorColor = yinPlayer == null ? Color.white : Color.black;
        IndicatorComponent component = new(new LiveString("☯", indicatorColor), GameState.Roaming, viewers: MyPlayer);

        remotes.GetValueOrDefault(target.PlayerId)?.Delete();
        remotes[target.PlayerId] = target.NameModel().GetComponentHolder<IndicatorHolder>().Add(component);

        if (yinPlayer == null) yinPlayer = target;
        else yangPlayer = target;

        SyncOptions();
        MyPlayer.RpcMark(target);
        return true;
    }


    [RoleAction(LotusActionType.MyDeath)]
    [RoleAction(LotusActionType.RoundEnd)]
    private void RoundEnd()
    {
        if (yinPlayer != null) remotes.GetValueOrDefault(yinPlayer.PlayerId)?.Delete();
        if (yangPlayer != null) remotes.GetValueOrDefault(yangPlayer.PlayerId)?.Delete();
        yinPlayer = null;
        yangPlayer = null;
    }

    [RoleAction(LotusActionType.FixedUpdate)]
    private void YinYangerKillCheck()
    {
        if (!fixedUpdateLock.AcquireLock()) return;
        if (yinPlayer == null || yangPlayer == null) return;

        if (yinPlayer.GetPlayersInAbilityRangeSorted().All(p => p.PlayerId != yangPlayer.PlayerId)) return;
        lazyDefer = true;
        yinPlayer.InteractWith(yangPlayer, new ManipulatedInteraction(new FatalIntent(), yinPlayer.GetCustomRole(), MyPlayer));
        yangPlayer.InteractWith(yinPlayer, new ManipulatedInteraction(new FatalIntent(), yangPlayer.GetCustomRole(), MyPlayer));
        lazyDefer = false;

        remotes.GetValueOrDefault(yinPlayer.PlayerId)?.Delete();
        remotes.GetValueOrDefault(yangPlayer.PlayerId)?.Delete();

        yinPlayer = null;
        yangPlayer = null;
    }

    [RoleAction(LotusActionType.Disconnect)]
    [RoleAction(LotusActionType.AnyDeath)]
    private void CheckPlayerDeaths(PlayerControl player)
    {
        if (lazyDefer) return;
        remotes.GetValueOrDefault(player.PlayerId)?.Delete();
        if (yinPlayer != null && yinPlayer.PlayerId == player.PlayerId) yinPlayer = null;
        else if (yangPlayer != null && yangPlayer.PlayerId == player.PlayerId) yangPlayer = null;
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        AddKillCooldownOptions(base.RegisterOptions(optionStream),
            key: "Yin Yang Cooldown", name: Localizer.Translate($"Roles.{nameof(YinYanger)}.YinYangCooldown", "Yin Yang Cooldown"));

    protected override AbstractBaseRole.RoleModifier Modify(AbstractBaseRole.RoleModifier roleModifier) =>
        base.Modify(roleModifier)
            .OptionOverride(new IndirectKillCooldown(KillCooldown, () => InYinMode));
}