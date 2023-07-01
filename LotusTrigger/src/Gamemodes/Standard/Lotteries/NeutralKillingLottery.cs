using System.Linq;
using Lotus;
using Lotus.Roles;
using Lotus.Roles.Internals.Enums;
using VentLib.Utilities.Extensions;

namespace LotusTrigger.Gamemodes.Standard.Lotteries;

public class NeutralKillingLottery: RoleLottery
{
    // TODO: maybe change this default role
    public NeutralKillingLottery() : base(ProjectLotus.RoleManager.Internal.IllegalRole)
    {
        ProjectLotus.RoleManager.AllRoles.Where(r => r.SpecialType is SpecialType.NeutralKilling or SpecialType.Undead).ForEach(r => AddRole(r));
    }
}