using Godot;
using Hurman.Core;

namespace Hurman.Interaction;

/// <summary>Sleep: advances the day, cools heat, saves the game.</summary>
public partial class Bed : Interactable
{
    public override string GetPrompt() => "Mattress: sleep (save, next day)";

    protected override void OnInteract(Node3D by)
    {
        var gs = GameState.Instance;
        gs.Sleep();
        EventBus.ShowToast($"Day {gs.Day}. Fred sleeps it off. Game saved.");
        EventBus.Instance.EmitSignal(EventBus.SignalName.ObjectiveChanged, ContractManager.Instance.CurrentObjective);
    }
}
