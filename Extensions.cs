using Godot;
using System.Collections.Generic;
using System.Linq;

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

    public static List<T> GetChildrenOfType<T>(this Node node)
    {
        return node.GetChildren().Where(n => n is T).Cast<T>().ToList();
    }
}