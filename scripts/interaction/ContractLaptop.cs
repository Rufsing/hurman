using System.Linq;
using Godot;
using Hurman.Core;

namespace Hurman.Interaction;

/// <summary>Fred's beat-up laptop: the Ledger contract board.</summary>
public partial class ContractLaptop : Interactable
{
    public override string GetPrompt() => ContractManager.Instance.Active != null
        ? "Laptop: review active contract"
        : "Laptop: check the Ledger for work";

    protected override void OnInteract(Node3D by)
    {
        var cm = ContractManager.Instance;
        if (cm.Active != null)
        {
            EventBus.ShowToast($"{cm.Active.FixerName}: \"{cm.Active.Briefing}\"");
            return;
        }

        var next = cm.GetAvailable().FirstOrDefault();
        if (next == null)
        {
            EventBus.ShowToast("No new work on the Ledger. Build your cred.");
            return;
        }

        cm.Accept(next.Id);
        EventBus.ShowToast($"CONTRACT ACCEPTED from {next.FixerName}: \"{next.Briefing}\"");
    }
}
