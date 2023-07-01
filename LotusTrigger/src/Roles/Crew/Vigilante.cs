using Lotus.Factions;
using Lotus.Roles.Builtins.Base;
using UnityEngine;

namespace LotusTrigger.Roles.Crew;


public class Vigilante: GuesserRoleBase
{
    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        roleModifier.Faction(FactionInstances.Crewmates)
            .RoleColor(new Color(0.89f, 0.88f, 0.52f));
}