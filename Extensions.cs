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

    public static bool ConnectSignalIfNeeded(
        this GodotObject godotObject,
        StringName signal,
        Callable callable,
        uint flags = 0)
    {
        if (!godotObject.IsConnected(signal, callable))
        {
            godotObject.Connect(signal, callable, flags);
            return true;
        }
        return false;
    }

    public static bool DisconnectSignalIfNeeded(this GodotObject godotObject, StringName signal, Callable callable)
    {
        if (godotObject.IsConnected(signal, callable))
        {
            godotObject.Disconnect(signal, callable);
            return true;
        }
        return false;
    }
}