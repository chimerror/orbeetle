using Godot;

public partial class ChefArea : PanelContainer
{
    private ChefAction _leftChefAction;
    private ChefActionDropSlot _leftChefActionDropSlot;
    private ChefAction _rightChefAction;
    private ChefActionDropSlot _rightChefActionDropSlot;

    [Export]
    public ChefAction LeftChefAction
    {
        get => _leftChefActionDropSlot?.ChefAction ?? _leftChefAction;
        set
        {
            if (_leftChefActionDropSlot != null)
            {
                _leftChefActionDropSlot.ChefAction = value;
            }
            _leftChefAction = value;
        }
    }

    [Export]
    public ChefAction RightChefAction
    {
        get => _rightChefActionDropSlot?.ChefAction ?? _rightChefAction;
        set
        {
            if (_rightChefActionDropSlot != null)
            {
                _rightChefActionDropSlot.ChefAction = value;
            }
            _rightChefAction = value;
        }
    }

    public override void _Ready()
    {
        _leftChefActionDropSlot = GetNode<ChefActionDropSlot>("%LeftChefActionDropSlot");
        _leftChefActionDropSlot.ChefAction = _leftChefAction;
        _rightChefActionDropSlot = GetNode<ChefActionDropSlot>("%RightChefActionDropSlot");
        _rightChefActionDropSlot.ChefAction = _rightChefAction;
    }
}