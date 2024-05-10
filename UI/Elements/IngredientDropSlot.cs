using Godot;

public partial class IngredientDropSlot : PanelContainer
{
    private Container _destination;

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
    public IngredientDropSlot LeftSlot { get; set; }

    [Export]
    public IngredientDropSlot RightSlot { get; set; }

    [Export]
    public IngredientDropSlot UpSlot { get; set; }

    [Export]
    public IngredientDropSlot DownSlot { get; set; }

    public override void _Ready()
    {
        foreach (var child in GetChildren())
        {
            if (child is Container containerChild)
            {
                _destination = containerChild;
                break;
            }
        }
    }

    public override bool _CanDropData(Vector2 atPosition, Variant data)
    {
        return true;
    }

    public override void _DropData(Vector2 atPosition, Variant data)
    {
        MoveToSlot(data.As<IngredientButton>(), true);
    }

    public void MoveToSlot(IngredientButton ingredientButton, bool grabFocus = false)
    {
        ingredientButton.ButtonSize = IngredientSize;
        ingredientButton.ForceReparent(_destination, false);
        ingredientButton.CurrentSlot = this;
        if (grabFocus)
        {
            ingredientButton.GrabFocus();
        }
    }

}
