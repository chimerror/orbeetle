using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class ChefsDePartieDropSlot : IngredientDropSlot
{
    private HBoxContainer _chefsHBox;
    private VBoxContainer _aimedIngredientVBox;

    public static ChefsDePartieDropSlot Instance { get; private set; }

    public static IngredientDropSlot CancelSlot { get; private set; }

    public IEnumerable<ChefActionDropSlot> ChefActionDropSlots
    {
        get => _chefsHBox
                .GetChildren()
                .SelectMany(ca => new List<Node>()
                    {
                        ca.GetNode("%LeftChefActionDropSlot"), ca.GetNode("%RightChefActionDropSlot")
                    })
                .Cast<ChefActionDropSlot>();
    }

    public override void _Ready()
    {
        Instance = this;
        _chefsHBox = GetNode<HBoxContainer>("%ChefsHBox");
        _aimedIngredientVBox = GetNode<VBoxContainer>("AimedIngredientVBox");
        foreach (var chefActionDropSlot in ChefActionDropSlots)
        {
            chefActionDropSlot.ParentChefsDePartieDropSlot = this;
            chefActionDropSlot.LeftSlot = LeftSlot;
            chefActionDropSlot.RightSlot = RightSlot;
            chefActionDropSlot.DownSlot = DownSlot;
        }
    }

    public override bool _CanDropData(Vector2 atPosition, Variant data)
    {
        // Despite the name, this one does not actually accept drag and drop. Kind of painted myself into a corner here.
        return false;
    }

    // This version never grabs focus so technically we ignore `_grabFocus`
    public override void MoveToSlot(IngredientButton ingredientButton, bool _grabFocus = false)
    {
        ChefActionDropSlot firstAcceptableDropSlot = null;
        foreach (var dropSlot in ChefActionDropSlots)
        {
            if (dropSlot.CanAcceptMainIngredient(ingredientButton.Ingredient))
            {
                firstAcceptableDropSlot ??= dropSlot;
            }
            dropSlot.PrepareForAiming(ingredientButton.Ingredient);
        }
        _aimedIngredientVBox.Visible = true;
        CancelSlot = ingredientButton.CurrentSlot;
        base.MoveToSlot(ingredientButton, false);
        ingredientButton.FocusMode = FocusModeEnum.None;
        firstAcceptableDropSlot.GrabFocus();
        IngredientSelection.Instance.LockNonChefSlots();
    }

    public bool CanAcceptMainIngredient(Ingredient ingredient)
    {
        return GetAcceptableChefActionDropSlots(ingredient).Any();
    }

    public void OnDragStart(Ingredient ingredient)
    {
        foreach (var dropSlot in ChefActionDropSlots)
        {
            dropSlot.OnDragStart(ingredient);
        }
    }

    public void ResetAfterIngredientTaken()
    {
        IngredientSelection.Instance.UnlockNonChefSlots();
        _aimedIngredientVBox.Visible = false;

        foreach (var dropSlot in ChefActionDropSlots)
        {
            dropSlot.ResetAfterAiming();
        }
    }

    private IEnumerable<ChefActionDropSlot> GetAcceptableChefActionDropSlots(Ingredient ingredient)
    {
        return ChefActionDropSlots.Where(cads => cads.CanAcceptMainIngredient(ingredient));
    }
}