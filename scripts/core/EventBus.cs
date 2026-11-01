using Godot;

namespace Hurman.Core;

/// <summary>
/// Global signal hub (autoload). Systems talk through these signals instead of
/// referencing each other directly. Loaded first, so it also registers the input map.
/// </summary>
public partial class EventBus : Node
{
    public static EventBus Instance { get; private set; }

    // Economy
    [Signal] public delegate void MoneyChangedEventHandler(int value);
    [Signal] public delegate void SovereignsChangedEventHandler(int value);
    [Signal] public delegate void CredChangedEventHandler(int value);
    [Signal] public delegate void HeatChangedEventHandler(float value);
    [Signal] public delegate void DayChangedEventHandler(int day);
    [Signal] public delegate void UpgradeChangedEventHandler(string upgradeId, int level);

    // Contracts
    [Signal] public delegate void ContractAcceptedEventHandler(string contractId);
    [Signal] public delegate void ContractCompletedEventHandler(string contractId);
    [Signal] public delegate void ObjectiveChangedEventHandler(string text);

    // UI / player
    [Signal] public delegate void InteractPromptChangedEventHandler(string text);
    [Signal] public delegate void ToastEventHandler(string text);
    [Signal] public delegate void AimChangedEventHandler(bool aiming);

    public override void _EnterTree()
    {
        Instance = this;
        InputSetup.Register();
    }

    /// <summary>Shows a short message on the HUD.</summary>
    public static void ShowToast(string text) => Instance?.EmitSignal(SignalName.Toast, text);
}
