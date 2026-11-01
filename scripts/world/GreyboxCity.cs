using Godot;
using Hurman.Core;
using Hurman.Interaction;

namespace Hurman.World;

/// <summary>
/// M1 greybox of Lowmarket: Fred's building (basement + ground floor), one street, and the
/// Neon Swan bar across the road. Everything is generated from boxes so the layout is easy to
/// tweak in code. It will be replaced by modular low-poly kit scenes (Kenney/KayKit) later.
///
/// Coordinates: street level is y = 0. Fred's building footprint is x[-8, 8], z[-28, -14],
/// with the front door facing +z toward the road (z[-6, 6]).
/// </summary>
public partial class GreyboxCity : Node3D
{
    private const float BasementY = -3.5f;
    private const float BuildingTop = 8f;
    private const float Wall = 0.4f;
    private const float MapHalf = 60f;

    private StandardMaterial3D _asphalt, _sidewalk, _concrete, _basementFloor, _facade, _wood,
        _metal, _lineWhite, _windowLit, _windowDark, _skin, _targetSuit, _carPaint, _glass;
    private StandardMaterial3D[] _facades;
    private readonly RandomNumberGenerator _rng = new() { Seed = 1337UL };

    public override void _Ready()
    {
        CreateMaterials();
        BuildLighting();
        BuildGround();
        BuildHomeBuilding();
        BuildBasement();
        BuildStreet();
        BuildCityRow();
        BuildBoundary();
        SpawnContractTargets();
    }

    // ------------------------------------------------------------------ materials & helpers

    private void CreateMaterials()
    {
        _asphalt = Mat(new Color(0.06f, 0.06f, 0.07f), 0.15f, 0.25f); // wet asphalt reflects neon
        _sidewalk = Mat(new Color(0.2f, 0.2f, 0.22f), 0.3f);
        _concrete = Mat(new Color(0.28f, 0.28f, 0.3f), 0.9f);
        _basementFloor = Mat(new Color(0.17f, 0.16f, 0.15f), 0.95f);
        _facade = Mat(new Color(0.3f, 0.25f, 0.23f), 0.9f);
        _wood = Mat(new Color(0.33f, 0.22f, 0.14f), 0.85f);
        _metal = Mat(new Color(0.3f, 0.32f, 0.35f), 0.4f, 0.7f);
        _lineWhite = Mat(new Color(0.75f, 0.75f, 0.7f), 0.5f);
        _windowLit = Neon(new Color(1f, 0.78f, 0.5f), 1.2f);
        _windowDark = Mat(new Color(0.04f, 0.05f, 0.08f), 0.1f, 0.6f);
        _skin = Mat(new Color(0.8f, 0.62f, 0.5f), 0.7f);
        _targetSuit = Mat(new Color(0.55f, 0.08f, 0.08f), 0.6f);
        _carPaint = Mat(new Color(0.25f, 0.03f, 0.05f), 0.25f, 0.6f);
        _glass = Mat(new Color(0.05f, 0.07f, 0.1f), 0.05f, 0.8f);
        _facades = new[]
        {
            Mat(new Color(0.3f, 0.25f, 0.23f), 0.9f),
            Mat(new Color(0.22f, 0.24f, 0.28f), 0.85f),
            Mat(new Color(0.35f, 0.3f, 0.25f), 0.9f),
            Mat(new Color(0.18f, 0.18f, 0.2f), 0.7f),
        };
    }

    private static StandardMaterial3D Mat(Color color, float roughness = 0.8f, float metallic = 0f) =>
        new() { AlbedoColor = color, Roughness = roughness, Metallic = metallic };

    private static StandardMaterial3D Neon(Color color, float energy = 4f) => new()
    {
        AlbedoColor = color,
        EmissionEnabled = true,
        Emission = color,
        EmissionEnergyMultiplier = energy,
    };

    /// <summary>Solid box with collision, positioned by center (optionally rotated).</summary>
    private StaticBody3D Box(Vector3 center, Vector3 size, Material mat, Vector3 rotation = default, Node parent = null)
    {
        var body = new StaticBody3D
        {
            Position = center,
            Rotation = rotation,
            CollisionLayer = Layers.World,
            CollisionMask = 0,
        };
        body.AddChild(new MeshInstance3D { Mesh = new BoxMesh { Size = size, Material = mat } });
        body.AddChild(new CollisionShape3D { Shape = new BoxShape3D { Size = size } });
        (parent ?? this).AddChild(body);
        return body;
    }

