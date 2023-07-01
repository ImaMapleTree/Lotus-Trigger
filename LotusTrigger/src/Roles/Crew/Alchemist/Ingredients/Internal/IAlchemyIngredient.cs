using UnityEngine;

namespace LotusTrigger.Roles.Crew.Alchemist.Ingredients.Internal;

public interface IAlchemyIngredient
{
    public string Name();

    public Color Color();

    public string Symbol();

    public bool IsCollectable(Alchemist collector);

    public bool IsExpired();

    public IngredientInfo AsInfo();

    public void Collect();
}