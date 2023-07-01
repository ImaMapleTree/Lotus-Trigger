using System.Collections.Generic;
using System.Linq;
using Lotus.Roles;

namespace LotusTrigger.Roles.Collections;

public abstract class RoleCollection
{
    private List<CustomRole>? roles;
    
    public List<CustomRole> GetRoles()
    {
        return roles ??= this.GetType()
            .GetFields()
            .Where(f => !f.IsStatic)
            .Select(f => (CustomRole)f.GetValue(this)!)
            .ToList();
    }
}