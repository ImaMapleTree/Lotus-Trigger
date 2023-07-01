using AmongUs.GameOptions;
using Lotus;
using Lotus.Extensions;
using Lotus.Factions;
using Lotus.Roles.Builtins.Vanilla;
using Lotus.Roles.Internals.Enums;
using VentLib.Options.Game;

namespace LotusTrigger.Roles.Madmates.Roles;

public class Madmate : Impostor
{
    protected override GameOptionBuilder RegisterOptions(GameOptionBuilder optionStream) =>
        base.RegisterOptions(optionStream)
            .SubOption(sub => sub.Name("Can Sabotage")
                .BindBool(b => canSabotage = b)
                .AddOnOffValues()
                .Build());

    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier)
            .VanillaRole(canSabotage ? RoleTypes.Impostor : RoleTypes.Engineer)
            .SpecialType(SpecialType.Madmate)
            .RoleColor(ModConstants.Palette.MadmateColor)
            .Faction(FactionInstances.Madmates);
}