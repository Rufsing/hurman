using Godot;
using Hurman.Core;

namespace Hurman.Interaction;

/// <summary>
/// Placeholder takedown on a contract target. It is only usable while that contract is active.
/// It will be replaced by real combat (guns and gun-fu) in the next M1 steps.
/// </summary>
public partial class ContractTarget : Interactable
{
    [Export] public string ContractId { get; set; } = "";
    [Export] public string TargetName { get; set; } = "";
    [Export] public float HeatOnKill { get; set; } = 1f;

    public override string GetPrompt() => $"Take down {TargetName}";

    public override bool CanInteract(Node3D by) =>
        Enabled && ContractManager.Instance.Active?.Id == ContractId;

    protected override void OnInteract(Node3D by)
    {
        Enabled = false;
        ContractManager.Instance.CompleteActive(HeatOnKill);
        // The target root (mesh + this trigger) is our parent.
        GetParent().QueueFree();
    }
}
