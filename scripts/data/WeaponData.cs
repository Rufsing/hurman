using Godot;

namespace Hurman.Data;

public enum WeaponSlot
{
    Sidearm,
    Primary,
}

/// <summary>Stats for one firearm. Each gun is a .tres file in res://data/weapons/.</summary>
[GlobalClass]
public partial class WeaponData : Resource
{
    [Export] public string Id { get; set; } = "";
    [Export] public string DisplayName { get; set; } = "";
    [Export] public WeaponSlot Slot { get; set; } = WeaponSlot.Sidearm;
    [Export] public float Damage { get; set; } = 25f;
    [Export] public float HeadshotMultiplier { get; set; } = 3f;
    [Export] public float FireRate { get; set; } = 4f; // shots per second
    [Export] public bool Automatic { get; set; }
    [Export] public int MagazineSize { get; set; } = 12;
    [Export] public int MaxReserveAmmo { get; set; } = 48;
    [Export] public float ReloadTime { get; set; } = 1.4f;
    [Export] public float Range { get; set; } = 60f;
    [Export] public float RecoilPitch { get; set; } = 1.5f; // degrees per shot
    [Export] public float RecoilYaw { get; set; } = 0.5f;
    [Export] public float Spread { get; set; } = 0.5f; // degrees, hip fire
    [Export] public float AimSpread { get; set; } = 0.1f;
    [Export] public float Noise { get; set; } = 30f; // radius in meters where NPCs hear it
    [Export] public int Price { get; set; } = 500;
}
