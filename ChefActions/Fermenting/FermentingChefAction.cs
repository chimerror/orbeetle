using System.Collections.Generic;
using System.Linq;

public partial class FermentingChefAction : ChefAction
{
    public override bool CanAcceptIngredients(IEnumerable<Ingredient> ingredients)
    {
        var ingredientsList = ingredients.ToList();
        if (!base.CanAcceptIngredients(ingredients) || ingredientsList.Count != 3)
        {
            return false;
        }
        var ingredientToFerment = GetIngredientsToFerment(ingredientsList);
        if (ingredientToFerment == default) // base checked if there is water
        {
            return false;
        }
        return ingredientToFerment.CurrentState == Ingredient.State.Chopped &&
            ingredientToFerment.AllowedStates.Contains(Ingredient.State.Fermented);
    }

    public override Ingredient ProcessIngredients(IEnumerable<Ingredient> inputIngredients)
    {
        // TODO: Affect Quality as well as State. Probably best to do this in a method in the base ChefAction so other
        // Chef Actions can all act the same.
        var ingredientsList = inputIngredients.ToList();
        var processedIngredient = GetIngredientsToFerment(ingredientsList).Duplicate(true) as Ingredient;
        processedIngredient.CurrentState = Ingredient.State.Fermented;
        return processedIngredient;
    }

    private Ingredient GetIngredientsToFerment(List<Ingredient> ingredients)
    {
        return ingredients.FirstOrDefault(i => !RequiredIngredients.Contains(i));
    }

    private Ingredient GetWater(List<Ingredient> ingredients)
    {
        return ingredients.FirstOrDefault(i => RequiredIngredients.Contains(i));
    }
}