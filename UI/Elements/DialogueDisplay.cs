using Godot;
using GColl = Godot.Collections;
using GodotInk;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

public partial class DialogueDisplay : Control
{
    private const float MaxDialogueCharacters = 200.0f;
    private readonly static Regex DirectiveRegex = new(@"^(?<directive>BG|CHARACTER)\s*(?<parameters>.+)?$");
    private readonly static Regex CharacterParameters = new(@"^(?<position>FAR_LEFT|LEFT|CENTER|RIGHT|FAR_RIGHT|OFF)(\s+(?<name>.+?))?(\s+MOOD (?<mood>[\w-]+))?$");
    private readonly static Regex DialogueRegex = new(@"^(?<speaker>.+):\s+(?<dialogue>.*)$");

    private readonly Dictionary<string, DialogueCharacter> _currentlyDisplayedCharacters = new();
    private TextureRect _backgroundTextureRect;
    private DialogueCharacter _farLeftCharacter;
    private DialogueCharacter _leftCharacter;
    private DialogueCharacter _centerCharacter;
    private DialogueCharacter _rightCharacter;
    private DialogueCharacter _farRightCharacter;
    private Control _dialogueBox;
    private PanelContainer _speakerPanelContainer;
    private Label _speakerLabel;
    private RichTextLabel _dialogueLabel;
    private AnimationTree _dialogueAnimationTree;
    private TextureRect _dialogueFinishedTextureRect;
    private TextureButton _skipButton;
    private bool _autoAdvanceEnabled = false;
    private TextureButton _autoAdvanceButton;
    private AnimationPlayer _autoAdvanceButtonAnimationPlayer;
    private PanelContainer _choicesPanelContainer;
    private VBoxContainer _choicesVBox;
    private ColorRect _confirmationDialogColorRect;
    private Label _confirmationDialogLabel;
    private CheckBox _confirmationDialogCheckBox;
    private Button _confirmationDialogYesButton;
    private Button _confirmationDialogNoButton;
    private bool _mouseOverElement = false;

    [Signal]
    public delegate void DialogueStartedEventHandler(string knotName);

    [Signal]
    public delegate void DialogueFinishedEventHandler();

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

    // Not very happy with having to use this blank character versus setting it to be not visible, but it seems that
    // even a GridContainer would have the issue that it collapses in the others when a child is not visible.
    [Export]
    public Character BlankCharacter { get; set; }

    [Export]
    public GColl.Array<Character> AvailableCharacters { get; set; }

    [Export]
    public GColl.Array<string> BackgroundNames { get; set; }

    [Export]
    public GColl.Array<Texture2D> BackgroundTextures { get; set; }

    // Speeds less than or equal to 0 are treated as instant
    [Export(PropertyHint.Range, "0,8,0.25,or_greater,orless")]
    public float DialogueSpeed { get; set; } = 1.0f;

    [Export]
    public bool ConfirmBeforeSkipping { get; set; } = true;

    [Export]
    public string SkippingConfirmationDialogMessage { get; set; }

    [Export]
    public string SkippingConfirmationDialogCheckboxString { get; set; }

    [Export]
    public bool ConfirmBeforeAutoAdvance { get; set; } = true;

    [Export]
    public string AutoAdvanceEnablingConfirmationDialogMessage { get; set; }

    [Export]
    public string AutoAdvanceDisablingConfirmationDialogMessage { get; set; }

    [Export]
    public string AutoAdvanceConfirmationDialogCheckboxString { get; set; }

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
        _dialogueAnimationTree = GetNode<AnimationTree>("%DialogueAnimationTree");
        _dialogueAnimationTree.Active = true;
        _dialogueAnimationTree.Set("parameters/conditions/instantDialogue", DialogueSpeed <= 0.0f);
        _dialogueFinishedTextureRect = GetNode<TextureRect>("%DialogueFinishedTextureRect");
        _skipButton = GetNode<TextureButton>("%SkipButton");
        _skipButton.Pressed += OnSkipButtonPressed;
        _autoAdvanceButton = GetNode<TextureButton>("%AutoAdvanceButton");
        _autoAdvanceButton.Pressed += OnAutoAdvanceButtonPressed;
        _autoAdvanceButtonAnimationPlayer = GetNode<AnimationPlayer>("%AutoAdvanceButtonAnimationPlayer");
        _choicesPanelContainer = GetNode<PanelContainer>("%ChoicesPanelContainer");
        _choicesVBox = GetNode<VBoxContainer>("%ChoicesVBox");
        _confirmationDialogColorRect = GetNode<ColorRect>("%ConfirmationDialogColorRect");
        _confirmationDialogLabel = GetNode<Label>("%ConfirmationDialogLabel");
        _confirmationDialogCheckBox = GetNode<CheckBox>("%ConfirmationDialogCheckBox");
        _confirmationDialogCheckBox.MouseEntered += () => _mouseOverElement = true;
        _confirmationDialogCheckBox.MouseExited += () => _mouseOverElement = false;
        _confirmationDialogYesButton = GetNode<Button>("%ConfirmationDialogYesButton");
        _confirmationDialogYesButton.MouseEntered += () => _mouseOverElement = true;
        _confirmationDialogYesButton.MouseExited += () => _mouseOverElement = false;
        _confirmationDialogYesButton.Pressed += HideConfirmationDialog;
        _confirmationDialogNoButton = GetNode<Button>("%ConfirmationDialogNoButton");
        _confirmationDialogNoButton.MouseEntered += () => _mouseOverElement = true;
        _confirmationDialogNoButton.MouseExited += () => _mouseOverElement = false;
        _confirmationDialogNoButton.Pressed += HideConfirmationDialog;

