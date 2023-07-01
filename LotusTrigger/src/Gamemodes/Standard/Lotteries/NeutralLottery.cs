using System.Linq;
using Lotus;
using Lotus.Roles;
using Lotus.Roles.Internals.Enums;
using VentLib.Utilities.Extensions;

namespace LotusTrigger.Gamemodes.Standard.Lotteries;

public class NeutralLottery: RoleLottery
{
    public NeutralLottery() : base(ProjectLotus.RoleManager.Internal.IllegalRole)
    {
        ProjectLotus.RoleManager.AllRoles.Where(r => r.SpecialType is SpecialType.Neutral).ForEach(r => AddRole(r));
    }
}