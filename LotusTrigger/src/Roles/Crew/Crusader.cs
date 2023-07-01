using AmongUs.GameOptions;
using Lotus;
using Lotus.API;
using Lotus.API.Odyssey;
using Lotus.API.Reactive.Actions;
using Lotus.Extensions;
using Lotus.Managers.History.Events;
using Lotus.Roles;
using Lotus.Roles.Builtins.Vanilla;
using Lotus.Roles.Interactions;
using Lotus.Roles.Interactions.Interfaces;
using Lotus.Roles.Interfaces;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Internals.Enums;
using Lotus.Roles.Overrides;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Options.Game;
using VentLib.Utilities.Optionals;
using static LotusTrigger.Roles.Crew.Crusader.CrusaderTranslations.CrusaderOptions;
using static Lotus.Utilities.TranslationUtil;

namespace LotusTrigger.Roles.Crew;

public class Crusader: Crewmate, ISabotagerRole
{
    private Optional<byte> protectedPlayer = Optional<byte>.Null();
    private bool protectAgainstHelpfulInteraction;
    private bool protectAgainstNeutralInteraction;

    [RoleAction(LotusActionType.Attack)]
    private void SelectTarget(PlayerControl target)
    {
        if (MyPlayer.InteractWith(target, LotusInteraction.HelpfulInteraction.Create(this)) == InteractionResult.Halt) return;
        protectedPlayer = Optional<byte>.NonNull(target.PlayerId);
        MyPlayer.RpcMark(target);
        Game.MatchData.GameHistory.AddEvent(new ProtectEvent(MyPlayer, target));
    }

    [RoleAction(LotusActionType.AnyInteraction)]
    private void AnyPlayerTargeted(PlayerControl killer, PlayerControl target, Interaction interaction, ActionHandle handle)
    {
        if (Game.State is not GameState.Roaming) return;
        if (killer.PlayerId == MyPlayer.PlayerId) return;
        if (!protectedPlayer.Exists()) return;
        if (target.PlayerId != protectedPlayer.Get()) return;
        Intent intent = interaction.Intent;

        switch (intent)
        {
            case IHelpfulIntent when !protectAgainstHelpfulInteraction:
            case INeutralIntent when !protectAgainstNeutralInteraction:
            case IFatalIntent fatalIntent when fatalIntent.IsRanged():
                return;
        }

        if (interaction is IDelayedInteraction or IRangedInteraction or IIndirectInteraction) return;

        handle.Cancel();
        RoleUtils.SwapPositions(target, MyPlayer);
        bool killed = MyPlayer.InteractWith(killer, LotusInteraction.FatalInteraction.Create(this)) is InteractionResult.Proceed;
        Game.MatchData.GameHistory.AddEvent(new PlayerSavedEvent(target, MyPlayer, killer));
        Game.MatchData.GameHistory.AddEvent(new KillEvent(MyPlayer, killer, killed));
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub.KeyName("Protect against Beneficial Interactions", Colorize(BeneficialInteractionProtection, ModConstants.Palette.PassiveColor))
                .BindBool(b => protectAgainstHelpfulInteraction = b)
                .AddOnOffValues(false)
                .Build())
            .SubOption(sub => sub.KeyName("Protect against Neutral Interactions", Colorize(NeutralInteractionProtection, ModConstants.Palette.NeutralColor))
                .BindBool(b => protectAgainstNeutralInteraction = b)
                .AddOnOffValues()
                .Build());

    protected override AbstractBaseRole.RoleModifier Modify(AbstractBaseRole.RoleModifier roleModifier) =>
        base.Modify(roleModifier)
            .DesyncRole(RoleTypes.Impostor)
            .RoleFlags(RoleFlag.CannotWinAlone)
            .RoleAbilityFlags(RoleAbilityFlag.CannotVent)
            .RoleColor(new Color(0.78f, 0.36f, 0.22f))
            .OptionOverride(new IndirectKillCooldown(() => AUSettings.KillCooldown()))
            .OptionOverride(Override.ImpostorLightMod, () => AUSettings.CrewLightMod());

    public bool CanSabotage() => false;

    [Localized(nameof(Crusader))]
    internal static class CrusaderTranslations
    {
        [Localized(ModConstants.Options)]
        public static class CrusaderOptions
        {
            [Localized(nameof(BeneficialInteractionProtection))]
            public static string BeneficialInteractionProtection = "Protect against Beneficial::0 Interactions";

            [Localized(nameof(NeutralInteractionProtection))]
            public static string NeutralInteractionProtection = "Protect against Neutral::0 Interactions";
        }
    }
}