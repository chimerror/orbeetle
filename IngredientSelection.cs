using Godot;
using System;

public partial class IngredientSelection : Node2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GetNode<Button>("CanvasLayer/RootHBox/LeftVBox/IngredientsGrid/IngredientButton").GrabFocus();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