    /// <summary>Solid axis-aligned box from min/max corners.</summary>
    private StaticBody3D Block(Vector3 min, Vector3 max, Material mat, Node parent = null) =>
        Box((min + max) / 2f, max - min, mat, default, parent);

    /// <summary>Visual-only box (no collision, no shadow), for windows, signs and markings.</summary>
    private MeshInstance3D Visual(Vector3 center, Vector3 size, Material mat, Node parent = null)
    {
        var mesh = new MeshInstance3D
        {
            Mesh = new BoxMesh { Size = size, Material = mat },
            Position = center,
            CastShadow = GeometryInstance3D.ShadowCastingSetting.Off,
        };
        (parent ?? this).AddChild(mesh);
        return mesh;
    }

    private OmniLight3D Light(Vector3 position, Color color, float energy, float range, bool shadows = false)
    {
        var light = new OmniLight3D
        {
            Position = position,
            LightColor = color,
            LightEnergy = energy,
            OmniRange = range,
            ShadowEnabled = shadows,
        };
        AddChild(light);
        return light;
    }

    /// <summary>Invisible wall (collision only).</summary>
    private void InvisibleBlock(Vector3 min, Vector3 max)
    {
        var body = new StaticBody3D { Position = (min + max) / 2f, CollisionLayer = Layers.World, CollisionMask = 0 };
        body.AddChild(new CollisionShape3D { Shape = new BoxShape3D { Size = max - min } });
        AddChild(body);
    }

    // ------------------------------------------------------------------ world

    private void BuildLighting()
    {
        var env = new Godot.Environment
        {
            BackgroundMode = Godot.Environment.BGMode.Color,
            BackgroundColor = new Color(0.015f, 0.02f, 0.045f),
            AmbientLightSource = Godot.Environment.AmbientSource.Color,
            AmbientLightColor = new Color(0.3f, 0.35f, 0.55f),
            AmbientLightEnergy = 0.7f,
            TonemapMode = Godot.Environment.ToneMapper.Aces,
            GlowEnabled = true,
            GlowIntensity = 0.9f,
            GlowBloom = 0.08f,
            FogEnabled = true,
            FogLightColor = new Color(0.1f, 0.1f, 0.18f),
            FogDensity = 0.012f,
            VolumetricFogEnabled = true,
            VolumetricFogDensity = 0.012f,
            VolumetricFogAlbedo = new Color(0.7f, 0.7f, 0.85f),
            SsrEnabled = true,
            SsaoEnabled = true,
        };
        AddChild(new WorldEnvironment { Environment = env });

        AddChild(new DirectionalLight3D
        {
            LightColor = new Color(0.55f, 0.62f, 0.9f),
            LightEnergy = 0.2f,
            ShadowEnabled = true,
            RotationDegrees = new Vector3(-55f, 30f, 0f),
        });
    }

    private void BuildGround()
    {
        // Street-level slabs (top at y = 0), leaving a hole for Fred's basement.
        Block(new(-MapHalf, -0.4f, -14), new(MapHalf, 0, -6), _sidewalk);   // home-side sidewalk
        Block(new(-MapHalf, -0.4f, -6), new(MapHalf, 0, 6), _asphalt);      // road
        Block(new(-MapHalf, -0.4f, 6), new(MapHalf, 0, 10), _sidewalk);     // far sidewalk
        Block(new(-MapHalf, -0.4f, 10), new(MapHalf, 0, MapHalf), _concrete);
        Block(new(-MapHalf, -0.4f, -MapHalf), new(MapHalf, 0, -28), _concrete);
        Block(new(-MapHalf, -0.4f, -28), new(-8, 0, -14), _concrete);
        Block(new(8, -0.4f, -28), new(MapHalf, 0, -14), _concrete);

        // Dashed center line
        for (float x = -MapHalf + 2f; x < MapHalf; x += 6f)
            Visual(new Vector3(x, 0.01f, 0f), new Vector3(3f, 0.02f, 0.18f), _lineWhite);
    }

