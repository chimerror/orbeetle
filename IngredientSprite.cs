using Godot;
using static IngredientTexture;

[Tool]
public partial class IngredientSprite : Sprite2D
{
	private IngredientType _ingredientType = IngredientType.Cheese;

	[Export]
	public IngredientType IngredientType
	{
		get => _ingredientType;
		set
		{
			var needUpdate = value != _ingredientType;
			_ingredientType = value;
			if (needUpdate) {
				UpdateSprite();
			}
		}
	}

	public override void _Ready()
	{
		UpdateSprite();
	}

	public override void _Process(double delta)
	{
	}

	private void UpdateSprite()
	{
		Texture = GetIngredientTexture(_ingredientType);
	}
}
