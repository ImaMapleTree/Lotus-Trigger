using Lotus.Factions;
using Lotus.Roles.Builtins.Vanilla;
using Lotus.Roles.Interfaces;
using Lotus.Roles.Internals.Enums;
using UnityEngine;

namespace LotusTrigger.Roles.NeutralKilling;

public partial class NeutralKillingBase: Impostor, IModdable
{
    protected override RoleModifier Modify(RoleModifier roleModifier) =>
        base.Modify(roleModifier)
            .SpecialType(SpecialType.NeutralKilling)
            .Faction(FactionInstances.Neutral)
            .RoleColor(Color.gray);
}