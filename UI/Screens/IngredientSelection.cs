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
    private bool _mouseOverButton;

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
            ingredientButton.MouseEntered += () => _mouseOverButton = true;
            ingredientButton.MouseExited += () => _mouseOverButton = false;
            _pantryDropSlot.MoveToSlot(ingredientButton, false);
            firstIngredient ??= ingredientButton;
        }
        firstIngredient.GrabFocus();
    }

    public override void _Input(InputEvent @event)
    {
        var currentFocus = GetViewport().GuiGetFocusOwner();
        if (currentFocus == null &&
            (@event is InputEventJoypadButton || @event is InputEventJoypadMotion || @event is InputEventKey))
        {
            RetakeFocus();
            return; // Don't actually try to process until next frame, just grab focus
        }
        else if (currentFocus != null && !_mouseOverButton && @event is InputEventMouse)
        {
            currentFocus.ReleaseFocus();
        }
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
