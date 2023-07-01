using Lotus.API.Odyssey;
using Lotus.API.Reactive.Actions;
using Lotus.Extensions;
using Lotus.GUI;
using Lotus.GUI.Name;
using Lotus.GUI.Name.Holders;
using Lotus.Options;
using Lotus.Roles;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Internals.Enums;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Logging;
using VentLib.Options.Game;
using VentLib.Utilities;

namespace LotusTrigger.Roles.NeutralKilling;

[Localized("Roles.Werewolf")]
public class Werewolf: NeutralKillingBase
{
    private bool rampaging;
    private bool canVentNormally;
    private bool canVentDuringRampage;

    [UIComponent(UI.Cooldown)]
    private Cooldown rampageDuration;

    [UIComponent(UI.Cooldown)]
    private Cooldown rampageCooldown;

    [Localized("Rampage")]
    private string rampagingString = "RAMPAGING";

    protected override void PostSetup()
    {
        base.PostSetup();
        MyPlayer.NameModel().GetComponentHolder<CooldownHolder>()[1].SetPrefix(RoleColor.Colorize(rampagingString + " "));
    }

    [RoleAction(LotusActionType.Attack)]
    public new bool TryKill(PlayerControl target) => rampaging && base.TryKill(target);

    [RoleAction(LotusActionType.OnPet)]
    private void EnterRampage()
    {
        if (rampageDuration.NotReady() || rampageCooldown.NotReady()) return;
        VentLogger.Trace($"{MyPlayer.GetNameWithRole()} Starting Rampage");
        rampaging = true;
        rampageDuration.Start();
        Async.Schedule(ExitRampage, rampageDuration.Duration);
    }

    [RoleAction(LotusActionType.RoundEnd)]
    private void ExitRampage()
    {
        VentLogger.Trace($"{MyPlayer.GetNameWithRole()} Ending Rampage");
        rampaging = false;
        rampageCooldown.Start();
    }

    public override bool CanVent() => canVentNormally || rampaging;

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub.Name("Rampage Kill Cooldown")
                .AddFloatRange(1f, 60f, 2.5f, 2, GeneralOptionTranslations.SecondsSuffix)
                .BindFloat(f => KillCooldown = f)
                .Build())
            .SubOption(sub => sub.Name("Rampage Cooldown")
                .AddFloatRange(5f, 120f, 2.5f, 14, GeneralOptionTranslations.SecondsSuffix)
                .BindFloat(rampageCooldown.SetDuration)
                .Build())
            .SubOption(sub => sub.Name("Rampage Duration")
                .AddFloatRange(5f, 120f, 2.5f, 4, GeneralOptionTranslations.SecondsSuffix)
                .BindFloat(rampageDuration.SetDuration)
                .Build())
            .SubOption(sub => sub.Name("Can Vent Normally")
                .AddOnOffValues(false)
                .BindBool(b => canVentNormally = b)
                .ShowSubOptionPredicate(o => !(bool)o)
                .SubOption(sub2 => sub2.Name("Can Vent in Rampage")
                    .BindBool(b => canVentDuringRampage = b)
                    .AddOnOffValues()
                    .Build())
                .Build());

    protected override AbstractBaseRole.RoleModifier Modify(AbstractBaseRole.RoleModifier roleModifier)
    {
        RoleAbilityFlag flags = RoleAbilityFlag.CannotSabotage;
        if (!(canVentNormally || canVentDuringRampage)) flags |= RoleAbilityFlag.CannotVent;

        return base.Modify(roleModifier)
            .RoleAbilityFlags(flags)
            .RoleColor(new Color(0.66f, 0.4f, 0.16f));
    }
}