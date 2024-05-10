using Godot;
using GColl = Godot.Collections;

public partial class IngredientSelection : Node2D
{
    private PackedScene _ingredientButtonScene =
        ResourceLoader.Load<PackedScene>("res://UI/Elements/IngredientButton.tscn");
    private IngredientDropSlot _pantryDropSlot;

    [Export]
    public GColl.Array<Ingredient> StartingIngredients { get; set; } = new GColl.Array<Ingredient>();

    public override void _Ready()
    {
        _pantryDropSlot = GetNode<IngredientDropSlot>("%PantryDropSlot");
        IngredientButton firstIngredient = null;
        foreach (var ingredient in StartingIngredients)
        {
            var ingredientButton = _ingredientButtonScene.Instantiate<IngredientButton>();
            var newIngredient = ingredient.Duplicate(true) as Ingredient;
            newIngredient.CurrentState = IngredientState.Raw;
            ingredientButton.Ingredient = newIngredient;
            _pantryDropSlot.MoveToSlot(ingredientButton, false);
            firstIngredient ??= ingredientButton;
        }
        firstIngredient.GrabFocus();
    }
}
