using Lotus.API.Odyssey;
using Lotus.API.Reactive.Actions;
using Lotus.API.Vanilla.Sabotages;
using Lotus.Extensions;
using Lotus.GUI.Name;
using Lotus.GUI.Name.Components;
using Lotus.GUI.Name.Holders;
using Lotus.Options;
using Lotus.Roles;
using Lotus.Roles.Builtins.Vanilla;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Internals.Enums;
using Lotus.Roles.Overrides;
using UnityEngine;
using VentLib.Logging;
using VentLib.Options.Game;
using Priority = Lotus.API.Priority;

namespace LotusTrigger.Roles.Impostors;

public class Mare: Impostor
{
    private bool canKillWithoutSabotage;
    private float normalKillCooldown;
    private bool redNameDuringSabotage;
    private float sabotageSpeedMod;
    private float reducedKillCooldown;
    private SabotageType activationSabo;
    private bool abilityEnabled;
    private bool abilityLightsOnly;
    private NameComponent coloredName;

    protected override void Setup(PlayerControl player)
    {
        activationSabo = abilityLightsOnly ? SabotageType.Lights : activationSabo;
        coloredName = new NameComponent(new LiveString(MyPlayer.name, new Color(0.36f, 0f, 0.58f)), GameState.Roaming, ViewMode.Absolute);
    }

    [RoleAction(LotusActionType.Attack)]
    public new bool TryKill(PlayerControl target) => (canKillWithoutSabotage || abilityEnabled) && base.TryKill(target);

    [RoleAction(LotusActionType.SabotageStarted, priority: Priority.Last)]
    private void MareSabotageCheck(ISabotage sabotage, ActionHandle handle)
    {
        if (!activationSabo.HasFlag(sabotage.SabotageType()) || handle.IsCanceled) return;
        VentLogger.Trace("Mare ability is activated", "MareAbilityCheck");
        abilityEnabled = true;
        if (redNameDuringSabotage) MyPlayer.NameModel().GetComponentHolder<NameHolder>().Add(coloredName);
        SyncOptions();
    }

    [RoleAction(LotusActionType.SabotageFixed)]
    private void MareSabotageFix()
    {
        abilityEnabled = false;
        if (redNameDuringSabotage) MyPlayer.NameModel().GetComponentHolder<NameHolder>().Remove(coloredName);
        SyncOptions();
    }

    // lol this was fun because of the bitwise operators
    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub
                .Name("Speed Modifier During Sabotage")
                .Bind(v => sabotageSpeedMod = (float)v)
                .AddFloatRange(0.5f, 3, 0.1f, 10, "x").Build())
            .SubOption(sub => sub
                .Name("Can Kill Without Sabotage")
                .Bind(v => canKillWithoutSabotage = (bool)v)
                .ShowSubOptionPredicate(v => (bool)v)
                .AddOnOffValues()
                .SubOption(sub2 => sub2
                    .Name("Normal Kill Cooldown")
                    .Bind(v => normalKillCooldown = (float)v)
                    .AddFloatRange(0, 120, 2.5f, 10, GeneralOptionTranslations.SecondsSuffix)
                    .Build())
                .Build())
            .SubOption(sub => sub
                .Name("Colored Name During Sabotage")
                .Bind(v => redNameDuringSabotage = (bool)v)
                .AddOnOffValues().Build())
            .SubOption(sub => sub
                .Name("Kill Cooldown During Sabotage")
                .Bind(v => reducedKillCooldown = (float)v)
                .AddFloatRange(0, 60, 5, 3, GeneralOptionTranslations.SecondsSuffix).Build())
            .SubOption(sub => sub
                .Name("Specific Sabotage Settings")
                .ShowSubOptionPredicate(v => (bool)v)
                .BindBool(v => abilityLightsOnly = v)
                .Value(v => v.Text("Lights Only").Value(false).Build())
                .Value(v => v.Text("Individual").Value(true).Build())
                .SubOption(sub2 => sub2
                    .Name("Lights")
                    .Bind(v => activationSabo = (bool)v ? activationSabo | SabotageType.Lights : activationSabo & ~SabotageType.Lights)
                    .AddOnOffValues().Build())
                .SubOption(sub2 => sub2
                    .Name("Communications")
                    .Bind(v => activationSabo = (bool)v ? activationSabo | SabotageType.Communications : activationSabo & ~SabotageType.Communications)
                    .AddOnOffValues(false).Build())
                .SubOption(sub2 => sub2
                    .Name("Oxygen")
                    .Bind(v => activationSabo = (bool)v ? activationSabo | SabotageType.Oxygen : activationSabo & ~SabotageType.Oxygen)
                    .AddOnOffValues(false).Build())
                .SubOption(sub2 => sub2
                    .Name("Reactor")
                    .Bind(v => activationSabo = (bool)v ? activationSabo | SabotageType.Reactor : activationSabo & ~SabotageType.Reactor)
                    .AddOnOffValues(false).Build())
                .SubOption(sub2 => sub2
                    .Name("Helicopter")
                    .Bind(v => activationSabo = (bool)v ? activationSabo | SabotageType.Helicopter : activationSabo & ~SabotageType.Helicopter)
                    .AddOnOffValues(false).Build())
                .Build());

    protected override AbstractBaseRole.RoleModifier Modify(AbstractBaseRole.RoleModifier roleModifier) =>
        base.Modify(roleModifier)
            .OptionOverride(Override.KillCooldown, () => abilityEnabled ? reducedKillCooldown : normalKillCooldown)
            .OptionOverride(Override.PlayerSpeedMod, () => sabotageSpeedMod, () => abilityEnabled);
}