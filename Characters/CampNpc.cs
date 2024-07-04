using Godot;
using System;

public partial class CampNpc : Area2D
{
    private Sprite2D _speechSprite;
    private bool _mouseInArea;

    // Will be set by CampScene._Ready
    public DialogueDisplay dialogueDisplay;

    public override void _Ready()
    {
        _speechSprite = GetNode<Sprite2D>("%SpeechSprite");
        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;
        MouseEntered += OnMouseEntered;
        MouseExited += OnMouseExited;
        SetProcessInput(false);
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_accept") ||
            (_mouseInArea &&
            @event is InputEventMouseButton mouseEvent &&
            mouseEvent.Pressed &&
            mouseEvent.ButtonIndex == MouseButton.Left))
        {
            dialogueDisplay.StartDialogue("kamari"); // TODOTODO: Have knot grabbed from a character resource?
        }
    }

    public void OnDialogueStarted(string _)
    {
        _speechSprite.Visible = false;
        SetProcessInput(false);
    }

    public void OnDialogueFinished()
    {
        if (GetOverlappingBodies().Count > 0)
        {
            _speechSprite.Visible = true;
            SetProcessInput(true);
        }
    }

    private void OnBodyEntered(Node2D body)
    {
        _speechSprite.Visible = true;
        SetProcessInput(true);
    }

    private void OnBodyExited(Node2D body)
    {
        _speechSprite.Visible = false;
        SetProcessInput(false);
    }

    private void OnMouseEntered()
    {
        GD.Print("DEBUG: Mouse entered area!");
        _mouseInArea = true;
    }

    private void OnMouseExited()
    {
        GD.Print("DEBUG: Mouse exited area!");
        _mouseInArea = false;
    }
}