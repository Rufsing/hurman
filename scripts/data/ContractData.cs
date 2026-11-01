using Godot;

namespace Hurman.Data;

public enum ContractType
{
    Assassination,
    Destruction,
    Shakedown,
}

/// <summary>
/// One Ledger contract. Create new contracts as .tres files in res://data/contracts/;
/// no code changes are needed.
/// </summary>
[GlobalClass]
public partial class ContractData : Resource
{
    [Export] public string Id { get; set; } = "";
    [Export] public string Title { get; set; } = "";
    [Export] public ContractType Type { get; set; } = ContractType.Assassination;
    [Export] public string FixerName { get; set; } = "";
    [Export(PropertyHint.MultilineText)] public string Briefing { get; set; } = "";
    [Export] public string TargetName { get; set; } = "";
    [Export] public string LocationHint { get; set; } = "";
    [Export] public int RequiredCred { get; set; }
    [Export] public int PayoutMoney { get; set; }
    [Export] public int PayoutSovereigns { get; set; }
    [Export] public int PayoutCred { get; set; }
}
