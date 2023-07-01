using Lotus.API;
using Lotus.API.Reactive.Actions;
using Lotus.Extensions;
using Lotus.GUI;
using Lotus.Options;
using Lotus.Roles;
using Lotus.Roles.Builtins.Vanilla;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Internals.Enums;
using Lotus.Roles.Overrides;
using VentLib.Options.Game;

namespace LotusTrigger.Roles.Impostors;

public class Freezer : Shapeshifter
{
    private PlayerControl currentFreezerTarget;
    private float freezeCooldown;
    private Cooldown freezeDuration;
    private bool canVent;

    [RoleAction(LotusActionType.Attack)]
    public override bool TryKill(PlayerControl target) => base.TryKill(target);

    [RoleAction(LotusActionType.SelfReportBody)]
    [RoleAction(LotusActionType.AnyReportedBody)]
    private void OnBodyReport()
    {
        if (currentFreezerTarget != null)
            ResetSpeed();
    }

    [RoleAction(LotusActionType.MyDeath)]
    [RoleAction(LotusActionType.SelfExiled)]
    private void OnExile()
    {
        if (currentFreezerTarget != null)
            ResetSpeed();
    }

    [RoleAction(LotusActionType.Shapeshift)]
    private void OnShapeshift(PlayerControl target)
    {
        if (freezeDuration.NotReady()) return;
        freezeDuration.Start();
        GameOptionOverride[] overrides = { new(Override.PlayerSpeedMod, 0.0001f) };
        target.GetCustomRole().SyncOptions(overrides);
        currentFreezerTarget = target;
    }
    [RoleAction(LotusActionType.Unshapeshift)]
    private void OnUnshapeshift()
    {
        freezeDuration.Finish();
        ResetSpeed();
        currentFreezerTarget = null;
    }

    private void ResetSpeed()
    {
        if (currentFreezerTarget == null) return;
        GameOptionOverride[] overrides = { new(Override.PlayerSpeedMod, AUSettings.PlayerSpeedMod()) };
        currentFreezerTarget.GetCustomRole().SyncOptions(overrides);
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub
                .Name("Freeze Cooldown")
                .Bind(v => freezeCooldown = (float)v)
                .AddFloatRange(5f, 120f, 2.5f, 10, GeneralOptionTranslations.SecondsSuffix)
                .Build())
            .SubOption(sub => sub
                .Name("Freeze Duration")
                .Bind(v => freezeDuration.Duration = (float)v)
                .AddFloatRange(5f, 60f, 2.5f, 4, GeneralOptionTranslations.SecondsSuffix)
                .Build())
            .SubOption(sub => sub
                .Name("Can Vent")
                .Bind(v => canVent = (bool)v)
                .AddOnOffValues()
                .Build());
    protected override AbstractBaseRole.RoleModifier Modify(AbstractBaseRole.RoleModifier modifier) =>
        base.Modify(modifier)
            .CanVent(canVent)
            .OptionOverride(Override.ShapeshiftDuration, freezeDuration.Duration)
            .OptionOverride(Override.ShapeshiftCooldown, freezeCooldown);
}