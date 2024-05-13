using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class ChefsDePartieDropSlot : IngredientDropSlot
{
    private HBoxContainer _chefsHBox;

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

    // TODOTODO: This might be the best place for handling the non-mouse aiming. Will have to figure out how to handle
    // focusing

    public override void _Ready()
    {
        _chefsHBox = GetNode<HBoxContainer>("%ChefsHBox");
        foreach (var chefActionDropSlot in ChefActionDropSlots)
        {
            chefActionDropSlot.DownSlot = DownSlot;
        }
    }

    public override bool _CanDropData(Vector2 atPosition, Variant data)
    {
        // Despite the name, this one does not actually accept drag and drop. Kinda painted myself into a corner here.
        return false;
    }
}