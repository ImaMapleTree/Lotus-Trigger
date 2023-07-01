using LotusTrigger.Roles.Crew.Alchemist.Ingredients.Internal;
using UnityEngine;
using VentLib.Localization.Attributes;

namespace LotusTrigger.Roles.Crew.Alchemist.Ingredients;

public class IngredientCatalyst: Ingredient
{
    [Localized("Catalyst")]
    public static string IngredientName = "Catalyst";

    public IngredientCatalyst() : base(15f)
    {
    }

    public override string Name() => IngredientName;

    public override Color Color() => new(0.54f, 0.63f, 0.63f);

    public override string Symbol() => "◆";

    public override bool IsCollectable(Alchemist collector) => true;
}