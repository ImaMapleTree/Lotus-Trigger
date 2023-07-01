using System.Linq;
using Lotus;
using Lotus.Factions.Impostors;
using Lotus.Roles;
using VentLib.Utilities.Extensions;

namespace LotusTrigger.Gamemodes.Standard.Lotteries;

public class ImpostorLottery: RoleLottery
{
    public ImpostorLottery() : base(ProjectLotus.RoleManager.Vanilla.Impostor)
    {
        ProjectLotus.RoleManager.AllRoles.Where(r => r.Faction is ImpostorFaction).ForEach(r => AddRole(r));
    }
}