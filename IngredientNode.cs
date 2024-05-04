using Godot;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

[Tool]
public partial class IngredientNode : Node2D
{
	private const int CellSize = 32;
	private const string AnimalSheetPath = "res://Art/lpc-food-v2/animal-products.png";
	private const string VeggieSheetPath = "res://Art/lpc-food-v2/fruits-veggies.png";
	private static readonly Dictionary<IngredientType, IngredientSpriteData> ingredientDataDictionary = new()
	{
		{ IngredientType.Cheese, new IngredientSpriteData { spriteSheetPath = AnimalSheetPath, xCell = 1, yCell = 1 } },
		{ IngredientType.Eggs, new IngredientSpriteData { spriteSheetPath = AnimalSheetPath, xCell = 13, yCell = 1 } },
		{ IngredientType.Butter, new IngredientSpriteData { spriteSheetPath = AnimalSheetPath, xCell = 18, yCell = 1 } },
		{ IngredientType.Chicken, new IngredientSpriteData { spriteSheetPath = AnimalSheetPath, xCell = 2, yCell = 5 } },
		{ IngredientType.Deer, new IngredientSpriteData { spriteSheetPath = AnimalSheetPath, xCell = 5, yCell = 5 } },
		{ IngredientType.Steak, new IngredientSpriteData { spriteSheetPath = AnimalSheetPath, xCell = 12, yCell = 6 } },
		{ IngredientType.Bacon, new IngredientSpriteData { spriteSheetPath = AnimalSheetPath, xCell = 19, yCell = 6 } },
		{ IngredientType.Fish, new IngredientSpriteData { spriteSheetPath = AnimalSheetPath, xCell = 10, yCell = 17 } },
		{ IngredientType.Potatoes, new IngredientSpriteData { spriteSheetPath = VeggieSheetPath, xCell = 0, yCell = 1 } },
		{ IngredientType.Peppers, new IngredientSpriteData { spriteSheetPath = VeggieSheetPath, xCell = 4, yCell = 1 } },
		{ IngredientType.Tomatoes, new IngredientSpriteData { spriteSheetPath = VeggieSheetPath, xCell = 5, yCell = 1 } },
		{ IngredientType.Cabbage, new IngredientSpriteData { spriteSheetPath = VeggieSheetPath, xCell = 1, yCell = 6 } },
		{ IngredientType.Beans, new IngredientSpriteData { spriteSheetPath = VeggieSheetPath, xCell = 5, yCell = 11 } },
	};

	private IngredientType _ingredientType = IngredientType.Cheese;
	private Sprite2D _ingredientSprite;

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
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_ingredientSprite = GetNode<Sprite2D>("IngredientSprite");
		GD.Print($"{_ingredientSprite}");
		UpdateSprite();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void UpdateSprite()
	{
		GD.Print("loading new ingredient sprite...");
		if (_ingredientSprite != null)
		{
			var spriteData = ingredientDataDictionary[_ingredientType];
			var texture = new AtlasTexture
			{
				Atlas = GD.Load<CompressedTexture2D>(spriteData.spriteSheetPath),
				Region = new Rect2(spriteData.xCell * CellSize, spriteData.yCell * CellSize, CellSize, CellSize)
			};
			_ingredientSprite.Texture = texture;
		}
	}
}
