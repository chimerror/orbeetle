using Godot;
using GColl = Godot.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class GrillingChefAction : ChefAction
{

    [Export]
    public GColl.Array<Ingredient> MustBeCooked { get; set; }

    public override bool CanAcceptIngredients(IEnumerable<Ingredient> ingredients)
    {
        var ingredientsList = ingredients.ToList();
        /*
        Unclear if I'll need this?
        if (ingredientsList.Count != 1)
        {
            return false;
        }
        */
        var candidateIngredient = ingredientsList[0];
        if (candidateIngredient.RequiresButchering && candidateIngredient.CurrentState != Ingredient.State.Butchered)
        {
            return false;
        }
        else if (!candidateIngredient.RequiresButchering && candidateIngredient.CurrentState != Ingredient.State.Raw)
        {
            return false;
        }
        return candidateIngredient.AllowedStates.Contains(Ingredient.State.Grilled);
    }

    public override Ingredient ProcessIngredients(IEnumerable<Ingredient> inputIngredients)
    {
        var processedIngredient = inputIngredients.First().Duplicate(true) as Ingredient;
        processedIngredient.CurrentState = Ingredient.State.Grilled;
        return processedIngredient;
    }
}