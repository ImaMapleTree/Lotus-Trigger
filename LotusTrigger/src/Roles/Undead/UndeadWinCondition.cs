using System.Collections.Generic;
using System.Linq;
using Lotus.API.Player;
using Lotus.Extensions;
using Lotus.Factions;
using Lotus.Factions.Interfaces;
using Lotus.Factions.Undead;
using Lotus.Roles;
using Lotus.Victory.Conditions;
using LotusTrigger.Roles.Undead.Roles;

namespace LotusTrigger.Roles.Undead;

public class UndeadWinCondition : IFactionWinCondition
{
    private static readonly List<IFaction> UndeadFactions = new() { FactionInstances.TheUndead, new TheUndead.Unconverted(null!, null!) };
    public List<IFaction> Factions() => UndeadFactions;

    public bool IsConditionMet(out List<IFaction> factions)
    {
        factions = UndeadFactions;

        int aliveUndead = 0;
        int aliveOther = 0;

        bool necromancerAlive = false;
        foreach (CustomRole role in Players.GetPlayers(PlayerFilter.Alive).Select(p => p.GetCustomRole()))
        {
            if (role is Necromancer) necromancerAlive = true;
            if (role.Faction is TheUndead) aliveUndead++;
            else aliveOther++;
        }

        //if (necromancerAlive && aliveUndead >= aliveOther) VentLogger.Info("Undead Win");
        return necromancerAlive && aliveUndead >= aliveOther;
    }

    public WinReason GetWinReason() => new(ReasonType.FactionLastStanding);
}