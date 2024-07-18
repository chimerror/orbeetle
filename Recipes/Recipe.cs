using Godot;
using GColl = Godot.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class Recipe : Resource
{
    public Quality CurrentQuality { get; set; } = Quality.Okay;

    [Export]
    public Texture2D RecipeTexture { get; set; }

    [Export]
    public string RecipeName { get; set; }

    [Export]
    public GColl.Array<Ingredient> RequiredIngredients { get; set; }

    public virtual bool CanAcceptIngredients(IEnumerable<Ingredient> ingredients)
    {
        return RequiredIngredients.All(ri => ingredients.Any(i => i.QualityInsensitiveEquals(ri)));
    }

    public Recipe ProcessIngredients(IEnumerable<Ingredient> inputIngredients)
    {
        // TODO: Decide final quality, etc.
        return Duplicate(true) as Recipe;
    }

     public enum Quality
    {
        Terrible,
        Bad,
        Okay,
        Good,
        Great
    }
}