    private void BuildHomeBuilding()
    {
        const float b = BasementY;
        const float top = BuildingTop;

        // Shell: walls run from the basement floor to the roof.
        Block(new(-8, b - 0.4f, -28), new(8, b, -14), _basementFloor);           // basement floor
        Block(new(-8, b, -28), new(8, top, -28 + Wall), _facade);                // back
        Block(new(-8, b, -28 + Wall), new(-8 + Wall, top, -14 - Wall), _facade); // left
        Block(new(8 - Wall, b, -28 + Wall), new(8, top, -14 - Wall), _facade);   // right
        Block(new(-8, b, -14 - Wall), new(8, 0, -14), _facade);                  // front, below ground
        Block(new(-8, 0, -14 - Wall), new(3, top, -14), _facade);                // front, left of door
        Block(new(5, 0, -14 - Wall), new(8, top, -14), _facade);                 // front, right of door
        Block(new(3, 2.6f, -14 - Wall), new(5, top, -14), _facade);              // above door
        Block(new(-8, top, -28), new(8, top + 0.3f, -14), _concrete);            // roof

        // Ground-floor slab (top y = 0), with a stairwell opening at x[-6, 2], z[-17, -14.4].
        Block(new(-7.6f, -0.3f, -27.6f), new(7.6f, 0, -17), _concrete);
        Block(new(-7.6f, -0.3f, -17), new(-6, 0, -14.4f), _concrete);
        Block(new(2, -0.3f, -17), new(7.6f, 0, -14.4f), _concrete);
        Block(new(-7.6f, 3.7f, -27.6f), new(7.6f, 4f, -14.4f), _concrete);       // 1st floor ceiling

        // Stair ramp: from (x=-6, y=basement) up to (x=2, y=0).
        float dx = 8f, dy = -BasementY;
        float angle = Mathf.Atan2(dy, dx);
        float length = Mathf.Sqrt(dx * dx + dy * dy);
        const float thick = 0.3f;
        var center = new Vector3(
            -2f + thick / 2f * Mathf.Sin(angle),
            BasementY / 2f - thick / 2f * Mathf.Cos(angle),
            -15.7f);
        Box(center, new Vector3(length, thick, 2.6f), _wood, new Vector3(0, 0, angle));

        // Railings around the stairwell on the ground floor.
        Block(new(-6, 0, -17.15f), new(2, 1f, -17), _metal);
        Block(new(-6.15f, 0, -17), new(-6, 1f, -14.4f), _metal);

        // Front: door frame glow, house number, a few lit windows.
        Visual(new Vector3(4, 2.75f, -13.95f), new Vector3(2.2f, 0.12f, 0.05f), Neon(new Color(1f, 0.6f, 0.25f), 3f));
        Light(new Vector3(4, 2.4f, -13.2f), new Color(1f, 0.65f, 0.35f), 1.2f, 5f);
        FacadeWindows(-8f, 8f, 4f, top, -13.95f, skipFrom: 2.5f, skipTo: 5.5f);

        // Ground floor (still Mrs. Dobrev's, empty for now): dim light.
        Light(new Vector3(4, 3.2f, -21), new Color(0.9f, 0.85f, 0.7f), 0.6f, 8f);
    }