        DialogueStarted += OnDialogueStarted;
        DialogueFinished += OnDialogueFinished;

        ContinueStory();
    }

    public override void _Input(InputEvent @event)
    {
        if (_choicesPanelContainer.Visible)
        {
            if (ManageFocusOnEvent(@event, _choicesVBox.GetChild<Button>(0)))
            {
                return;
            }
        }
        else if (_confirmationDialogColorRect.Visible)
        {
            if (ManageFocusOnEvent(@event, _confirmationDialogNoButton))
            {
                return;
            }
        }
        else
        {
            if (@event.IsActionPressed("ui_accept"))
            {
                FinishWritingOrAdvanceStory();
                AcceptEvent();
            }
            else if (@event.IsActionPressed("dialogue_skip"))
            {
                OnSkipButtonPressed();
            }
            else if (@event.IsActionPressed("dialogue_toggle_auto_advance"))
            {
                OnAutoAdvanceButtonPressed();
            }
        }
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent &&
            mouseEvent.Pressed &&
            mouseEvent.ButtonIndex == MouseButton.Left)
        {
            FinishWritingOrAdvanceStory();
            AcceptEvent();
        }
    }

    public void StartDialogue(string knotName)
    {
        EmitSignal(SignalName.DialogueStarted, knotName);
        AcceptEvent(); // To consume the button press
    }

    public void AutoAdvanceIfPossible()
    {
        if (_autoAdvanceEnabled && !_choicesPanelContainer.Visible && !_confirmationDialogColorRect.Visible)
        {
            ContinueStory();
        }
    }

    private bool ManageFocusOnEvent(InputEvent @event, Control controlToFocus)
    {
        var currentFocus = GetViewport().GuiGetFocusOwner();
        if (currentFocus == null &&
            (@event is InputEventJoypadButton || @event is InputEventJoypadMotion || @event is InputEventKey))
        {
            GD.Print("Moving keyboard focus to desired control!");
            controlToFocus.GrabFocus();
            return true; // Don't actually try to process the event until next frame, just grab focus
        }
        else if (currentFocus != null && !_mouseOverElement && @event is InputEventMouse)
        {
            GD.Print("Releasing keyboard focus in favor of mouse!");
            currentFocus.ReleaseFocus();
        }
        return false;
    }

    private void OnDialogueStarted(string knotName)
    {
        ProcessMode = ProcessModeEnum.Always;
        Visible = true;
        Story.ChoosePathString(knotName);
        ContinueStory();
    }

    private void OnDialogueFinished()
    {
        // TODO: Hook this up to advance the scene when in a normal dialogue cutscene
        ProcessMode = ProcessModeEnum.Disabled;
        Visible = false;
    }

    private bool FinishWritingIfNeeded()
    {
        var animationPlayback =
            (AnimationNodeStateMachinePlayback)_dialogueAnimationTree.Get("parameters/playback");
        if (animationPlayback.GetCurrentNode() == "WriteDialogue")
        {
            animationPlayback.Next();
            return true;
        }
        else
        {
            return false;
        }
    }

    private void FinishWritingOrAdvanceStory()
    {
        if (!FinishWritingIfNeeded())
        {
            ContinueStory();
        }
    }

    private void ContinueStory(int choice = -1, bool skip = false)
    {
        if (choice > -1)
        {
            Story.ChooseChoiceIndex(choice);
        }

        string rawText = Story.CanContinue ? Story.Continue() : null;
        while (rawText != null)
        {
            if (DirectiveRegex.IsMatch(rawText))
            {
                PerformDirective(rawText);
            }
            else if (!skip)
            {
                break;
            }
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
            EmitSignal(SignalName.DialogueFinished);
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
        if (parameters.ToLowerInvariant() == "off")
        {
            GD.Print($"Setting background to off...");
            _backgroundTextureRect.Texture = null;
        }
        else
        {
            GD.Print($"Loading background '{parameters}'");
            _backgroundTextureRect.Texture = BackgroundDictionary[parameters];
        }
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
        string dialogue;
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
        var dialogueSpeedMultiplier = DialogueSpeed > 0.0f ? DialogueSpeed : 1f;
        var computedDialogueSpeed = MaxDialogueCharacters / dialogue.Length * dialogueSpeedMultiplier;
        if (DialogueSpeed <= 0.0f)
        {
            // Only speed up or slow down the instant animation when auto-advance is enabled, otherwise keep the normal
            // speed
            if (!_autoAdvanceEnabled)
            {
                computedDialogueSpeed = 1.0f;
            }
            _dialogueAnimationTree.Set("parameters/conditions/instantDialogue", true);
            _dialogueAnimationTree.Set("parameters/InstantWriteDialogue/TimeScale/scale", computedDialogueSpeed);
        }
        else
        {
            _dialogueAnimationTree.Set("parameters/conditions/instantDialogue", false);
            _dialogueLabel.VisibleRatio = 0f; // Otherwise, there will be a flash of the new text during auto-advance
            _dialogueAnimationTree.Set("parameters/WriteDialogue/TimeScale/scale", computedDialogueSpeed);
        }

        var animationPlayback =
            (AnimationNodeStateMachinePlayback)_dialogueAnimationTree.Get("parameters/playback");
        animationPlayback.Start("Start", true);

        var mood = currentTags.Count > 0 ? currentTags[0] : null;
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
            choiceButton.MouseEntered += () => _mouseOverElement = true;
            choiceButton.MouseExited += () => _mouseOverElement = false;
            choiceButton.ThemeTypeVariation = "ChoiceButton";
            _choicesVBox.AddChild(choiceButton);
        }
    }

    private void HideConfirmationDialog()
    {
        _confirmationDialogColorRect.Visible = false;
    }

    private void OnSkipButtonPressed()
    {
        FinishWritingIfNeeded();
        if (ConfirmBeforeSkipping)
        {
            _confirmationDialogLabel.Text = SkippingConfirmationDialogMessage;
            _confirmationDialogCheckBox.Text = SkippingConfirmationDialogCheckboxString;
            _confirmationDialogCheckBox.SetPressedNoSignal(true);
            _confirmationDialogCheckBox.DisconnectSignalIfNeeded(
                CheckBox.SignalName.Toggled, Callable.From<bool>(ToggleAutoAdvanceConfirmation));
            _confirmationDialogCheckBox.ConnectSignalIfNeeded(
                CheckBox.SignalName.Toggled, Callable.From<bool>(ToggleSkipConfirmation));
            _confirmationDialogYesButton.DisconnectSignalIfNeeded(
                Button.SignalName.Pressed, Callable.From(ToggleAutoAdvance));
            _confirmationDialogYesButton.ConnectSignalIfNeeded(
                Button.SignalName.Pressed, Callable.From(PerformSkip));
            _confirmationDialogColorRect.Visible = true;
        }
        else
        {
            PerformSkip();
        }
    }

    private void ToggleSkipConfirmation(bool state)
    {
        ConfirmBeforeSkipping = state;
    }

    private void PerformSkip()
    {
        GD.Print($"Skipping to next choice...");
        ContinueStory(-1, true);
        AcceptEvent();
    }

    private void OnAutoAdvanceButtonPressed()
    {
        if (ConfirmBeforeAutoAdvance)
        {
            FinishWritingIfNeeded();
            _confirmationDialogLabel.Text = _autoAdvanceEnabled ?
                AutoAdvanceDisablingConfirmationDialogMessage :
                AutoAdvanceEnablingConfirmationDialogMessage;
            _confirmationDialogCheckBox.Text = AutoAdvanceConfirmationDialogCheckboxString;
            _confirmationDialogCheckBox.SetPressedNoSignal(true);
            _confirmationDialogCheckBox.DisconnectSignalIfNeeded(
                CheckBox.SignalName.Toggled, Callable.From<bool>(ToggleSkipConfirmation));
            _confirmationDialogCheckBox.ConnectSignalIfNeeded(
                CheckBox.SignalName.Toggled, Callable.From<bool>(ToggleAutoAdvanceConfirmation));
            _confirmationDialogYesButton.DisconnectSignalIfNeeded(
                Button.SignalName.Pressed, Callable.From(PerformSkip));
            _confirmationDialogYesButton.ConnectSignalIfNeeded(
                Button.SignalName.Pressed, Callable.From(ToggleAutoAdvance));
            _confirmationDialogColorRect.Visible = true;
        }
        else
        {
            ToggleAutoAdvance();
        }
    }

    private void ToggleAutoAdvanceConfirmation(bool state)
    {
        ConfirmBeforeAutoAdvance = state;
    }

    private void ToggleAutoAdvance()
    {
        _autoAdvanceEnabled = !_autoAdvanceEnabled;
        var animationToPlay = _autoAdvanceEnabled ? "AutoAdvanceEnabled" : "RESET";
        var currentFocus = GetViewport().GuiGetFocusOwner();
        if (currentFocus == _autoAdvanceButton)
        {
            _autoAdvanceButton.ReleaseFocus(); // Don't ever want button to retain focus
        }
        _autoAdvanceButtonAnimationPlayer.Play(animationToPlay);
    }
}
