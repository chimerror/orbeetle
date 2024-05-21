using Godot;
using GodotInk;
using System;
using System.Text;

public partial class CrawlCutscene : Control
{
	private Control _crawlControl;
	private Label _crawlLabel;

	[Export]
	public float CrawlSpeed { get; set; } = 0.9375f;

	[Export]
	public InkStory Story;

	public override void _Ready()
	{
		_crawlControl = GetNode<Control>("%CrawlControl");
		_crawlLabel = GetNode<Label>("%CrawlLabel");

		_crawlLabel.Position = _crawlLabel.Position with { Y = Size.Y };
		var builder = new StringBuilder();
		while (Story.CanContinue)
		{
			builder.Append(Story.Continue());
			builder.AppendLine();
		}
		_crawlLabel.Text = builder.ToString();
	}

	public override void _Process(double delta)
	{
		var labelPosition = _crawlLabel.Position;
		labelPosition.Y -= CrawlSpeed;
		if (labelPosition.Y < -_crawlLabel.Size.Y)
		{
			labelPosition.Y = Size.Y;
		}
		_crawlLabel.Position = labelPosition;
	}
}
