using Godot;

namespace Hurman.Data;

/// <summary>A base upgrade (armory, gun range, garage...). Level costs index by level: Costs[0] buys level 1.</summary>
[GlobalClass]
public partial class UpgradeData : Resource
{
    [Export] public string Id { get; set; } = "";
    [Export] public string DisplayName { get; set; } = "";
    [Export(PropertyHint.MultilineText)] public string Description { get; set; } = "";
    [Export] public int[] Costs { get; set; } = { 1500 };
    [Export] public int RequiredCred { get; set; }

    public int MaxLevel => Costs.Length;
}
