using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class IngredientDropSlot : PanelContainer
{
    public IEnumerable<IngredientButton> CurrentIngredientButtons
    {
        get => DestinationSlot.GetChildren().Where(c => c is IngredientButton).Cast<IngredientButton>();
    }

    public IEnumerable<Ingredient> CurrentIngredients
    {
        get => CurrentIngredientButtons.Select(ib => ib.Ingredient);
    }

    public bool CanGoLeft
    {
        get => LeftSlot != null;
    }

    public bool CanGoRight
    {
        get => RightSlot != null;
    }

    public bool CanGoUp
    {
        get => UpSlot != null;
    }

    public bool CanGoDown
    {
        get => DownSlot != null;
    }

    [Export]
    public int IngredientSize { get; set; } = 64;

    [Export]
    public Container DestinationSlot { get; set; }

    [Export]
    public IngredientDropSlot LeftSlot { get; set; }

    [Export]
    public IngredientDropSlot RightSlot { get; set; }

    [Export]
    public IngredientDropSlot UpSlot { get; set; }

    [Export]
    public IngredientDropSlot DownSlot { get; set; }

    public override bool _CanDropData(Vector2 atPosition, Variant data)
    {
        return true;
    }

    public override void _DropData(Vector2 atPosition, Variant data)
    {
        MoveToSlot(data.As<IngredientButton>(), true);
    }

    public virtual void MoveToSlot(IngredientButton ingredientButton, bool grabFocus = false)
    {
        ingredientButton.ButtonSize = IngredientSize;
        ingredientButton.ForceReparent(DestinationSlot, false);

        var oldSlot = ingredientButton.CurrentSlot;
        if (oldSlot is ChefActionDropSlot chefActionDropSlot)
        {
            chefActionDropSlot.ResetAfterIngredientTaken();
        }

        ingredientButton.CurrentSlot = this;
        if (grabFocus)
        {
            ingredientButton.GrabFocus();
        }
    }

    public void RetakeFocus()
    {
        var firstButton = CurrentIngredientButtons.FirstOrDefault();
        if (firstButton != default)
        {
            firstButton.GrabFocus();
        }
        else
        {
            // A bit worried about having to use the path here but it'll have to do.
            GetNode<IngredientSelection>("/root/IngredientSelection").RetakeFocus();
        }
    }
}
