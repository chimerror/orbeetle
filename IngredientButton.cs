using Godot;
using System;

public partial class IngredientButton : Button
{
    private int _buttonSize = 64;
    private TextureRect _textureRect;
    private Ingredient _ingredient;

    public Action<IngredientButton> UiAcceptCallback { get; set; }
    public Action<IngredientButton> UiCancelCallback { get; set; }

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
        if (@event.IsActionPressed("ui_accept"))
        {
            GD.Print($"ui_accept action triggered for {Name}!");
            UiAcceptCallback(this);
            GrabFocus();
        }
        else if (@event.IsActionPressed("ui_cancel"))
        {
            GD.Print($"ui_cancel action triggered for {Name}!");
            UiCancelCallback(this);
            GrabFocus();
        }
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
