using Godot;
using Hurman.Core;

namespace Hurman.Interaction;

/// <summary>
/// Anything Fred can press "interact" on. The player's sensor finds the nearest one,
/// shows GetPrompt() on the HUD, and calls Interact(). Subclass and override OnInteract.
/// </summary>
[GlobalClass]
public partial class Interactable : Area3D
{
    [Signal] public delegate void InteractedEventHandler(Node3D by);

    [Export] public string Prompt { get; set; } = "Interact";
    [Export] public bool Enabled { get; set; } = true;

    public override void _Ready()
    {
        CollisionLayer = Layers.Interactable;
        CollisionMask = 0;
        Monitoring = false;
        Monitorable = true;
    }

    public virtual string GetPrompt() => Prompt;

    public virtual bool CanInteract(Node3D by) => Enabled;

    public void Interact(Node3D by)
    {
        if (!CanInteract(by)) return;
        OnInteract(by);
        EmitSignal(SignalName.Interacted, by);
    }

    protected virtual void OnInteract(Node3D by) { }

    /// <summary>Creates an interactable with a sphere trigger and adds it under parent.</summary>
    public static T Create<T>(Node parent, Vector3 position, float radius) where T : Interactable, new()
    {
        var interactable = new T { Position = position };
        interactable.AddChild(new CollisionShape3D { Shape = new SphereShape3D { Radius = radius } });
        parent.AddChild(interactable);
        return interactable;
    }
}
