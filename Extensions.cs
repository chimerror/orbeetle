using Godot;
using System;

public static class Extensions
{
    public static void ForceReparent(this Node node, Node newParent, bool keepGlobalTransform = true)
    {
        var currentParent = node.GetParent();
        if (currentParent == null)
        {
            newParent.AddChild(node);
        }
        else
        {
            node.Reparent(newParent, keepGlobalTransform);
        }
    }
}