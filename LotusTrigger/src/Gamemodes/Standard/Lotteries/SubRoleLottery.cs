using System.Linq;
using Lotus;
using Lotus.Roles;
using VentLib.Utilities.Extensions;

namespace LotusTrigger.Gamemodes.Standard.Lotteries;

public class SubRoleLottery: RoleLottery
{
    public SubRoleLottery() : base(ProjectLotus.RoleManager.Internal.IllegalRole)
    {
        ProjectLotus.RoleManager.AllRoles.Where(r => r.RoleFlags.HasFlag(RoleFlag.IsSubrole)).ForEach(r => AddRole(r));
    }
}