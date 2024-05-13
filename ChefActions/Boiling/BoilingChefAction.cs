using System.Collections.Generic;
using System.Linq;

public partial class BoilingChefAction : ChefAction
{
    public override bool CanAcceptIngredients(IEnumerable<Ingredient> ingredients)
    {
        var ingredientsList = ingredients.ToList();
        if (!base.CanAcceptIngredients(ingredients) || ingredientsList.Count != 2)
        {
            return false;
        }
        var ingredientToBoil = GetIngredientToBoil(ingredientsList);
        if (ingredientToBoil == default) // base checked if there is water
        {
            return false;
        }
        return ingredientToBoil.CurrentState == IngredientState.Chopped &&
            ingredientToBoil.AllowedStates.Contains(IngredientState.Boiled);
    }

    public override Ingredient ProcessIngredients(IEnumerable<Ingredient> inputIngredients)
    {
        // TODO: Affect Quality as well as State. Probably best to do this in a method in the base ChefAction so other
        // Chef Actions can all act the same.
        var ingredientsList = inputIngredients.ToList();
        var processedIngredient = GetIngredientToBoil(ingredientsList).Duplicate(true) as Ingredient;
        processedIngredient.CurrentState = IngredientState.Boiled;
        return processedIngredient;
    }

    private Ingredient GetIngredientToBoil(List<Ingredient> ingredients)
    {
        return ingredients.FirstOrDefault(i => !RequiredIngredients.Contains(i));
    }

    private Ingredient GetWater(List<Ingredient> ingredients)
    {
        return ingredients.FirstOrDefault(i => RequiredIngredients.Contains(i));
    }
}