    private void BuildBasement()
    {
        const float b = BasementY;

        // Mattress on pallets = Fred's bed.
        Block(new(-7.5f, b, -27.5f), new(-5.4f, b + 0.35f, -25.4f), _wood);
        Visual(new Vector3(-6.45f, b + 0.45f, -26.45f), new Vector3(1.9f, 0.2f, 1.9f), Mat(new Color(0.45f, 0.42f, 0.38f)));
        Interactable.Create<Bed>(this, new Vector3(-6.4f, b + 0.8f, -26.4f), 1.5f);

        // Desk + laptop = the Ledger contract board.
        Block(new(4, b, -27.5f), new(6.6f, b + 0.8f, -26.4f), _wood);
        Visual(new Vector3(5.2f, b + 0.83f, -26.9f), new Vector3(0.55f, 0.04f, 0.38f), _metal);
        Visual(new Vector3(5.2f, b + 1.02f, -27.1f), new Vector3(0.55f, 0.35f, 0.03f), Neon(new Color(0.3f, 0.9f, 1f), 2.5f));
        Light(new Vector3(5.2f, b + 1.3f, -26.6f), new Color(0.3f, 0.9f, 1f), 0.8f, 3.5f);
        Interactable.Create<ContractLaptop>(this, new Vector3(5.2f, b + 1f, -26.6f), 1.5f);

        // Rusted locker corner = future Armory.
        Block(new(7.0f, b, -22.2f), new(7.6f, b + 2.1f, -19.8f), _metal);
        var armory = Interactable.Create<UpgradeStation>(this, new Vector3(6.6f, b + 1f, -21f), 1.5f);
        armory.UpgradeId = "armory";
        armory.DisplayName = "Armory";
        armory.Cost = 1500;
        armory.BuiltDescription = "Weapon rack installed. Loadouts unlock with the weapon update.";

        // Junk, a pillar, and a single bare bulb: it is run-down.
        Block(new(-7.5f, b, -22f), new(-6.2f, b + 1.2f, -20.8f), _wood);
        Block(new(-7.5f, b, -20.6f), new(-6.7f, b + 0.8f, -19.8f), _wood);
        Block(new(-0.25f, b, -21.25f), new(0.25f, -0.3f, -20.75f), _concrete);
        Block(new(1.5f, b, -27.5f), new(2.5f, b + 1.6f, -27f), _metal); // boiler
        Light(new Vector3(-1.5f, -0.6f, -23f), new Color(1f, 0.75f, 0.45f), 3f, 12f, shadows: true);
        Light(new Vector3(4f, -0.8f, -18f), new Color(0.6f, 0.7f, 1f), 0.8f, 9f); // cold spill from the stairwell
    }

    private void BuildStreet()
    {
        var sodium = new Color(1f, 0.68f, 0.38f);
        for (float x = -54f; x <= 54f; x += 12f)
        {
            foreach (float z in new[] { -7.2f, 7.2f })
            {
                Block(new(x - 0.1f, 0, z - 0.1f), new(x + 0.1f, 5f, z + 0.1f), _metal);
                Visual(new Vector3(x, 5f, z), new Vector3(0.5f, 0.15f, 0.3f), Neon(sodium, 3f));
                Light(new Vector3(x, 4.8f, z), sodium, 2.2f, 11f);
            }
        }

        // Parked cars (static greybox until the vehicle step).
        ParkedCar(new Vector3(14f, 0f, -3.6f));
        ParkedCar(new Vector3(-22f, 0f, 3.6f));

        // Dumpster beside the bar = cover.
        Block(new(6.5f, 0, 8.2f), new(8.3f, 1.3f, 9.6f), Mat(new Color(0.1f, 0.25f, 0.15f), 0.6f, 0.3f));
    }

    private void ParkedCar(Vector3 at)
    {
        Block(at + new Vector3(-2.2f, 0.3f, -0.95f), at + new Vector3(2.2f, 1.1f, 0.95f), _carPaint);
        Block(at + new Vector3(-1.1f, 1.1f, -0.85f), at + new Vector3(1.0f, 1.65f, 0.85f), _glass);
        Visual(at + new Vector3(2.21f, 0.8f, 0.6f), new Vector3(0.05f, 0.15f, 0.3f), Neon(Colors.White, 2f));
        Visual(at + new Vector3(2.21f, 0.8f, -0.6f), new Vector3(0.05f, 0.15f, 0.3f), Neon(Colors.White, 2f));
        Visual(at + new Vector3(-2.21f, 0.8f, 0.6f), new Vector3(0.05f, 0.15f, 0.3f), Neon(Colors.Red, 2f));
        Visual(at + new Vector3(-2.21f, 0.8f, -0.6f), new Vector3(0.05f, 0.15f, 0.3f), Neon(Colors.Red, 2f));
    }

