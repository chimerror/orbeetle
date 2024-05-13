using System.Collections.Generic;
using System.Linq;
using Godot;
using GColl = Godot.Collections;

public abstract partial class ChefAction : Resource
{
    // TODO: Add ways to specify animations?
    [Export]
    public string ActionVerb { get; set; }

    [Export]
    public double Duration { get; set; } = 3.0;

    [Export]
    public GColl.Array<Ingredient> RequiredIngredients { get; set; } = new GColl.Array<Ingredient>();

    public virtual bool CanAcceptIngredients(IEnumerable<Ingredient> ingredients)
    {
        // Don't forget to call this base method if you have required ingredients.
        return RequiredIngredients.Count == 0 ||
            RequiredIngredients.All(ri => ingredients.Any(i => i.QualityInsensitiveEquals(ri)));
    }

    public abstract Ingredient ProcessIngredients(IEnumerable<Ingredient> inputIngredients);
}