﻿using Lotus.Factions;
using Lotus.Factions.Interfaces;
using Lotus.Roles;

namespace LotusTrigger.Roles.Subroles.Romantics;

public class RomanticFaction: Lotus.Factions.Neutrals.Neutral
{
    public byte Partner = byte.MaxValue;
    private IFaction originalFaction;

    public RomanticFaction(IFaction originalFaction)
    {
        this.originalFaction = originalFaction;
    }

    public override Relation Relationship(CustomRole otherRole)
    {
        return otherRole.MyPlayer.PlayerId == Partner ? Relation.SharedWinners : originalFaction.Relationship(otherRole);
    }

    public override Relation RelationshipOther(IFaction other)
    {
        return originalFaction.Relationship(other);
    }
}