    private void BuildCityRow()
    {
        Color[] neonColors =
        {
            new(1f, 0.15f, 0.7f),  // magenta
            new(0.1f, 0.9f, 1f),   // cyan
            new(1f, 0.55f, 0.1f),  // amber
            new(0.5f, 0.3f, 1f),   // violet
        };

        // Across the street: z[10, 26], fronts face -z.
        for (int i = 0; i < 7; i++)
        {
            float x0 = -56f + i * 16f;
            float x1 = x0 + 13f;
            bool isBar = i == 3; // x[-8, 5]: the Neon Swan
            float height = isBar ? 7f : _rng.RandfRange(9f, 24f);
            Block(new(x0, 0, 10), new(x1, height, 26), _facades[_rng.RandiRange(0, _facades.Length - 1)]);
            FacadeWindows(x0, x1, isBar ? 4f : 3.5f, height, 9.95f);

            if (isBar)
            {
                var magenta = neonColors[0];
                Visual(new Vector3(-1.5f, 3.4f, 9.9f), new Vector3(5f, 0.9f, 0.12f), Neon(magenta, 5f));
                Visual(new Vector3(-1.5f, 2.6f, 9.92f), new Vector3(5.6f, 0.06f, 0.08f), Neon(neonColors[1], 4f));
                Light(new Vector3(-1.5f, 3f, 8.6f), magenta, 3f, 9f);
                // Bar entrance
                Visual(new Vector3(-1.5f, 1.2f, 9.95f), new Vector3(1.6f, 2.4f, 0.05f), Neon(new Color(1f, 0.5f, 0.3f), 0.8f));
                continue;
            }

            if (_rng.Randf() < 0.75f)
            {
                var color = neonColors[_rng.RandiRange(0, neonColors.Length - 1)];
                float y = _rng.RandfRange(3.2f, 6f);
                float cx = (x0 + x1) / 2f + _rng.RandfRange(-3f, 3f);
                Visual(new Vector3(cx, y, 9.9f), new Vector3(_rng.RandfRange(2f, 5f), 0.5f, 0.1f), Neon(color, 5f));
                Light(new Vector3(cx, y, 8.8f), color, 2.5f, 8f);
            }
        }

        // Fred's side: neighbours left and right of his building, fronts face +z.
        foreach (var (x0, x1) in new[] { (-44f, -28f), (-26f, -10f), (10f, 26f), (28f, 44f) })
        {
            float height = _rng.RandfRange(7f, 16f);
            Block(new(x0, 0, -28), new(x1, height, -14), _facades[_rng.RandiRange(0, _facades.Length - 1)]);
            FacadeWindows(x0, x1, 3.5f, height, -13.95f);
        }
    }

    /// <summary>Grid of lit/dark windows on a facade at depth z.</summary>
    private void FacadeWindows(float x0, float x1, float firstFloorY, float height, float z,
        float skipFrom = float.MaxValue, float skipTo = float.MinValue)
    {
        for (float y = firstFloorY; y < height - 1.5f; y += 3f)
        {
            for (float x = x0 + 1.3f; x < x1 - 1.3f; x += 2.4f)
            {
                if (y < 3f && x > skipFrom && x < skipTo) continue;
                var mat = _rng.Randf() < 0.35f ? _windowLit : _windowDark;
                Visual(new Vector3(x + 0.6f, y + 0.8f, z), new Vector3(1.2f, 1.5f, 0.06f), mat);
            }
        }
    }

    private void BuildBoundary()
    {
        const float h = 12f;
        InvisibleBlock(new(-MapHalf - 1, 0, -MapHalf), new(-MapHalf, h, MapHalf));
        InvisibleBlock(new(MapHalf, 0, -MapHalf), new(MapHalf + 1, h, MapHalf));
        InvisibleBlock(new(-MapHalf, 0, -MapHalf - 1), new(MapHalf, h, -MapHalf));
        InvisibleBlock(new(-MapHalf, 0, MapHalf), new(MapHalf, h, MapHalf + 1));
    }

    // ------------------------------------------------------------------ contract targets

    private void SpawnContractTargets()
    {
        const string contractId = "c001_last_call";
        if (ContractManager.Instance.IsCompleted(contractId)) return;

        var contract = ContractManager.Instance.Get(contractId);
        string name = contract?.TargetName ?? "Target";

        // Viktor Lusk smokes outside the Neon Swan.
        var target = new Node3D { Name = "Target_ViktorLusk", Position = new Vector3(0.5f, 0f, 8.6f) };
        AddChild(target);
        target.AddChild(new MeshInstance3D
        {
            Mesh = new CapsuleMesh { Radius = 0.32f, Height = 1.75f, Material = _targetSuit },
            Position = new Vector3(0, 0.875f, 0),
        });
        target.AddChild(new MeshInstance3D
        {
            Mesh = new SphereMesh { Radius = 0.15f, Height = 0.3f, Material = _skin },
            Position = new Vector3(0, 1.85f, 0),
        });
        var takedown = Interactable.Create<ContractTarget>(target, new Vector3(0, 1f, 0), 1.6f);
        takedown.ContractId = contractId;
        takedown.TargetName = name;
    }
}
