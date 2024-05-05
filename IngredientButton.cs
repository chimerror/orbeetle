using Godot;
using static IngredientTexture;

[Tool]
public partial class IngredientButton : Button
{
	private IngredientType _ingredientType = IngredientType.Cheese;

	private TextureRect _textureRect;

	[Export]
	public IngredientType IngredientType
	{
		get => _ingredientType;
		set
		{
			var needUpdate = value != _ingredientType;
			_ingredientType = value;
			if (needUpdate) {
				UpdateTexture();
			}
		}
	}

	public override void _Ready()
	{
		_textureRect = GetNode<TextureRect>("TextureRect");
		UpdateTexture();
	}

	public override void _Process(double delta)
	{
	}

	private void UpdateTexture()
	{
		if (_textureRect != null)
		{
			_textureRect.Texture = GetIngredientTexture(_ingredientType);
		}
	}
}
