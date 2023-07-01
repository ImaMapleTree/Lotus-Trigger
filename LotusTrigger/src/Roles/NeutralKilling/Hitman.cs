using System.Collections.Generic;
using Lotus.API.Odyssey;
using Lotus.API.Reactive.Actions;
using Lotus.Extensions;
using Lotus.Factions;
using Lotus.Factions.Interfaces;
using Lotus.Roles;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Internals.Enums;
using Lotus.Victory;
using Lotus.Victory.Conditions;
using UnityEngine;
using VentLib.Options.Game;

namespace LotusTrigger.Roles.NeutralKilling;

public class Hitman: NeutralKillingBase
{
    private static HitmanFaction _hitmanFaction = new();
    public List<string> AdditionalWinRoles = new();

    protected override void Setup(PlayerControl player)
    {
        base.Setup(player);
        Game.GetWinDelegate().AddSubscriber(GameEnd);
    }

    [RoleAction(LotusActionType.Attack)]
    public override bool TryKill(PlayerControl target) => base.TryKill(target);

    private void GameEnd(WinDelegate winDelegate)
    {
        if (!MyPlayer.IsAlive()) return;
        if (winDelegate.GetWinReason().ReasonType is ReasonType.SoloWinner && !AdditionalWinRoles.Contains(winDelegate.GetWinners()[0].GetCustomRole().EnglishRoleName)) return;
        winDelegate.AddAdditionalWinner(MyPlayer);
    }

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub
                .Name("Wins with Absolute Winners")
                .Value(v => v.Text("None").Color(Color.red).Value(0).Build())
                .Value(v => v.Text("All").Color(Color.cyan).Value(1).Build())
                .Value(v => v.Text("Individual").Color(new Color(0.45f, 0.31f, 0.72f)).Value(2).Build())
                .ShowSubOptionPredicate(o => (int)o == 2)
                .SubOption(sub2 => sub2
                    .Name("Executioner")
                    .Color(new Color(0.55f, 0.17f, 0.33f))
                    .AddOnOffValues()
                    .BindBool(RoleUtils.BindOnOffListSetting(AdditionalWinRoles, "Executioner"))
                    .Build())
                .SubOption(sub2 => sub2
                    .Name("Jester")
                    .Color(new Color(0.93f, 0.38f, 0.65f))
                    .AddOnOffValues()
                    .BindBool(RoleUtils.BindOnOffListSetting(AdditionalWinRoles, "Jester"))
                    .Build())
                .Build());

    protected override AbstractBaseRole.RoleModifier Modify(AbstractBaseRole.RoleModifier roleModifier) => base.Modify(roleModifier).Faction(_hitmanFaction);


    private class HitmanFaction : Lotus.Factions.Neutrals.Neutral
    {
        public override Relation Relationship(Lotus.Factions.Neutrals.Neutral sameFaction)
        {
            return Relation.SharedWinners;
        }

        public override Relation RelationshipOther(IFaction other)
        {
            return Relation.SharedWinners;
        }
    }
}