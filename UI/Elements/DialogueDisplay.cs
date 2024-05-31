using Godot;
using GColl = Godot.Collections;
using GodotInk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

public partial class DialogueDisplay : Control
{
    private readonly static Regex DirectiveRegex = new Regex(@"^(?<directive>BG|CHARACTER)\s*(?<parameters>.+)?$");
    private readonly static Regex CharacterParameters = new Regex(@"^(?<position>FAR_LEFT|LEFT|CENTER|RIGHT|FAR_RIGHT|OFF)(\s+(?<name>.+?))?(\s+MOOD (?<mood>[\w-]+))?$");
    private readonly static Regex DialogueRegex = new Regex(@"^(?<speaker>.+):\s+(?<dialogue>.*)$");

    private TextureRect _backgroundTextureRect;
    private Dictionary<string, DialogueCharacter> _currentlyDisplayedCharacters = new();
    private DialogueCharacter _farLeftCharacter;
    private DialogueCharacter _leftCharacter;
    private DialogueCharacter _centerCharacter;
    private DialogueCharacter _rightCharacter;
    private DialogueCharacter _farRightCharacter;
    private Control _dialogueBox;
    private PanelContainer _speakerPanelContainer;
    private Label _speakerLabel;
    private RichTextLabel _dialogueLabel;
    private PanelContainer _choicesPanelContainer;
    private VBoxContainer _choicesVBox;
    private bool _mouseOverChoice = false;

    public Dictionary<string, Character> CharacterDictionary
    {
        get => AvailableCharacters.ToDictionary(c => c.ShortName, c => c);
    }

    public Dictionary<string, Texture2D> BackgroundDictionary
    {
        get => BackgroundNames.Zip(BackgroundTextures).ToDictionary(g => g.First, g => g.Second);
    }

    [Export]
    public InkStory Story { get; set; }

    // TODO: This blank character is necessary because I have used an HBox as the container, and if I mark a character
    // as not visible, it will shift the others. I think there may be a grid container that will work better here, but
    // I'm fine with doing it this way to start.
    [Export]
    public Character BlankCharacter { get; set; }

    [Export]
    public GColl.Array<Character> AvailableCharacters { get; set; }

    [Export]
    public GColl.Array<string> BackgroundNames { get; set; }

    [Export]
    public GColl.Array<Texture2D> BackgroundTextures { get; set; }

    public override void _Ready()
    {
        _backgroundTextureRect = GetNode<TextureRect>("%BackgroundTextureRect");
        _farLeftCharacter = GetNode<DialogueCharacter>("%FarLeftCharacter");
        _leftCharacter = GetNode<DialogueCharacter>("%LeftCharacter");
        _centerCharacter = GetNode<DialogueCharacter>("%CenterCharacter");
        _rightCharacter = GetNode<DialogueCharacter>("%RightCharacter");
        _farRightCharacter = GetNode<DialogueCharacter>("%FarRightCharacter");
        _dialogueBox = GetNode<Control>("%DialogueBox");
        _speakerPanelContainer = GetNode<PanelContainer>("%SpeakerPanelContainer");
        _speakerLabel = GetNode<Label>("%SpeakerLabel");
        _dialogueLabel = GetNode<RichTextLabel>("%DialogueLabel");
        _choicesPanelContainer = GetNode<PanelContainer>("%ChoicesPanelContainer");
        _choicesVBox = GetNode<VBoxContainer>("%ChoicesVBox");

        ContinueStory();
    }

    public override void _Input(InputEvent @event)
    {
        var currentFocus = GetViewport().GuiGetFocusOwner();
        if (_choicesPanelContainer.Visible)
        {
            if (currentFocus == null &&
                (@event is InputEventJoypadButton || @event is InputEventJoypadMotion || @event is InputEventKey))
            {
                _choicesVBox.GetChild<Button>(0).GrabFocus();
                return; // Don't actually try to process the event until next frame, just grab focus
            }
            else if (currentFocus != null && !_mouseOverChoice && @event is InputEventMouse)
            {
                GD.Print("DEBUG: Releasing keyboard focus in favor of mouse!");
                currentFocus.ReleaseFocus();
            }
        }
        else
        {
            if (@event.IsActionPressed("ui_accept") ||
                (@event is InputEventMouseButton mouseEvent &&
                mouseEvent.Pressed &&
                mouseEvent.ButtonIndex == MouseButton.Left))
            {
                ContinueStory();
            }
        }
    }

    private void ContinueStory(int choice = -1)
    {
        if (choice > -1)
        {
            Story.ChooseChoiceIndex(choice);
        }

        var rawText = Story.CanContinue ? Story.Continue() : null;

        while (rawText != null && DirectiveRegex.IsMatch(rawText))
        {
            PerformDirective(rawText);
            rawText = Story.CanContinue ? Story.Continue() : null;
        }

        if (rawText != null)
        {
            _choicesPanelContainer.Visible = false;
            UpdateDialogue(rawText, Story.CurrentTags);
            _dialogueBox.Visible = true;
        }
        else if (Story.CurrentChoices.Count > 0)
        {
            _dialogueBox.Visible = false;
            UpdateChoices();
            _choicesPanelContainer.Visible = true;
        }
        else
        {
            // TODOTODO: handle hitting end of script
        }
    }

