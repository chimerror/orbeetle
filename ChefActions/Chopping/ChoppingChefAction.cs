using System.Collections.Generic;
using System.Linq;

public partial class ChoppingChefAction : ChefAction
{
    public override bool CanAcceptIngredients(IEnumerable<Ingredient> ingredients)
    {
        var ingredientsList = ingredients.ToList();
        if (ingredientsList.Count != 1)
        {
            return false;
        }
        var candidateIngredient = ingredientsList[0];
        return candidateIngredient.CurrentState == Ingredient.State.Raw &&
            candidateIngredient.AllowedStates.Contains(Ingredient.State.Chopped);
    }

    public override Ingredient ProcessIngredients(IEnumerable<Ingredient> inputIngredients)
    {
        var processedIngredient = inputIngredients.First().Duplicate(true) as Ingredient;
        processedIngredient.CurrentState = Ingredient.State.Chopped;
        return processedIngredient;
    }
}