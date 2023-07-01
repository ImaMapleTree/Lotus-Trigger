using Lotus;
using Lotus.API.Odyssey;
using Lotus.API.Reactive.Actions;
using Lotus.Extensions;
using Lotus.Roles;
using Lotus.Roles.Builtins.Base;
using Lotus.Roles.Interfaces;
using Lotus.Roles.Internals.Attributes;
using Lotus.Roles.Internals.Enums;
using Lotus.Roles.Overrides;
using UnityEngine;
using VentLib.Localization.Attributes;
using VentLib.Options.Game;
using VentLib.Utilities.Collections;

namespace LotusTrigger.Roles.Subroles;

public class Flash : Subrole, IVariantSubrole
{
    private static Escalation _escalation = new();

    private float playerSpeedIncrease = 1f;
    private Remote<GameOptionOverride>? overrideRemote;

    public override string Identifier() => "◎";

    public Subrole Variation() => _escalation;

    public bool AssignVariation() => RoleUtils.RandomSpawn(_escalation);

    [RoleAction(LotusActionType.RoundStart)]
    private void GameStart(bool isStart)
    {
        if (!isStart) return;
        AdditiveOverride additiveOverride = new(Override.PlayerSpeedMod, playerSpeedIncrease);
        overrideRemote = Game.MatchData.Roles.AddOverride(MyPlayer.PlayerId, additiveOverride);
    }

    [RoleAction(LotusActionType.MyDeath)]
    private void RemoveOverride() => overrideRemote?.Delete();

    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub
                .KeyName("Speed Increase", Translations.Options.SpeedIncrease)
                .AddFloatRange(0.25f, 2.5f, 0.25f, 3)
                .BindFloat(f => playerSpeedIncrease = f)
                .Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier)
            .RoleColor(Color.yellow)
            .LinkedRoles(_escalation);

    [Localized(nameof(Flash))]
    private static class Translations
    {
        [Localized(ModConstants.Options)]
        public static class Options
        {
            [Localized(nameof(SpeedIncrease))]
            public static string SpeedIncrease = "Speed Increase";
        }
    }

}

