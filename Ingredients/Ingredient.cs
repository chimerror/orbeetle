using Godot;
using GColl = Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Ingredient : Resource
{
    public Quality CurrentQuality { get; set; } = Quality.Okay;

    public Dictionary<State, Texture2D> TextureDictionary
    {
        get => AllowedStates.Zip(StateTextures).ToDictionary(g => g.First, g => g.Second);
    }

    public Texture2D CurrentTexture
    {
        get => TextureDictionary[CurrentState];
    }

    public bool RequiresButchering
    {
        get => AllowedStates.Contains(State.Butchered);
    }

    [Export]
    public string IngredientName { get; set; }

    [Export]
    public State CurrentState { get; set; } = State.Raw;

    [Export]
    public GColl.Array<State> AllowedStates { get; set; } = new GColl.Array<State>();

    [ExportGroup("Ingredient Visuals")]
    [Export]
    public GColl.Array<Texture2D> StateTextures { get; set; } = new GColl.Array<Texture2D>();

    public bool IsStateAllowed(State state)
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
            CurrentQuality == that.CurrentQuality;
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
        return HashCode.Combine(IngredientName, CurrentQuality, CurrentState);
    }

    public enum State
    {
        Raw,
        Chopped,
        Boiled,
        Butchered,
        Fermented,
        Grilled,
        Roasted,
        Fried,
        Mixed
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
