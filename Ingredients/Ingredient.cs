using Godot;
using GColl = Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Ingredient : Resource
{
    public IngredientQuality Quality { get; set; } = IngredientQuality.Okay;

    public Dictionary<IngredientState, Texture2D> TextureDictionary
    {
        get => AllowedStates.Zip(StateTextures).ToDictionary(g => g.First, g => g.Second);
    }

    public Texture2D CurrentTexture
    {
        get => TextureDictionary[CurrentState];
    }

    [Export]
    public string IngredientName { get; set; }

    [Export]
    public IngredientState CurrentState { get; set; } = IngredientState.Raw;

    [Export]
    public GColl.Array<IngredientState> AllowedStates { get; set; } = new GColl.Array<IngredientState>();

    [ExportGroup("Ingredient Visuals")]
    [Export]
    public GColl.Array<Texture2D> StateTextures { get; set; } = new GColl.Array<Texture2D>();

    public bool IsStateAllowed(IngredientState state)
    {
        return AllowedStates.Contains(state);
    }

    public override bool Equals(object that)
    {
        if (that == null)
        {
            return false;
        }

        if (that is Ingredient thatIngredient)
        {
            return Equals(thatIngredient);
        }
        else
        {
            return false;
        }
    }

    public bool Equals(Ingredient that)
    {
        if (that == null)
        {
            return false;
        }

        return IngredientName == that.IngredientName &&
            CurrentState == that.CurrentState &&
            Quality == that.Quality;
    }

    public bool QualityInsensitiveEquals(Ingredient that)
    {
        if (that == null)
        {
            return false;
        }

        return IngredientName == that.IngredientName &&
            CurrentState == that.CurrentState;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(IngredientName, Quality, CurrentState);
    }
}
