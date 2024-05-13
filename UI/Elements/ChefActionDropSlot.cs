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

    [Export]
    public ChefAction ChefAction { get; set; }

    public override void _Ready()
    {
        _actionVerbLabel = GetNode<Label>("%ActionVerbLabel");
        _actionVerbLabel.Text = ChefAction.ActionVerb;

        _actionTimer = GetNode<Timer>("%ActionTimer");
        _actionTimer.WaitTime = ChefAction.Duration;
        _actionTimer.Timeout += OnIngredientReady;

        _actionProgressBar = GetNode<ProgressBar>("%ActionProgressBar");

        _actionIngredientMarginContainer = GetNode<MarginContainer>("%ActionIngredientMarginContainer");
        _actionIngredientPanelContainer = GetNode<PanelContainer>("%ActionIngredientPanelContainer");
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

    public override bool _CanDropData(Vector2 atPosition, Variant data)
    {
        // TODOTODO: Should we reject dragging between Chef Actions to keep it the same between mouse and non-mouse?
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
            requiredIngredientButton.QueueFree();
        }

        var oldSlot = ingredientButton.CurrentSlot;
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

        _currentState = State.Waiting;
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