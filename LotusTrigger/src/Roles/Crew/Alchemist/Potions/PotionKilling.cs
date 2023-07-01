using System.Collections.Generic;
using Lotus.Extensions;
using Lotus.Factions;
using Lotus.Roles;
using Lotus.Roles.Interactions;
using LotusTrigger.Roles.Crew.Alchemist.Ingredients.Internal;
using UnityEngine;
using VentLib.Localization.Attributes;

namespace LotusTrigger.Roles.Crew.Alchemist.Potions;

public class PotionKilling: Potion
{
    [Localized("Death")]
    public static string PotionName = "Potion of Death";

    public PotionKilling(int requiredCatalyst) : base((1, Ingredient.Death), (requiredCatalyst, Ingredient.Catalyst))
    {
    }

    public override string Name() => PotionName;

    public override Color Color() => FactionInstances.TheUndead.Color;

    public override bool Use(PlayerControl user)
    {
        List<PlayerControl> sortedPlayers = user.GetPlayersInAbilityRangeSorted();
        if (sortedPlayers.Count == 0) return false;
        user.InteractWith(sortedPlayers[0], LotusInteraction.FatalInteraction.Create(user.GetCustomRole()));
        return true;
    }
}