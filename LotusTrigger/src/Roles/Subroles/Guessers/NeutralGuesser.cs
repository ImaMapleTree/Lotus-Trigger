using Lotus.Extensions;
using Lotus.Roles;
using Lotus.Roles.Builtins.Base;
using Lotus.Roles.Internals.Enums;

namespace LotusTrigger.Roles.Subroles.Guessers;

public class NeutralGuesser: Guesser
{
    public override bool IsAssignableTo(PlayerControl player)
    {
        CustomRole role = player.GetCustomRole();
        return role is not GuesserRoleBase && role.SpecialType is SpecialType.Neutral;
    }
}