using Godot;
using System;

public partial class HungerLabel : Label
{
	private const string HungerText = "My stomach is rumbling...";
	private const double FullTextTimeout = 2.0;
	private int _currentDisplayedLength = 0;
	private double _currentFullTextTimeout = 0.0;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		_currentDisplayedLength++;
		if (_currentDisplayedLength >= HungerText.Length)
		{
			if (_currentFullTextTimeout >= FullTextTimeout)
			{
				_currentDisplayedLength = 0;
				_currentFullTextTimeout = 0.0;
			}
			else
			{
				_currentFullTextTimeout += delta;
				_currentDisplayedLength = HungerText.Length;
			}
		}
		Text = HungerText[0..(_currentDisplayedLength)];
	}
}
