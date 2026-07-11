using Godot;

namespace Goldenglow.Utils;

/// <summary>
/// Extension methods for Godot node tree traversal.
/// </summary>
public static class GodotExtension
{
    /// <summary>
    /// Walks up the parent chain until a node of type <typeparamref name="T"/> is found.
    /// Returns null if the root is reached without finding one.
    /// </summary>
    public static T? FindParent<T>(this Node node) where T : Node
    {
        var current = node.GetParent();
        while (current != null)
        {
            if (current is T result)
                return result;
            current = current.GetParent();
        }
        return null;
    }

    public static void PlayAllParticles(this Node2D node)
    {
        foreach (var particle in node.GetChildren().OfType<CpuParticles2D>())
        {
            particle.Emitting = true;
        }
        
        foreach (var particle in node.GetChildren().OfType<GpuParticles2D>())
        {
            particle.Emitting = true;
        }
    }
}