    private void PerformDirective(string rawText)
    {
        var match = DirectiveRegex.Match(rawText);
        if (!match.Success)
        {
            GD.PrintErr($"Unknown directive sent to PerformDirective: {rawText}");
        }

        var parameters = match.Groups["parameters"].Value;
        switch (match.Groups["directive"].Value)
        {
            case "BG":
                UpdateBackground(parameters);
                return;

            case "CHARACTER":
                HandleCharacter(parameters);
                return;

            default:
                GD.PrintErr($"Unimplemented directive: {rawText}");
                return;
        }
    }

    private void UpdateBackground(string parameters)
    {
        GD.Print($"Loading background '{parameters}'");
        _backgroundTextureRect.Texture = BackgroundDictionary[parameters];
    }

    private void HandleCharacter(string parameters)
    {
        var match = CharacterParameters.Match(parameters);
        if (!match.Success)
        {
            GD.PrintErr($"Invalid CHARACTER directive parameters: {parameters}");
            return;
        }

        var position = match.Groups["position"].Value;
        if (position.Equals("OFF")) // Turns all characters off
        {
        }

        DialogueCharacter positionCharacter;
        switch (position)
        {
            case "OFF": // Turns all characters off
                ResetToBlankCharacter(_farLeftCharacter);
                ResetToBlankCharacter(_leftCharacter);
                ResetToBlankCharacter(_centerCharacter);
                ResetToBlankCharacter(_rightCharacter);
                ResetToBlankCharacter(_farRightCharacter);
                return;

            case "FAR_LEFT":
                positionCharacter = _farLeftCharacter;
                break;

            case "LEFT":
                positionCharacter = _leftCharacter;
                break;

            case "CENTER":
                positionCharacter = _centerCharacter;
                break;

            case "RIGHT":
                positionCharacter = _rightCharacter;
                break;

            case "FAR_RIGHT":
                positionCharacter = _farRightCharacter;
                break;

            default:
                GD.PrintErr($"Invalid CHARACTER directive position parameter, placing in center: {position}");
                positionCharacter = _centerCharacter;
                break;
        }

        _currentlyDisplayedCharacters.Remove(positionCharacter.Character.ShortName);

        if (!match.Groups["name"].Success)
        {
            GD.PrintErr($"No character name provided for non-OFF position: {parameters}");
            return;
        }

        var name = match.Groups["name"].Value.ToLowerInvariant();
        if (name.Equals("off"))
        {
            ResetToBlankCharacter(positionCharacter);
            return;
        }

        if (_currentlyDisplayedCharacters.ContainsKey(name) && _currentlyDisplayedCharacters[name] != positionCharacter)
        {
            ResetToBlankCharacter(_currentlyDisplayedCharacters[name]);
        }

        if (!CharacterDictionary.ContainsKey(name))
        {
            GD.PrintErr($"No character of name '{name}' among available characters during CHARACTER directive");
            return;
        }
        positionCharacter.Character = CharacterDictionary[name];
        _currentlyDisplayedCharacters[name] = positionCharacter;

        var mood = match.Groups["mood"].Success ? match.Groups["mood"].Value : "neutral";
        positionCharacter.CurrentMood = mood;
    }

    private void ResetToBlankCharacter(DialogueCharacter dialogueCharacter)
    {
        dialogueCharacter.CurrentMood = "neutral"; // Have to set mood first since the blank has only neutral
        dialogueCharacter.Character = BlankCharacter;
    }

    private void UpdateDialogue(string rawText, IReadOnlyList<string> currentTags)
    {
        string speaker = null;
        string dialogue = null;
        var match = DialogueRegex.Match(rawText);
        if (match.Success)
        {
            speaker = match.Groups["speaker"].Value.ToLowerInvariant();
            dialogue = match.Groups["dialogue"].Value;
        }
        else
        {
            dialogue = rawText;
        }

        if (speaker != null && !CharacterDictionary.ContainsKey(speaker))
        {
            GD.PrintErr($"No character of name '{speaker}' among available characters during dialogue");
            return;
        }

        if (speaker != null)
        {
            _speakerPanelContainer.Visible = true;
            _speakerLabel.Text = CharacterDictionary[speaker].FullDialogueName;
        }
        else
        {
            _speakerPanelContainer.Visible = false;
        }

        _dialogueLabel.Text = dialogue;

        var mood = currentTags.FirstOrDefault();
        if (mood != null)
        {
            if (speaker == null || !_currentlyDisplayedCharacters.ContainsKey(speaker))
            {
                GD.PrintErr($"Attempted to set mood of character '{speaker}' when no such speaker was displayed.");
                return;
            }
            _currentlyDisplayedCharacters[speaker].CurrentMood = mood;
        }
    }

    private void UpdateChoices()
    {
        foreach (var child in _choicesVBox.GetChildren())
        {
            child.QueueFree();
        }

        foreach (var choice in Story.CurrentChoices)
        {
            Button choiceButton = new() { Text = choice.Text };
            choiceButton.Pressed += () => ContinueStory(choice.Index);
            choiceButton.MouseEntered += () => _mouseOverChoice = true;
            choiceButton.MouseExited += () => _mouseOverChoice = false;
            choiceButton.ThemeTypeVariation = "ChoiceButton";
            _choicesVBox.AddChild(choiceButton);
        }
    }
}
