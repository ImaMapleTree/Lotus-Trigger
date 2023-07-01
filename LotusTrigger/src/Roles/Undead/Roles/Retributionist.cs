using System.Collections.Generic;
using System.Linq;
using Hazel;
using Lotus;
using Lotus.API.Odyssey;
using Lotus.API.Reactive.Actions;
using Lotus.Extensions;
using Lotus.GUI;
using Lotus.GUI.Name;
using Lotus.GUI.Name.Components;
using Lotus.GUI.Name.Holders;
using Lotus.Options;
using Lotus.Roles;
using Lotus.Roles.Interactions;
using Lotus.Roles.Interactions.Interfaces;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Internals.Enums;
using Lotus.Utilities;
using LotusTrigger.Roles.NeutralKilling;
using UnityEngine;
using VentLib.Networking.RPC;
using VentLib.Options.Game;
using VentLib.Utilities;
using VentLib.Utilities.Collections;
using VentLib.Utilities.Extensions;
using Object = UnityEngine.Object;
using Unstoppable = LotusTrigger.Roles.Subroles.Unstoppable;

namespace LotusTrigger.Roles.Undead.Roles;

public class Retributionist : NeutralKillingBase
{
    private PlayerControl? attacker;
    private Remote<NameComponent>? remote;

    [UIComponent(UI.Cooldown)]
    private Cooldown revengeDuration;
    private bool invisibleRevenge;
    private int retributionLimit;
    private int remainingRevenges;

    [UIComponent(UI.Counter)]
    private string RevengeCounter() => retributionLimit != -1 ? RoleUtils.Counter(remainingRevenges, retributionLimit, RoleColor) : "";

    protected override void PostSetup()
    {
        remainingRevenges = retributionLimit;
        MyPlayer.NameModel().GetComponentHolder<CooldownHolder>()[0].SetPrefix("Time until Death: ").SetTextColor(RoleColor);
    }

    [RoleAction(LotusActionType.Attack)]
    public override bool TryKill(PlayerControl target)
    {
        if (attacker != null && target.PlayerId != attacker.PlayerId)
        {
            revengeDuration.Finish();
            return false;
        }
        if (!base.TryKill(target)) return false;
        attacker = null;
        revengeDuration.Finish();
        return true;
    }

    [RoleAction(LotusActionType.Interaction)]
    private void InitialAttack(PlayerControl actor, Interaction interaction, ActionHandle handle)
    {
        if (retributionLimit != -1 && remainingRevenges == 0) return;
        if (revengeDuration.NotReady()) return;
        if (interaction.Intent is not (IFatalIntent or Unstoppable.UnstoppableIntent)) return;

        remainingRevenges--;
        switch (interaction)
        {
            case IDelayedInteraction:
            case IIndirectInteraction:
            case IUnblockedInteraction:
                return;
        }

        handle.Cancel();
        attacker = actor;
        remote = attacker.NameModel().GetComponentHolder<NameHolder>().Add(new ColoredNameComponent(attacker, new Color(1f, 0.53f, 0f), GameState.Roaming, MyPlayer));
        revengeDuration.StartThenRun(CheckRevenge);
        DoRevengeTeleport();
    }

    private void DoRevengeTeleport()
    {
        List<Vent> vents = Object.FindObjectsOfType<Vent>().ToList();
        if (vents.Count == 0) return;
        Vent randomVent = vents.GetRandom();
        Vector2 ventPosition = randomVent.transform.position;
        Utils.Teleport(MyPlayer.NetTransform, new Vector2(ventPosition.x, ventPosition.y + 0.3636f));

        if (!invisibleRevenge) return;

        // Important: SendOption.None is necessary to prevent kicks via anticheat. In the future if this role is kicking players this is probably why
        Async.Schedule(() => RpcV3.Immediate(MyPlayer.MyPhysics.NetId, RpcCalls.EnterVent, SendOption.None).WritePacked(randomVent.Id).SendExcluding(MyPlayer.GetClientId()), NetUtils.DeriveDelay(0.5f));
        Async.Schedule(() => RpcV3.Immediate(MyPlayer.MyPhysics.NetId, RpcCalls.BootFromVent).WritePacked(randomVent.Id).Send(MyPlayer.GetClientId()), NetUtils.DeriveDelay(1.1f));
    }

    [RoleAction(LotusActionType.MeetingCalled)]
    private void CheckRevenge()
    {
        if (!MyPlayer.IsAlive() || attacker == null) return;
        remote?.Delete();
        remote = null;
        attacker.InteractWith(MyPlayer, new UnblockedInteraction(new FatalIntent(true), attacker.GetCustomRole()));
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub.Name("Revenge Time Limit")
                .AddFloatRange(5, 60, 2.5f, 2, GeneralOptionTranslations.SecondsSuffix)
                .BindFloat(revengeDuration.SetDuration)
                .Build())
            .SubOption(sub => sub.Name("Invisible During Revenge")
                .AddOnOffValues(false)
                .BindBool(b => invisibleRevenge = b)
                .Build())
            .SubOption(sub => sub.Name("Number of Revenges")
                .Value(v => v.Text("∞").Color(ModConstants.Palette.InfinityColor).Value(-1).Build())
                .AddIntRange(1, 20, 1, 0)
                .BindInt(i => retributionLimit = i)
                .Build());


    protected override AbstractBaseRole.RoleModifier Modify(AbstractBaseRole.RoleModifier roleModifier) =>
        base.Modify(roleModifier).RoleColor(new Color(0.73f, 0.66f, 0.69f));
}