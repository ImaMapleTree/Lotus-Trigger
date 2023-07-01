using System.Linq;
using Lotus;
using Lotus.Factions.Crew;
using Lotus.Roles;
using VentLib.Utilities.Extensions;

namespace LotusTrigger.Gamemodes.Standard.Lotteries;

public class CrewmateLottery: RoleLottery
{
    public CrewmateLottery() : base(ProjectLotus.RoleManager.Vanilla.Crewmate)
    {
        ProjectLotus.RoleManager.AllRoles.Where(r => r.Faction is Crewmates && !r.RoleFlags.HasFlag(RoleFlag.IsSubrole)).ForEach(r => AddRole(r));
    }

    public override void AddRole(CustomRole role, bool useSubsequentChance = false)
    {
        int chance = useSubsequentChance ? role.AdditionalChance : role.Chance;
        if (chance == 0 || role.RoleFlags.HasFlag(RoleFlag.Unassignable)) return;
        uint id = Roles.Add(role);
        uint batch = BatchNumber++;

        string roleId = ProjectLotus.RoleManager.GetIdentifier(role);


        // If the role chance is at 100, we move it into the priority list
        if (chance >= 100)
        {
            PriorityTickets.Add(new Ticket { Id = id, Batch = batch, RoleId = roleId});
            return;
        }

        // Add tickets for the new role first
        for (int i = 0; i < chance; i++) Tickets.Add(new Ticket { Id = id, Batch = batch, RoleId = roleId});
    }
}