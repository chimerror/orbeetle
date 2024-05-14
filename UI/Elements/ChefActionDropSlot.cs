using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class ChefActionDropSlot : IngredientDropSlot
{
    private Label _actionVerbLabel;
    private Timer _actionTimer;
    private ProgressBar _actionProgressBar;
    private MarginContainer _actionIngredientMarginContainer;
    private PanelContainer _actionIngredientPanelContainer;
    private State _currentState = State.Waiting;
    private readonly List<Ingredient> _ingredientsToProcess = new();

    public bool HasIngredient
    {
        get => CurrentIngredientButton != null;
    }

    public IngredientButton CurrentIngredientButton
    {
        get => DestinationSlot.GetChildOrNull<IngredientButton>(0);
    }

    [Export]
    public ChefsDePartieDropSlot ParentChefsDePartieDropSlot { get; set; }

    [Export]
    public ChefAction ChefAction { get; set; }

    [ExportGroup("Progress Bar Visuals")]
    [Export]
    public StyleBoxFlat DefaultStyleBox { get; set; }

    [Export]
    public StyleBoxFlat SelectedStyleBox { get; set; }

    [Export]
    public StyleBoxFlat ReadyStyleBox { get; set; }

    public override void _Ready()
    {
        FocusEntered += OnFocusEntered;
        FocusExited += OnFocusExited;
        MouseEntered += OnMouseEntered;
        MouseExited += OnMouseExited;

        _actionVerbLabel = GetNode<Label>("%ActionVerbLabel");
        _actionVerbLabel.Text = ChefAction.ActionVerb;

        _actionTimer = GetNode<Timer>("%ActionTimer");
        _actionTimer.WaitTime = ChefAction.Duration;
        _actionTimer.Timeout += OnIngredientReady;

        _actionProgressBar = GetNode<ProgressBar>("%ActionProgressBar");

        _actionIngredientMarginContainer = GetNode<MarginContainer>("%ActionIngredientMarginContainer");
        _actionIngredientPanelContainer = GetNode<PanelContainer>("%ActionIngredientPanelContainer");
    }

    public override void _Notification(int what)
    {
        if (what == NotificationDragEnd)
        {
            if (!HasIngredient && _actionProgressBar.Value != 0.0)
            {
                _actionProgressBar.Value = 0.0;
                _actionProgressBar.AddThemeStyleboxOverride("fill", DefaultStyleBox);
            }
            else if (HasIngredient)
            {
                // Re-enabling mouse input after we disabled it to stop the animation
                CurrentIngredientButton.MouseFilter = MouseFilterEnum.Stop;
            }
        }
    }

    public override void _Process(double delta)
    {
        if (_currentState == State.Processing)
        {
            var timeUsed = _actionTimer.WaitTime - _actionTimer.TimeLeft;
            var percentUsed = timeUsed / _actionTimer.WaitTime;
            _actionProgressBar.Value = percentUsed * 100.0;
        }
    }

    public override void _GuiInput(InputEvent @event)
    {
        var ingredientButton = ParentChefsDePartieDropSlot.DestinationSlot.GetChildOrNull<IngredientButton>(0);
        // In theory, if we even get input `ingredientButton` should be non-null, but shouldn't hurt to be defensive
        if (!HasIngredient && ingredientButton != null)
        {
            if (@event.IsActionPressed("ui_accept"))
            {
                GD.Print($"Sending to chef action drop slot {Name}!");
                MoveToSlot(ingredientButton, false);
            }
            else if (@event.IsActionPressed("ui_cancel"))
            {
                GD.Print($"Cancelling sending to Chef Action from drop slot...");
                ChefsDePartieDropSlot.CancelSlot.MoveToSlot(ingredientButton, true);
            }
        }
    }

    public override bool _CanDropData(Vector2 atPosition, Variant data)
    {
        return _currentState == State.Waiting && CanAcceptMainIngredient(data.As<IngredientButton>().Ingredient);
    }

    public override void _DropData(Vector2 atPosition, Variant data)
    {
        // Don't want to keep focus when accepting as action since the player will need to wait for the action to
        // finish
        MoveToSlot(data.As<IngredientButton>(), false);
    }

    public bool CanAcceptMainIngredient(Ingredient candidateIngredient)
    {
        if (HasIngredient)
        {
            return false;
        }

        var foundIngredients = new List<Ingredient>
        {
            candidateIngredient
        };
        if (ChefAction.RequiredIngredients.Count > 0)
        {
            foundIngredients.AddRange(GetRequiredIngredientsFromPrepArea());
        }
        return ChefAction.CanAcceptIngredients(foundIngredients.ToArray());
    }

    // This version never grabs focus so technically we ignore `_grabFocus`
    public override void MoveToSlot(IngredientButton ingredientButton, bool _grabFocus = false)
    {
        var requiredIngredientButtons = GetRequiredIngredientButtonsFromPrepArea().ToList();
        var requiredIngredients = requiredIngredientButtons.Select(ib => ingredientButton.Ingredient).ToList();
        _ingredientsToProcess.Clear();
        _ingredientsToProcess.Add(ingredientButton.Ingredient);
        _ingredientsToProcess.AddRange(requiredIngredients);
        foreach (var requiredIngredientButton in requiredIngredientButtons)
        {
            GD.Print($"About to free Required Ingredient {requiredIngredientButton.Name}!");
            requiredIngredientButton.Visible = false;
            requiredIngredientButton.ForceReparent(this, false);
            requiredIngredientButton.FocusMode = FocusModeEnum.None; // To make sure it doesn't gain focus
            requiredIngredientButton.QueueFree();
        }

        var oldSlot = ingredientButton.CurrentSlot;
        if (oldSlot is ChefsDePartieDropSlot)
        {
            oldSlot = ChefsDePartieDropSlot.CancelSlot;
        }
        base.MoveToSlot(ingredientButton, false);
        oldSlot.RetakeFocus();
        ingredientButton.FocusMode = FocusModeEnum.None;

        _currentState = State.Processing;
        _actionProgressBar.Value = 0.0;
        _actionTimer.Start();
    }

    public void ResetAfterIngredientTaken()
    {
        _actionIngredientMarginContainer.AddThemeConstantOverride("margin_right", 8);
        _actionIngredientMarginContainer.SizeFlagsHorizontal = SizeFlags.ShrinkEnd;
        _actionVerbLabel.Visible = true;
        _actionProgressBar.Value = 0.0;
        _actionProgressBar.AddThemeStyleboxOverride("fill", DefaultStyleBox);

        _currentState = State.Waiting;
    }

    public void PrepareForAiming(Ingredient ingredient)
    {
        if (CanAcceptMainIngredient(ingredient))
        {
            FocusMode = FocusModeEnum.All;
            _actionProgressBar.Value = 100.0;
        }
        else if (HasIngredient)
        {
            // To prevent taking back ingredients while aiming
            CurrentIngredientButton.FocusMode = FocusModeEnum.None;
        }
    }

    public void ResetAfterAiming()
    {
        FocusMode = FocusModeEnum.None; // May not need to, but shouldn't hurt
        if (HasIngredient)
        {
            CurrentIngredientButton.FocusMode = FocusModeEnum.All;
        }
        else
        {
            _actionProgressBar.Value = 0.0;
        }
    }

    public void OnDragStart(Ingredient ingredient)
    {
        if (CanAcceptMainIngredient(ingredient))
        {
            _actionProgressBar.Value = 100.0;
        }
        else if (HasIngredient)
        {
            // To stop the animation when the mouse enters the IngredientButton
            CurrentIngredientButton.MouseFilter = MouseFilterEnum.Ignore;
        }
    }

    private void OnFocusEntered()
    {
        if (!HasIngredient)
        {
            _actionProgressBar.AddThemeStyleboxOverride("fill", SelectedStyleBox);
        }
    }

    private void OnFocusExited()
    {
        if (!HasIngredient)
        {
            _actionProgressBar.AddThemeStyleboxOverride("fill", DefaultStyleBox);
        }
    }

    private void OnMouseEntered()
    {
        if (!HasIngredient && _actionProgressBar.Value == 100.0)
        {
            _actionProgressBar.AddThemeStyleboxOverride("fill", SelectedStyleBox);
        }
    }

    private void OnMouseExited()
    {
        if (!HasIngredient && _actionProgressBar.Value == 100.0)
        {
            _actionProgressBar.AddThemeStyleboxOverride("fill", DefaultStyleBox);
        }
    }

    private IEnumerable<Ingredient> GetRequiredIngredientsFromPrepArea()
    {
        return GetRequiredIngredientButtonsFromPrepArea().Select(ib => ib.Ingredient);
    }

    private IEnumerable<IngredientButton> GetRequiredIngredientButtonsFromPrepArea()
    {
        var neededRequiredIngredients = new List<Ingredient>(ChefAction.RequiredIngredients);
        var prepAreaIngredientButtons = DownSlot.CurrentIngredientButtons.ToList();
        var foundIngredientButtons = new List<IngredientButton>();
        foreach (var prepAreaIngredientButton in prepAreaIngredientButtons)
        {
            var matchingRequiredIngredient = neededRequiredIngredients
                .FirstOrDefault(i => i.QualityInsensitiveEquals(prepAreaIngredientButton.Ingredient));
            if (matchingRequiredIngredient != default)
            {
                foundIngredientButtons.Add(prepAreaIngredientButton);
                neededRequiredIngredients.Remove(matchingRequiredIngredient);
            }

            if (neededRequiredIngredients.Count == 0)
            {
                break;
            }
        }

        return foundIngredientButtons;
    }

    private void OnIngredientReady()
    {
        _actionTimer.Stop();
        _actionProgressBar.Value = 100.0; // Just in case this happens before _Ready runs
        _actionProgressBar.AddThemeStyleboxOverride("fill", ReadyStyleBox);
        _actionIngredientMarginContainer.AddThemeConstantOverride("margin_right", 0);
        _actionIngredientMarginContainer.SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
        _actionVerbLabel.Visible = false;

        var resultIngredient = ChefAction.ProcessIngredients(_ingredientsToProcess);
        var ingredientButton = _actionIngredientPanelContainer.GetChild<IngredientButton>(0); // Should be only one
        ingredientButton.Ingredient = resultIngredient;
        ingredientButton.ButtonSize = 64;
        ingredientButton.FocusMode = FocusModeEnum.All;

        _currentState = State.Ready;
    }

    private enum State
    {
        Waiting,
        Processing,
        Ready
    }
}