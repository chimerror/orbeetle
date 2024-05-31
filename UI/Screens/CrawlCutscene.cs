using Godot;
using GColl = Godot.Collections;
using GodotInk;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public partial class CrawlCutscene : Control
{
    private TextureRect _backgroundTextureRect;
    private Control _crawlControl;
    private Label _crawlLabel;
    private List<float> _orderedBackgroundPositions = new();
    private readonly List<string> _orderedBackgroundNames = new();
    private int _currentBackground = 0;

    [Export]
    public float CrawlSpeed { get; set; } = 0.9375f;

    [Export]
    public InkStory Story { get; set; }

    [Export]
    public GColl.Array<string> BackgroundNames { get; set; } = new();

    [Export]
    public GColl.Array<Texture2D> Backgrounds { get; set; } = new();

    private Dictionary<string, Texture2D> BackgroundDictionary
    {
        get => BackgroundNames.Zip(Backgrounds).ToDictionary(g => g.First, g => g.Second);
    }

    public override void _Ready()
    {
        _backgroundTextureRect = GetNode<TextureRect>("%BackgroundTextureRect");
        _crawlControl = GetNode<Control>("%CrawlControl");
        _crawlLabel = GetNode<Label>("%CrawlLabel");

        _crawlLabel.Position = _crawlLabel.Position with { Y = Size.Y };
        var backgroundCharacterPositions = new List<int>();
        var builder = new StringBuilder();
        while (Story.CanContinue)
        {
            var nextPart = Story.Continue();
            if (nextPart.StartsWith("BG "))
            {
                var backgroundName = nextPart.Replace("BG ", string.Empty).TrimEnd();
                backgroundCharacterPositions.Add(builder.Length);
                _orderedBackgroundNames.Add(backgroundName);
            }
            else
            {
                builder.Append(nextPart);
                builder.AppendLine();
            }
        }
        var maxLength = (float)builder.Length;
        _crawlLabel.Text = builder.ToString();

        var crawlSize = _crawlLabel.Size.Y;
        var screenMidpoint = Size.Y / 2.0f;
        // TODO: I don't know why I needed to add that fudge factor of 1.5 here.
        _orderedBackgroundPositions = backgroundCharacterPositions
            .Select(p => screenMidpoint - p / maxLength * _crawlLabel.Size.Y * 1.5f)
            .ToList();
    }

    public override void _Process(double delta)
    {
        var labelPosition = _crawlLabel.Position;
        labelPosition.Y -= CrawlSpeed;

        if (_currentBackground < _orderedBackgroundPositions.Count &&
            labelPosition.Y < _orderedBackgroundPositions[_currentBackground])
        {
            _backgroundTextureRect.Texture = BackgroundDictionary[_orderedBackgroundNames[_currentBackground]];
            _currentBackground++;
        }
        if (labelPosition.Y < -_crawlLabel.Size.Y)
        {
            labelPosition.Y = Size.Y;
        }
        _crawlLabel.Position = labelPosition;
    }
}
