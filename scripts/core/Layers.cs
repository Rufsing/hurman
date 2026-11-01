namespace Hurman.Core;

/// <summary>Physics layer bitmasks. Keep in sync with [layer_names] in project.godot.</summary>
public static class Layers
{
    public const uint World = 1 << 0;
    public const uint Player = 1 << 1;
    public const uint Npc = 1 << 2;
    public const uint Interactable = 1 << 3;
}
