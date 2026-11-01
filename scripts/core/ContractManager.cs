using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Godot;
using Hurman.Data;

namespace Hurman.Core;

/// <summary>
/// Loads every ContractData resource in res://data/contracts/ and runs the
/// accept → complete flow (autoload). The active contract id lives in GameState so it is saved.
/// </summary>
public partial class ContractManager : Node
{
    public static ContractManager Instance { get; private set; }

    private const string ContractDir = "res://data/contracts/";
    private readonly Dictionary<string, ContractData> _contracts = new();

    public ContractData Active =>
        _contracts.GetValueOrDefault(GameState.Instance.ActiveContractId ?? "");

    public string CurrentObjective => Active != null
        ? $"{Active.Title}\nEliminate {Active.TargetName}. {Active.LocationHint}"
        : "Check the laptop in the basement for work.";

    public override void _EnterTree() => Instance = this;

    public override void _Ready() => LoadContracts();

    private void LoadContracts()
    {
        foreach (var name in DirAccess.GetFilesAt(ContractDir))
        {
            // Exported builds rename resources to *.remap
            var file = name.EndsWith(".remap") ? name[..^".remap".Length] : name;
            if (!file.EndsWith(".tres")) continue;

            var contract = GD.Load<ContractData>(ContractDir + file);
            if (contract == null || string.IsNullOrEmpty(contract.Id))
            {
                GD.PushWarning($"Skipping invalid contract: {file}");
                continue;
            }
            _contracts[contract.Id] = contract;
        }
        GD.Print($"[Ledger] {_contracts.Count} contract(s) loaded.");
    }

    public ContractData Get(string id) => _contracts.GetValueOrDefault(id);

    public bool IsCompleted(string id) => GameState.Instance.CompletedContracts.Contains(id);

    /// <summary>Contracts Fred can take right now, lowest requirement first.</summary>
    public IEnumerable<ContractData> GetAvailable() =>
        _contracts.Values
            .Where(c => !IsCompleted(c.Id) && c.RequiredCred <= GameState.Instance.Cred && c != Active)
            .OrderBy(c => c.RequiredCred);

    public bool Accept(string id)
    {
        if (Active != null || !_contracts.ContainsKey(id)) return false;
        GameState.Instance.ActiveContractId = id;
        EventBus.Instance.EmitSignal(EventBus.SignalName.ContractAccepted, id);
        EventBus.Instance.EmitSignal(EventBus.SignalName.ObjectiveChanged, CurrentObjective);
        return true;
    }

    /// <summary>Pays out the active contract. Heat is added for messy work.</summary>
    public void CompleteActive(float heat)
    {
        var contract = Active;
        if (contract == null) return;

        var gs = GameState.Instance;
        gs.ActiveContractId = "";
        gs.CompletedContracts.Add(contract.Id);
        gs.AddMoney(contract.PayoutMoney);
        gs.AddSovereigns(contract.PayoutSovereigns);
        gs.AddCred(contract.PayoutCred);
        gs.AddHeat(heat);

        EventBus.Instance.EmitSignal(EventBus.SignalName.ContractCompleted, contract.Id);
        EventBus.Instance.EmitSignal(EventBus.SignalName.ObjectiveChanged,
            "Job done. Head home and sleep it off (cools heat, saves).");
        EventBus.ShowToast(
            $"CONTRACT COMPLETE: +${contract.PayoutMoney.ToString("N0", CultureInfo.InvariantCulture)}  " +
            $"+{contract.PayoutSovereigns} Sovereigns  +{contract.PayoutCred} cred");
    }
}
