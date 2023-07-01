using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Lotus.API.Odyssey;
using Lotus.API.Reactive.Actions;
using Lotus.Extensions;
using Lotus.Factions;
using Lotus.GUI.Name;
using Lotus.GUI.Name.Components;
using Lotus.GUI.Name.Holders;
using Lotus.Roles;
using Lotus.Roles.Builtins.Vanilla;
using Lotus.Roles.Events;
using Lotus.Roles.Interactions;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Internals.Enums;
using Lotus.Roles.Overrides;
using Lotus.Utilities;
using UnityEngine;
using VentLib.Utilities.Collections;
using VentLib.Utilities.Extensions;

namespace LotusTrigger.Roles.Impostors;

public class Puppeteer: Impostor
{
    [NewOnSetup] private List<PlayerControl> cursedPlayers;
    [NewOnSetup] private Dictionary<byte, Remote<IndicatorComponent>> playerRemotes = null!;

    private FixedUpdateLock fixedUpdateLock = new();

    [RoleAction(LotusActionType.Attack)]
    public override bool TryKill(PlayerControl target)
    {
        MyPlayer.RpcMark(target);
        if (MyPlayer.InteractWith(target, LotusInteraction.HostileInteraction.Create(this)) is InteractionResult.Halt) return false;

        Game.MatchData.GameHistory.AddEvent(new ManipulatedEvent(MyPlayer, target));
        cursedPlayers.Add(target);

        playerRemotes!.GetValueOrDefault(target.PlayerId, null)?.Delete();
        IndicatorComponent component = new(new LiveString("◆", new Color(0.36f, 0f, 0.58f)), Game.IgnStates, viewers: MyPlayer);
        playerRemotes[target.PlayerId] = target.NameModel().GetComponentHolder<IndicatorHolder>().Add(component);

        return true;
    }

    [RoleAction(LotusActionType.FixedUpdate)]
    private void PuppeteerKillCheck()
    {
        if (!fixedUpdateLock.AcquireLock()) return;
        foreach (PlayerControl player in new List<PlayerControl>(cursedPlayers))
        {
            if (player == null)
            {
                cursedPlayers.Remove(player!);
                continue;
            }
            if (player.Data.IsDead) {
                RemovePuppet(player);
                continue;
            }

            List<PlayerControl> inRangePlayers = player.GetPlayersInAbilityRangeSorted().Where(p => p.Relationship(MyPlayer) is not Relation.FullAllies).ToList();
            if (inRangePlayers.Count == 0) continue;
            PlayerControl target = inRangePlayers.GetRandom();
            ManipulatedPlayerDeathEvent playerDeathEvent = new(target, player);
            FatalIntent fatalIntent = new(false, () => playerDeathEvent);
            bool isDead = player.InteractWith(target, new ManipulatedInteraction(fatalIntent, player.GetCustomRole(), MyPlayer)) is InteractionResult.Proceed;
            Game.MatchData.GameHistory.AddEvent(new ManipulatedPlayerKillEvent(player, target, MyPlayer, isDead));
            RemovePuppet(player);
        }

        cursedPlayers.Where(p => p.Data.IsDead).ToArray().Do(RemovePuppet);
    }

    [RoleAction(LotusActionType.SelfExiled)]
    [RoleAction(LotusActionType.MyDeath)]
    [RoleAction(LotusActionType.RoundStart, triggerAfterDeath: true)]
    private void ClearPuppets()
    {
        cursedPlayers.ToArray().ForEach(RemovePuppet);
        cursedPlayers.Clear();
    }

    [RoleAction(LotusActionType.AnyDeath)]
    [RoleAction(LotusActionType.Disconnect)]
    private void RemovePuppet(PlayerControl puppet)
    {
        if (cursedPlayers.All(p => p.PlayerId != puppet.PlayerId)) return;
        playerRemotes!.GetValueOrDefault(puppet.PlayerId, null)?.Delete();
        cursedPlayers.RemoveAll(p => p.PlayerId == puppet.PlayerId);
    }

    protected override AbstractBaseRole.RoleModifier Modify(AbstractBaseRole.RoleModifier roleModifier) =>
        base.Modify(roleModifier).OptionOverride(new IndirectKillCooldown(KillCooldown));
}