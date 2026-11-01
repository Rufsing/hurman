using System.Globalization;
using Godot;
using Hurman.Core;

namespace Hurman.Interaction;

/// <summary>A spot in the base that can be renovated into a facility (armory, gun range...).</summary>
public partial class UpgradeStation : Interactable
{
    [Export] public string UpgradeId { get; set; } = "";
    [Export] public string DisplayName { get; set; } = "";
    [Export] public int Cost { get; set; } = 1000;
    [Export] public string BuiltDescription { get; set; } = "";

    private int Level => GameState.Instance.GetUpgradeLevel(UpgradeId);

    public override string GetPrompt() => Level > 0
        ? $"{DisplayName} (Lv {Level}): {BuiltDescription}"
        : $"{DisplayName} (run-down): renovate for ${Cost.ToString("N0", CultureInfo.InvariantCulture)}";

    protected override void OnInteract(Node3D by)
    {
        if (Level > 0)
        {
            EventBus.ShowToast($"{DisplayName}: {BuiltDescription}");
            return;
        }

        if (!GameState.Instance.TrySpendMoney(Cost))
        {
            EventBus.ShowToast($"Not enough money. The {DisplayName} renovation costs ${Cost.ToString("N0", CultureInfo.InvariantCulture)}.");
            return;
        }

        GameState.Instance.SetUpgradeLevel(UpgradeId, 1);
        EventBus.ShowToast($"{DisplayName} renovated! Sleep to save your progress.");
    }
}
