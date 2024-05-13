using Godot;

public partial class IngredientButton : Button
{
    private int _buttonSize = 64;
    private TextureRect _textureRect;
    private Ingredient _ingredient;

    public IngredientDropSlot CurrentSlot { get; set; }

    [Export]
    public int ButtonSize
    {
        get => _buttonSize;
        set
        {
            var needUpdate = value != _buttonSize;
            _buttonSize = value;
            if (needUpdate)
            {
                UpdateSize();
            }
        }
    }

    [Export]
    public Ingredient Ingredient
    {
        get => _ingredient;
        set
        {
            var needUpdate = value != _ingredient;
            _ingredient = value;
            if (needUpdate)
            {
                UpdateTexture();
            }
        }
    }

    public override void _Ready()
    {
        _textureRect = GetNode<TextureRect>("TextureRect");
        UpdateTexture();
        UpdateSize();
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (@event.IsActionPressed("cooking_send_left") && CurrentSlot.CanGoLeft)
        {
            GD.Print($"Sending {Name} to the left!");
            CurrentSlot.LeftSlot.MoveToSlot(this, true);
        }
        else if (@event.IsActionPressed("cooking_send_right") && CurrentSlot.CanGoRight)
        {
            GD.Print($"Sending {Name} to the right!");
            CurrentSlot.RightSlot.MoveToSlot(this, true);
        }
        else if (@event.IsActionPressed("cooking_send_up"))
        {
            if (CurrentSlot.UpSlot is ChefsDePartieDropSlot)
            {
                // TODOTODO: When sending up to the chef actions, a non-mouse player will have to make a second choice
                // of which action to send it to before sending it. This seems complicated enough that there may need to
                // be a new state to represent when an ingredient is being aimed at an action.
            }
            else if (CurrentSlot.CanGoUp)
            {
                GD.Print($"Sending {Name} up!");
                CurrentSlot.UpSlot.MoveToSlot(this, true);
            }
        }
        else if (@event.IsActionPressed("cooking_send_down") && CurrentSlot.CanGoDown)
        {
            GD.Print($"Sending {Name} down!");
            CurrentSlot.DownSlot.MoveToSlot(this, true);
        }

        // Same button on controller as cooking_send_down, so we don't want to have it propagate to the button, because
        // that will cause it to do a clicking animation and nothing else.
        if (@event.IsActionPressed("ui_accept"))
        {
            AcceptEvent();
        }
    }

    public override Variant _GetDragData(Vector2 atPosition)
    {
        var preview = new TextureRect
        {
            Texture = _textureRect.Texture,
            Size = new Vector2(_buttonSize, _buttonSize)
        };
        SetDragPreview(preview);
        return this;
    }

    private void UpdateSize()
    {
        var sizeVector = new Vector2(_buttonSize, _buttonSize);
        if (_textureRect != null)
        {
            _textureRect.Size = sizeVector;
        }
        Size = sizeVector;
        CustomMinimumSize = sizeVector;
    }

    private void UpdateTexture()
    {
        if (_textureRect != null)
        {
            _textureRect.Texture = _ingredient.CurrentTexture;
        }
    }
}
