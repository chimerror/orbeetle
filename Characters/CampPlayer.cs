using Godot;
using System;

public partial class CampPlayer : CharacterBody2D
{
    private AnimationTree _campPlayerAnimationTree;
    private Vector2 _mouseTarget;
    private bool _hasMouseTarget;

    public const float Speed = 300.0f;

    public override void _Ready()
    {
        _campPlayerAnimationTree = GetNode<AnimationTree>("%CampPlayerAnimationTree");
        _campPlayerAnimationTree.Active = true;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButtonEvent &&
            mouseButtonEvent.Pressed &&
            mouseButtonEvent.ButtonIndex == MouseButton.Left)
        {
            _mouseTarget = GetGlobalMousePosition();
            _hasMouseTarget = true;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector2 direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
        if (direction != Vector2.Zero)
        {
            _hasMouseTarget = false;
        }
        else if (_hasMouseTarget)
        {
            direction = Position.DistanceTo(_mouseTarget) > 10 ? Position.DirectionTo(_mouseTarget) : Vector2.Zero;
        }
        if (direction != Vector2.Zero)
        {
            Velocity = direction * Speed;
            _campPlayerAnimationTree.Set("parameters/conditions/isIdle", false);
            _campPlayerAnimationTree.Set("parameters/conditions/isMoving", true);
            _campPlayerAnimationTree.Set("parameters/Idle/blend_position", direction);
            _campPlayerAnimationTree.Set("parameters/Walking/blend_position", direction);
        }
        else
        {
            Velocity = Vector2.Zero;
            _hasMouseTarget = false;
            _campPlayerAnimationTree.Set("parameters/conditions/isIdle", true);
            _campPlayerAnimationTree.Set("parameters/conditions/isMoving", false);
        }

        MoveAndSlide();
    }
}
