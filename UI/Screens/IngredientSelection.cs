using System.Linq;
using Godot;
using GColl = Godot.Collections;

public partial class IngredientSelection : Node2D
{
    private PackedScene _ingredientButtonScene =
        ResourceLoader.Load<PackedScene>("res://UI/Elements/IngredientButton.tscn");
    private IngredientDropSlot _pantryDropSlot;
    private IngredientDropSlot _prepAreaDropSlot;
    private IngredientDropSlot _platingAreaDropSlot;
    private Button _serveButton;

    [Export]
    public GColl.Array<Ingredient> StartingIngredients { get; set; } = new GColl.Array<Ingredient>();

    public override void _Ready()
    {
        _pantryDropSlot = GetNode<IngredientDropSlot>("%PantryDropSlot");
        _prepAreaDropSlot = GetNode<IngredientDropSlot>("%PrepAreaDropSlot");
        _platingAreaDropSlot = GetNode<IngredientDropSlot>("%PlatingAreaDropSlot");
        _serveButton = GetNode<Button>("%ServeButton");
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

    public void RetakeFocus()
    {
        var firstIngredientButton = _pantryDropSlot.CurrentIngredientButtons
            .Concat(_prepAreaDropSlot.CurrentIngredientButtons)
            .Concat(_platingAreaDropSlot.CurrentIngredientButtons)
            .FirstOrDefault();

        (firstIngredientButton ?? _serveButton).GrabFocus();
    }
}
