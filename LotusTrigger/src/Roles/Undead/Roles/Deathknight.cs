using System;
using System.Collections.Generic;
using System.Linq;
using Lotus.API;
using Lotus.API.Odyssey;
using Lotus.API.Reactive.Actions;
using Lotus.Extensions;
using Lotus.GUI;
using Lotus.GUI.Name;
using Lotus.GUI.Name.Components;
using Lotus.GUI.Name.Holders;
using Lotus.Options;
using Lotus.Roles;
using Lotus.Roles.Internals;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Internals.Enums;
using UnityEngine;
using VentLib.Logging;
using VentLib.Options.Game;

namespace LotusTrigger.Roles.Undead.Roles;

public class Deathknight : UndeadRole
{
    public bool CanBecomeNecromancer;
    private float influenceRange;
    private bool multiInfluence;

    private Cooldown influenceCooldown;

    [NewOnSetup]
    private List<PlayerControl> inRangePlayers;
    private DateTime lastCheck = DateTime.Now;

    private const float UpdateTimeout = 0.25f;

    protected override void Setup(PlayerControl player)
    {
        base.Setup(player);
        LiveString liveString = new(() => inRangePlayers.Count > 0 ? "★" : "", Color.white);
        player.NameModel().GetComponentHolder<IndicatorHolder>().Add(new IndicatorComponent(liveString, GameState.Roaming, viewers: player));
    }

    [RoleAction(LotusActionType.FixedUpdate)]
    private void DeathknightFixedUpdate()
    {
        if (DateTime.Now.Subtract(lastCheck).TotalSeconds < UpdateTimeout) return;
        lastCheck = DateTime.Now;
        inRangePlayers.Clear();
        if (influenceCooldown.NotReady()) return;
        inRangePlayers = (influenceRange < 0
            ? MyPlayer.GetPlayersInAbilityRangeSorted().Where(IsUnconvertedUndead)
            : RoleUtils.GetPlayersWithinDistance(MyPlayer, influenceRange).Where(IsUnconvertedUndead))
            .ToList();
    }
    [RoleAction(LotusActionType.OnPet, priority: Priority.First)]
    private void InitiatePlayer(ActionHandle handle)
    {
        VentLogger.Trace("Deathknight Influence Ability", "DeathknightAbility");
        if (influenceCooldown.NotReady()) return;
        int influenceCount = Math.Min(inRangePlayers.Count, multiInfluence ? int.MaxValue : 1);
        if (influenceCount == 0) return;
        influenceCooldown.Start();
        handle.Cancel();
        for (int i = 0; i < influenceCount; i++) InitiateUndead(inRangePlayers[i]);
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub.Name("Can Become Necromancer")
                .AddOnOffValues()
                .BindBool(b => CanBecomeNecromancer = b)
                .Build())
            .SubOption(sub => sub.Name("Influence Cooldown")
                .AddFloatRange(5f, 120f, 2.5f, 7, GeneralOptionTranslations.SecondsSuffix)
                .BindFloat(influenceCooldown.SetDuration)
                .Build())
            .SubOption(sub => sub.Name("Influence Range")
                .BindFloat(v => influenceRange = v)
                .Value(v => v.Text("Kill Distance").Value(-1f).Build())
                .AddFloatRange(1.5f, 3f, 0.1f, 4)
                .Build())
            .SubOption(sub => sub.Name("Influences Many")
                .BindBool(b => multiInfluence = b)
                .AddOnOffValues()
                .Build());

    protected override AbstractBaseRole.RoleModifier Modify(AbstractBaseRole.RoleModifier roleModifier) =>
        base.Modify(roleModifier).RoleColor(new Color(0.34f, 0.34f, 0.39f)).RoleFlags(RoleFlag.TransformationRole);
}