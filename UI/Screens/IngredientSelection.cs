using Godot;
using GColl = Godot.Collections;
using System.Collections.Generic;

public partial class IngredientSelection : Node2D
{
    private const string RootNodePath = "CanvasLayer/RootHBox";
    private const int PantryIngredientSize = 64;
    private const int PrepAreaIngredientSize = 256;
    private const int PlatingAreaIngredientSize = 128;
    private PackedScene _ingredientButtonScene =
        ResourceLoader.Load<PackedScene>("res://UI/Elements/IngredientButton.tscn");
    private GridContainer _pantryGrid;
    private GridContainer _prepAreaGrid;
    private GridContainer _platingAreaGrid;

    [Export]
    public GColl.Array<Ingredient> StartingIngredients { get; set; } = new GColl.Array<Ingredient>();

    public override void _Ready()
    {
        _pantryGrid = GetNode<GridContainer>($"{RootNodePath}/LeftVBox/PantryGrid");
        _prepAreaGrid = GetNode<GridContainer>($"{RootNodePath}/MiddleVBox/PrepAreaGrid");
        _platingAreaGrid = GetNode<GridContainer>($"{RootNodePath}/RightVBox/PlatingAreaPanel/PlatingAreaGrid");
        IngredientButton firstIngredient = null;
        foreach (var ingredient in StartingIngredients)
        {
            var ingredientButton = _ingredientButtonScene.Instantiate<IngredientButton>();
            var newIngredient = ingredient.Duplicate(true) as Ingredient;
            newIngredient.CurrentState = IngredientState.Raw;
            ingredientButton.Ingredient = newIngredient;
            ingredientButton.ButtonSize = PantryIngredientSize;
            ingredientButton.UiAcceptCallback = MoveToPrepArea;
            ingredientButton.UiCancelCallback = DoNothing;
            _pantryGrid.AddChild(ingredientButton);
            firstIngredient ??= ingredientButton;
        }
        firstIngredient.GrabFocus();
    }

    public override void _Process(double delta)
    {
    }

    private static void DoNothing(IngredientButton _)
    {
    }

    private void MoveToPantry(IngredientButton ingredientButton)
    {
        GD.Print($"Moving {ingredientButton.Name} to Pantry!");
        ingredientButton.Reparent(_pantryGrid, false);
        ingredientButton.ButtonSize = PantryIngredientSize;
        ingredientButton.UiAcceptCallback = MoveToPrepArea;
        ingredientButton.UiCancelCallback = MoveToPrepArea;
    }

    private void MoveToPrepArea(IngredientButton ingredientButton)
    {
        GD.Print($"Moving {ingredientButton.Name} to Prep Area!");
        ingredientButton.Reparent(_prepAreaGrid, false);
        ingredientButton.ButtonSize = PrepAreaIngredientSize;
        ingredientButton.UiAcceptCallback = MoveToPlatingArea;
        ingredientButton.UiCancelCallback = MoveToPantry;
    }

    private void MoveToPlatingArea(IngredientButton ingredientButton)
    {
        GD.Print($"Moving {ingredientButton.Name} to Plating Area!");
        ingredientButton.Reparent(_platingAreaGrid, false);
        ingredientButton.ButtonSize = PlatingAreaIngredientSize;
        ingredientButton.UiAcceptCallback = MoveToPrepArea;
        ingredientButton.UiCancelCallback = MoveToPrepArea;
    }
}
