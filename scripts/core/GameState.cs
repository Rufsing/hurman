using System.Collections.Generic;
using System.Text.Json;
using Godot;

namespace Hurman.Core;

/// <summary>
/// Fred's persistent progress (autoload): money, Sovereigns, street cred, heat,
/// base upgrades and contract history. Saved as JSON in user://save.json.
/// </summary>
public partial class GameState : Node
{
    public static GameState Instance { get; private set; }

    public const float MaxHeat = 5f;
    private const string SavePath = "user://save.json";
    private const int StartingMoney = 500;

    public int Money { get; private set; } = StartingMoney;
    public int Sovereigns { get; private set; }
    public int Cred { get; private set; }
    public float Heat { get; private set; }
    public int Day { get; private set; } = 1;
    public string ActiveContractId { get; set; } = "";
    public HashSet<string> CompletedContracts { get; private set; } = new();
    private Dictionary<string, int> _upgrades = new();

    public override void _EnterTree() => Instance = this;

    public override void _Ready()
    {
        Load();
    }

    public override void _UnhandledInput(InputEvent e)
    {
        if (e.IsActionPressed("debug_reset"))
        {
            ResetToNewGame();
            GetTree().ReloadCurrentScene();
        }
    }

    // ---------- Economy ----------

    public void AddMoney(int amount)
    {
        Money = Mathf.Max(0, Money + amount);
        Bus.EmitSignal(EventBus.SignalName.MoneyChanged, Money);
    }

    public bool TrySpendMoney(int amount)
    {
        if (Money < amount) return false;
        AddMoney(-amount);
        return true;
    }

    public void AddSovereigns(int amount)
    {
        Sovereigns = Mathf.Max(0, Sovereigns + amount);
        Bus.EmitSignal(EventBus.SignalName.SovereignsChanged, Sovereigns);
    }

    public void AddCred(int amount)
    {
        Cred = Mathf.Max(0, Cred + amount);
        Bus.EmitSignal(EventBus.SignalName.CredChanged, Cred);
    }

    public void AddHeat(float amount)
    {
        Heat = Mathf.Clamp(Heat + amount, 0f, MaxHeat);
        Bus.EmitSignal(EventBus.SignalName.HeatChanged, Heat);
    }

    // ---------- Base upgrades ----------

    public int GetUpgradeLevel(string id) => _upgrades.TryGetValue(id, out var level) ? level : 0;

    public void SetUpgradeLevel(string id, int level)
    {
        _upgrades[id] = level;
        Bus.EmitSignal(EventBus.SignalName.UpgradeChanged, id, level);
    }

    // ---------- Time ----------

    /// <summary>Sleep in the basement: next day, heat cools down, game saves.</summary>
    public void Sleep()
    {
        Day++;
        AddHeat(-1f);
        Bus.EmitSignal(EventBus.SignalName.DayChanged, Day);
        Save();
    }

    // ---------- Save / load ----------

    private class SaveData
    {
        public int Version { get; set; } = 1;
        public int Money { get; set; }
        public int Sovereigns { get; set; }
        public int Cred { get; set; }
        public float Heat { get; set; }
        public int Day { get; set; }
        public string ActiveContractId { get; set; } = "";
        public List<string> CompletedContracts { get; set; } = new();
        public Dictionary<string, int> Upgrades { get; set; } = new();
    }

    public void Save()
    {
        var data = new SaveData
        {
            Money = Money,
            Sovereigns = Sovereigns,
            Cred = Cred,
            Heat = Heat,
            Day = Day,
            ActiveContractId = ActiveContractId,
            CompletedContracts = new List<string>(CompletedContracts),
            Upgrades = new Dictionary<string, int>(_upgrades),
        };
        using var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Write);
        if (file == null)
        {
            GD.PushError($"Could not write save: {FileAccess.GetOpenError()}");
            return;
        }
        file.StoreString(JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true }));
    }

    public void Load()
    {
        if (!FileAccess.FileExists(SavePath)) return;
        using var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Read);
        if (file == null) return;

        SaveData data;
        try
        {
            data = JsonSerializer.Deserialize<SaveData>(file.GetAsText());
        }
        catch (JsonException ex)
        {
            GD.PushWarning($"Save file is corrupt, starting fresh: {ex.Message}");
            return;
        }
        if (data == null) return;

        Money = data.Money;
        Sovereigns = data.Sovereigns;
        Cred = data.Cred;
        Heat = data.Heat;
        Day = Mathf.Max(1, data.Day);
        ActiveContractId = data.ActiveContractId ?? "";
        CompletedContracts = new HashSet<string>(data.CompletedContracts ?? new List<string>());
        _upgrades = data.Upgrades ?? new Dictionary<string, int>();
    }

    public void ResetToNewGame()
    {
        Money = StartingMoney;
        Sovereigns = 0;
        Cred = 0;
        Heat = 0;
        Day = 1;
        ActiveContractId = "";
        CompletedContracts = new HashSet<string>();
        _upgrades = new Dictionary<string, int>();
        if (FileAccess.FileExists(SavePath))
            DirAccess.RemoveAbsolute(ProjectSettings.GlobalizePath(SavePath));
        EventBus.ShowToast("New game (save deleted).");
    }

    private static EventBus Bus => EventBus.Instance;
}
