using System.Globalization;
using Godot;
using Hurman.Core;

namespace Hurman.UI;

/// <summary>
/// On-screen HUD built in code: stats (top-left), objective (top-right),
/// interact prompt (bottom), toast messages (top-center) and a crosshair while aiming.
/// </summary>
public partial class Hud : CanvasLayer
{
    private Label _stats;
    private Label _objective;
    private Label _prompt;
    private Label _toast;
    private Label _crosshair;
    private Label _help;
    private float _toastTimeLeft;

    public override void _Ready()
    {
        var root = new Control { MouseFilter = Control.MouseFilterEnum.Ignore };
        root.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        AddChild(root);

        _stats = MakeLabel(root, new(0, 0, 0.5f, 0), new(24, 18, 0, 140), HorizontalAlignment.Left, 22);
        _objective = MakeLabel(root, new(0.5f, 0, 1, 0), new(0, 18, -24, 140), HorizontalAlignment.Right, 20);
        _objective.Modulate = new Color(1f, 0.85f, 0.55f);
        _prompt = MakeLabel(root, new(0, 1, 1, 1), new(0, -130, 0, -90), HorizontalAlignment.Center, 24);
        _toast = MakeLabel(root, new(0, 0, 1, 0), new(320, 150, -320, 260), HorizontalAlignment.Center, 22);
        _toast.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        _toast.Modulate = new Color(0.6f, 1f, 0.95f);
        _crosshair = MakeLabel(root, new(0.5f, 0.5f, 0.5f, 0.5f), new(-20, -20, 20, 20), HorizontalAlignment.Center, 26);
        _crosshair.VerticalAlignment = VerticalAlignment.Center;
        _crosshair.Text = "+";
        _crosshair.Visible = false;
        _help = MakeLabel(root, new(0, 1, 1, 1), new(24, -40, -24, -12), HorizontalAlignment.Left, 15);
        _help.Modulate = new Color(1, 1, 1, 0.55f);
        _help.Text = "WASD move · Shift sprint · C crouch · Space jump · RMB aim · E interact · Esc free mouse · F9 new game";

        var bus = EventBus.Instance;
        bus.MoneyChanged += OnIntChanged;
        bus.SovereignsChanged += OnIntChanged;
        bus.CredChanged += OnIntChanged;
        bus.DayChanged += OnIntChanged;
        bus.HeatChanged += OnHeatChanged;
        bus.ObjectiveChanged += OnObjectiveChanged;
        bus.InteractPromptChanged += OnPromptChanged;
        bus.Toast += OnToast;
        bus.AimChanged += OnAimChanged;

        RefreshStats();
        _objective.Text = ContractManager.Instance.CurrentObjective;
        _toast.Text = "";
    }

    public override void _ExitTree()
    {
        var bus = EventBus.Instance;
        if (bus == null) return;
        bus.MoneyChanged -= OnIntChanged;
        bus.SovereignsChanged -= OnIntChanged;
        bus.CredChanged -= OnIntChanged;
        bus.DayChanged -= OnIntChanged;
        bus.HeatChanged -= OnHeatChanged;
        bus.ObjectiveChanged -= OnObjectiveChanged;
        bus.InteractPromptChanged -= OnPromptChanged;
        bus.Toast -= OnToast;
        bus.AimChanged -= OnAimChanged;
    }

    public override void _Process(double delta)
    {
        if (_toastTimeLeft <= 0f) return;
        _toastTimeLeft -= (float)delta;
        var c = _toast.Modulate;
        _toast.Modulate = new Color(c.R, c.G, c.B, Mathf.Clamp(_toastTimeLeft, 0f, 1f));
    }

    private void OnIntChanged(int _) => RefreshStats();
    private void OnHeatChanged(float _) => RefreshStats();
    private void OnObjectiveChanged(string text) => _objective.Text = text;
    private void OnPromptChanged(string text) => _prompt.Text = text;
    private void OnAimChanged(bool aiming) => _crosshair.Visible = aiming;

    private void OnToast(string text)
    {
        _toast.Text = text;
        _toastTimeLeft = 3f + text.Length / 25f;
    }

    private void RefreshStats()
    {
        var gs = GameState.Instance;
        int heat = Mathf.RoundToInt(gs.Heat);
        string heatBar = new string('#', heat) + new string('-', (int)GameState.MaxHeat - heat);
        _stats.Text =
            $"FRED HALE · Day {gs.Day}\n" +
            $"${gs.Money.ToString("N0", CultureInfo.InvariantCulture)}   ◉ {gs.Sovereigns} Sovereigns\n" +
            $"Cred {gs.Cred}   Heat [{heatBar}]";
    }

    private static Label MakeLabel(Control parent, Vector4 anchors, Vector4 offsets, HorizontalAlignment align, int fontSize)
    {
        var label = new Label
        {
            AnchorLeft = anchors.X, AnchorTop = anchors.Y, AnchorRight = anchors.Z, AnchorBottom = anchors.W,
            OffsetLeft = offsets.X, OffsetTop = offsets.Y, OffsetRight = offsets.Z, OffsetBottom = offsets.W,
            HorizontalAlignment = align,
            MouseFilter = Control.MouseFilterEnum.Ignore,
            LabelSettings = new LabelSettings
            {
                FontSize = fontSize,
                OutlineSize = 6,
                OutlineColor = new Color(0, 0, 0, 0.85f),
            },
        };
        parent.AddChild(label);
        return label;
    }
}
