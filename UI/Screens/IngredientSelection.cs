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

    public static IngredientSelection Instance { get; private set; }

    [Export]
    public GColl.Array<Ingredient> StartingIngredients { get; set; } = new GColl.Array<Ingredient>();

    public override void _Ready()
    {
        Instance = this;
        _pantryDropSlot = GetNode<IngredientDropSlot>("%PantryDropSlot");
        _prepAreaDropSlot = GetNode<IngredientDropSlot>("%PrepAreaDropSlot");
        _platingAreaDropSlot = GetNode<IngredientDropSlot>("%PlatingAreaDropSlot");
        _serveButton = GetNode<Button>("%ServeButton");
        IngredientButton firstIngredient = null;
        foreach (var ingredient in StartingIngredients)
        {
            var ingredientButton = _ingredientButtonScene.Instantiate<IngredientButton>();
            var newIngredient = ingredient.Duplicate(true) as Ingredient;
            newIngredient.CurrentState = Ingredient.State.Raw;
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

        var focusElement = firstIngredientButton ?? _serveButton;
        GD.Print($"Taking focus in {Name} to {focusElement.Name} which has focus mode {focusElement.FocusMode}!");
        focusElement.GrabFocus();
    }

    public void LockNonChefSlots(bool lockSlots = true)
    {
        var ingredientButtons = _pantryDropSlot.CurrentIngredientButtons
            .Concat(_prepAreaDropSlot.CurrentIngredientButtons)
            .Concat(_platingAreaDropSlot.CurrentIngredientButtons);
        foreach (var ingredientButton in ingredientButtons)
        {
            ingredientButton.FocusMode = lockSlots ? Control.FocusModeEnum.None : Control.FocusModeEnum.All;
        }
        _serveButton.FocusMode = lockSlots ? Control.FocusModeEnum.None : Control.FocusModeEnum.All;
    }

    public void UnlockNonChefSlots()
    {
        LockNonChefSlots(false);
    }
}
