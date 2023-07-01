using LotusTrigger.Roles.Crew.Alchemist.Ingredients.Internal;
using UnityEngine;
using VentLib.Utilities.Collections;

namespace LotusTrigger.Roles.Crew.Alchemist.Potions;

public interface ICraftable
{
    public string Name();

    public Color Color();

    public OrderedDictionary<IngredientInfo, int> Ingredients();

    public bool Use(PlayerControl user);
}