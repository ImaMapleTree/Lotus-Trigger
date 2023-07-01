using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Lotus.API.Reactive.Actions;
using Lotus.Extensions;
using Lotus.Factions;
using Lotus.GUI;
using Lotus.GUI.Name;
using Lotus.Options;
using Lotus.Roles;
using Lotus.Roles.Builtins.Vanilla;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Internals.Enums;
using Lotus.Roles.Overrides;
using VentLib.Options.Game;
using VentLib.Utilities;

namespace LotusTrigger.Roles.Impostors;

public class Grenadier : Impostor
{
    [UIComponent(UI.Cooldown)]
    private Cooldown blindCooldown;
    private float blindDuration;
    private float blindDistance;
    private bool canVent;
    private bool canBlindAllies;
    private int grenadeAmount;
    private int grenadesLeft;

    [RoleAction(LotusActionType.Attack)]
    public new bool TryKill(PlayerControl target) => base.TryKill(target);

    [RoleAction(LotusActionType.OnPet)]
    private void GrenadierBlind()
    {
        if (blindCooldown.NotReady() || grenadesLeft <= 0) return;

        GameOptionOverride[] overrides = { new(Override.CrewLightMod, 0f), new(Override.ImpostorLightMod, 0f) };
        List<PlayerControl> playersInDistance = blindDistance > 0
            ? RoleUtils.GetPlayersWithinDistance(MyPlayer, blindDistance).ToList()
            : MyPlayer.GetPlayersInAbilityRangeSorted();

        playersInDistance.Where(p => canBlindAllies || p.Relationship(MyPlayer) is not Relation.FullAllies)
            .Do(p =>
            {
                p.GetCustomRole().SyncOptions(overrides);
                Async.Schedule(() => p.GetCustomRole().SyncOptions(), blindDuration);
            });

        blindCooldown.Start();
        grenadesLeft--;
    }

    [RoleAction(LotusActionType.RoundStart)]
    private void SetGrenadeAmount() => grenadesLeft = grenadeAmount;

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub
                .Name("Amount of Grenades")
                .Bind(v => grenadeAmount = (int)v)
                .AddIntRange(1, 5, 1, 2)
                .Build())
            .SubOption(sub => sub
                .Name("Blind Cooldown")
                .Bind(v => blindCooldown.Duration = (float)v)
                .AddFloatRange(5f, 120f, 2.5f, 10, GeneralOptionTranslations.SecondsSuffix)
                .Build())
            .SubOption(sub => sub
                .Name("Blind Duration")
                .Bind(v => blindDuration = (float)v)
                .AddFloatRange(5f, 60f, 2.5f, 4, GeneralOptionTranslations.SecondsSuffix)
                .Build())
            .SubOption(sub => sub
                .Name("Blind Effect Radius")
                .Bind(v => blindDistance = (float)v)
                .Value(v => v.Text("Kill Distance").Value(-1f).Build())
                .AddFloatRange(1.5f, 3f, 0.1f, 4)
                .Build())
            .SubOption(sub => sub
                .Name("Can Blind Allies")
                .Bind(v => canBlindAllies = (bool)v)
                .AddOnOffValues(false)
                .Build())
            .SubOption(sub => sub
                .Name("Can Vent")
                .Bind(v => canVent = (bool)v)
                .AddOnOffValues()
                .Build());

    protected override AbstractBaseRole.RoleModifier Modify(AbstractBaseRole.RoleModifier roleModifier) =>
        base.Modify(roleModifier)
            .CanVent(canVent);
}