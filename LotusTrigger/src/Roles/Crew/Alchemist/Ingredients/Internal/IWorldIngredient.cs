using UnityEngine;

namespace LotusTrigger.Roles.Crew.Alchemist.Ingredients.Internal;

public interface IWorldIngredient : IAlchemyIngredient
{
    public Vector2 Position();

    public float CollectRadius();
}