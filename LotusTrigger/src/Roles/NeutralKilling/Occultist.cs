﻿using System.Collections.Generic;
using Lotus;
using Lotus.API.Odyssey;
using Lotus.API.Player;
using Lotus.API.Reactive.Actions;
using Lotus.Extensions;
using Lotus.GUI;
using Lotus.GUI.Name;
using Lotus.GUI.Name.Components;
using Lotus.GUI.Name.Holders;
using Lotus.Managers.History.Events;
using Lotus.Roles;
using Lotus.Roles.Events;
using Lotus.Roles.Interactions;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Internals.Enums;
using Lotus.Roles.Overrides;
using Lotus.Statuses;
using Lotus.Utilities;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Options.Game;
using VentLib.Utilities;
using VentLib.Utilities.Collections;
using VentLib.Utilities.Extensions;
using VentLib.Utilities.Optionals;

namespace LotusTrigger.Roles.NeutralKilling;

public class Occultist: NeutralKillingBase
{
    private bool freelySwitchModes;
    private bool switchModesAfterAttack;

    [NewOnSetup] private Dictionary<byte, Remote<IndicatorComponent>> indicators;
    [NewOnSetup] private Dictionary<byte, Remote<IStatus>?> cursedPlayers;

    private bool isCursingMode = true;

    [UIComponent(UI.Text)]
    private string ModeDisplay() => freelySwitchModes ? isCursingMode ? RoleColor.Colorize(Translations.CursingModeText) : Color.red.Colorize(Translations.KillingModeText) : "";

    [RoleAction(LotusActionType.Attack)]
    public override bool TryKill(PlayerControl target)
    {
        if (!isCursingMode)
        {
            if (switchModesAfterAttack) isCursingMode = !isCursingMode;
            return base.TryKill(target);
        }

        MyPlayer.RpcMark(target);
        if (switchModesAfterAttack) isCursingMode = !isCursingMode;
        if (MyPlayer.InteractWith(target, LotusInteraction.HostileInteraction.Create(this)) is InteractionResult.Halt) return false;
        if (cursedPlayers.ContainsKey(target.PlayerId)) return false;

        CustomStatus status = CustomStatus.Of(Translations.HexedCauseOfDeath).Description(Translations.CursedStatusDescription).Color(Color.red).Build();
        cursedPlayers.Add(target.PlayerId,  MatchData.AddStatus(target, status, MyPlayer));
        indicators.GetValueOrDefault(target.PlayerId)?.Delete();
        indicators[target.PlayerId] = target.NameModel().GCH<IndicatorHolder>().Add(new SimpleIndicatorComponent("†", Color.red, GameState.InMeeting));

        string eventMessage = TranslationUtil.Colorize(Translations.CursedMessage.Formatted(MyPlayer.name, target.name), RoleColor, target.GetCustomRole().RoleColor);
        Game.MatchData.GameHistory.AddEvent(new GenericTargetedEvent(MyPlayer, target, eventMessage));
        return false;
    }

    [RoleAction(LotusActionType.OnPet)]
    public void SwitchWitchMode()
    {
        if (freelySwitchModes) isCursingMode = !isCursingMode;
    }

    [RoleAction(LotusActionType.MeetingEnd)]
    public void KillCursedPlayers(Optional<GameData.PlayerInfo> exiledPlayer)
    {
        if (exiledPlayer.Compare(p => p.PlayerId == MyPlayer.PlayerId)) return;
        cursedPlayers.Keys.Filter(Players.PlayerById).ForEach(p =>
        {
            IDeathEvent cod = new CustomDeathEvent(MyPlayer, p, Translations.HexedCauseOfDeath);
            MyPlayer.InteractWith(p, new UnblockedInteraction(new FatalIntent(false, () => cod), this));
            cursedPlayers[p.PlayerId]?.Delete();
        });
        cursedPlayers.Clear();
        indicators.ForEach(i => i.Value.Delete());
        indicators.Clear();
    }

    [RoleAction(LotusActionType.MyDeath)]
    [RoleAction(LotusActionType.SelfExiled)]
    public override void HandleDisconnect()
    {
        indicators.ForEach(i => i.Value.Delete());
        cursedPlayers.Clear();
        indicators.Clear();
    }


    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub.KeyName("Freely Switch Modes", Translations.Options.FreelySwitchModes)
                .AddOnOffValues()
                .BindBool(b => freelySwitchModes = b)
                .Build())
            .SubOption(sub => sub.KeyName("Switch Modes After Attack", Translations.Options.SwitchModesAfterAttack)
                .AddOnOffValues()
                .BindBool(b => switchModesAfterAttack = b)
                .Build());

    protected override AbstractBaseRole.RoleModifier Modify(AbstractBaseRole.RoleModifier roleModifier) =>
        base.Modify(roleModifier)
            .RoleColor(new Color(0.39f, 0.47f, 0.87f))
            .RoleAbilityFlags(RoleAbilityFlag.CannotSabotage)
            .OptionOverride(new IndirectKillCooldown(KillCooldown, () => isCursingMode));


    [Localized(nameof(Occultist))]
    private static class Translations
    {
        [Localized(nameof(CursedStatusDescription))]
        public static string CursedStatusDescription = "You have been hexed. Hexed players will die after the meeting unless the source of the hex is voted out.";

        [Localized(nameof(CursedMessage))]
        public static string CursedMessage = "{0}::0 cursed {1}::1 to die at the end of next meeting.";

        [Localized(nameof(CursingModeText))]
        public static string CursingModeText = "Cursing";

        [Localized(nameof(KillingModeText))]
        public static string KillingModeText = "Killing";

        [Localized(nameof(HexedCauseOfDeath))]
        public static string HexedCauseOfDeath = "Hexed";

        [Localized(ModConstants.Options)]
        public static class Options
        {
            [Localized(nameof(FreelySwitchModes))]
            public static string FreelySwitchModes = "Freely Switch Modes";

            [Localized(nameof(SwitchModesAfterAttack))]
            public static string SwitchModesAfterAttack = "Switch Modes After Attack";
        }
    }

}