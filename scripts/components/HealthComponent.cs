using Godot;

namespace Hurman.Components;

/// <summary>Reusable health for the player, NPCs and destructible objects.</summary>
[GlobalClass]
public partial class HealthComponent : Node
{
    [Signal] public delegate void HealthChangedEventHandler(float current, float max);
    [Signal] public delegate void DiedEventHandler(Node3D killer);

    [Export] public float MaxHealth { get; set; } = 100f;
    public float Current { get; private set; }
    public bool IsDead => Current <= 0f;

    public override void _Ready() => Current = MaxHealth;

    public void Damage(float amount, Node3D source = null)
    {
        if (IsDead || amount <= 0f) return;
        Current = Mathf.Max(0f, Current - amount);
        EmitSignal(SignalName.HealthChanged, Current, MaxHealth);
        if (IsDead) EmitSignal(SignalName.Died, source);
    }

    public void Heal(float amount)
    {
        if (IsDead) return;
        Current = Mathf.Min(MaxHealth, Current + amount);
        EmitSignal(SignalName.HealthChanged, Current, MaxHealth);
    }
}
