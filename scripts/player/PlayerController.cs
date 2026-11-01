using Godot;
using Hurman.Core;
using Hurman.Interaction;

namespace Hurman.Player;

/// <summary>
/// Fred: third-person movement with an over-the-shoulder camera.
/// The root body never rotates. "CameraPivot" holds the look yaw/pitch, and "Model" turns to face
/// the move direction (or the camera direction while aiming).
/// </summary>
public partial class PlayerController : CharacterBody3D
{
    [ExportGroup("Movement")]
    [Export] public float WalkSpeed { get; set; } = 3.6f;
    [Export] public float SprintSpeed { get; set; } = 6.8f;
    [Export] public float CrouchSpeed { get; set; } = 1.8f;
    [Export] public float AimSpeed { get; set; } = 2.6f;
    [Export] public float Acceleration { get; set; } = 12f;
    [Export] public float AirControl { get; set; } = 0.3f;
    [Export] public float JumpVelocity { get; set; } = 4.2f;
    [Export] public float TurnSpeed { get; set; } = 12f;

    [ExportGroup("Camera")]
    [Export] public float MouseSensitivity { get; set; } = 0.0025f;
    [Export] public float PadLookSpeed { get; set; } = 3f;
    [Export] public float NormalFov { get; set; } = 70f;
    [Export] public float AimFov { get; set; } = 50f;
    [Export] public float NormalArmLength { get; set; } = 3.2f;
    [Export] public float AimArmLength { get; set; } = 1.4f;
    [Export] public Vector3 ShoulderOffset { get; set; } = new(0.5f, 0f, 0f);
    [Export] public Vector3 AimShoulderOffset { get; set; } = new(0.65f, 0.05f, 0f);
    [Export] public float StandEyeHeight { get; set; } = 1.6f;
    [Export] public float CrouchEyeHeight { get; set; } = 1.05f;

    private const float StandHeight = 1.8f;
    private const float CrouchHeight = 1.1f;
    private const float MinPitch = -1.25f;
    private const float MaxPitch = 0.9f;

    private Node3D _pivot;
    private SpringArm3D _arm;
    private Camera3D _camera;
    private Node3D _model;
    private CollisionShape3D _collision;
    private CapsuleShape3D _capsule;
    private Area3D _sensor;

    private float _yaw;
    private float _pitch;
    private float _gravity;
    private bool _crouching;
    private Vector3 _spawnPoint;
    private string _lastPrompt = "";
    private Interactable _focused;

    public bool IsAiming { get; private set; }
    public bool IsCrouching => _crouching;
    public Camera3D Camera => _camera;

    public override void _Ready()
    {
        _pivot = GetNode<Node3D>("CameraPivot");
        _arm = GetNode<SpringArm3D>("CameraPivot/SpringArm3D");
        _camera = GetNode<Camera3D>("CameraPivot/SpringArm3D/Camera3D");
        _model = GetNode<Node3D>("Model");
        _collision = GetNode<CollisionShape3D>("CollisionShape3D");
        _capsule = (CapsuleShape3D)_collision.Shape;
        _sensor = GetNode<Area3D>("InteractSensor");

        // Spawn facing is taken from the scene rotation, then the root is un-rotated.
        _yaw = Rotation.Y;
        Rotation = Vector3.Zero;
        _model.Rotation = new Vector3(0, _yaw, 0);

        _arm.AddExcludedObject(GetRid());
        _gravity = (float)ProjectSettings.GetSetting("physics/3d/default_gravity");
        _spawnPoint = GlobalPosition;
        _camera.Current = true;
        Input.MouseMode = Input.MouseModeEnum.Captured;
    }

    public override void _UnhandledInput(InputEvent e)
    {
        if (e.IsActionPressed("pause"))
        {
            Input.MouseMode = Input.MouseMode == Input.MouseModeEnum.Captured
                ? Input.MouseModeEnum.Visible
                : Input.MouseModeEnum.Captured;
            return;
        }

        if (Input.MouseMode != Input.MouseModeEnum.Captured)
        {
            // Click back into the game window to recapture the mouse.
            if (e is InputEventMouseButton { Pressed: true })
                Input.MouseMode = Input.MouseModeEnum.Captured;
            return;
        }

        if (e is InputEventMouseMotion motion)
        {
            float sensitivity = MouseSensitivity * (IsAiming ? 0.6f : 1f);
            _yaw -= motion.Relative.X * sensitivity;
            _pitch = Mathf.Clamp(_pitch - motion.Relative.Y * sensitivity, MinPitch, MaxPitch);
        }
        else if (e.IsActionPressed("interact"))
        {
            _focused?.Interact(this);
        }
        else if (e.IsActionPressed("crouch"))
        {
            SetCrouching(!_crouching);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        float dt = (float)delta;

        // Gamepad look
        Vector2 look = Input.GetVector("look_left", "look_right", "look_up", "look_down");
        _yaw -= look.X * PadLookSpeed * dt;
        _pitch = Mathf.Clamp(_pitch - look.Y * PadLookSpeed * dt, MinPitch, MaxPitch);
        _pivot.Rotation = new Vector3(_pitch, _yaw, 0f);

        bool aiming = Input.IsActionPressed("aim") && Input.MouseMode == Input.MouseModeEnum.Captured;
        if (aiming != IsAiming)
        {
            IsAiming = aiming;
            EventBus.Instance.EmitSignal(EventBus.SignalName.AimChanged, aiming);
        }

        Move(dt);
        UpdateFacing(dt);
        UpdateCamera(dt);
        UpdateInteractFocus();

        if (GlobalPosition.Y < -40f) Respawn();
    }

    private void Move(float dt)
    {
        Vector3 velocity = Velocity;
        if (!IsOnFloor())
            velocity.Y -= _gravity * dt;
        else if (Input.IsActionJustPressed("jump") && !_crouching)
            velocity.Y = JumpVelocity;

        Vector2 input = Input.GetVector("move_left", "move_right", "move_forward", "move_back");
        Vector3 direction = new Basis(Vector3.Up, _yaw) * new Vector3(input.X, 0f, input.Y);

        bool sprinting = Input.IsActionPressed("sprint") && !IsAiming && input.LengthSquared() > 0.01f;
        if (sprinting && _crouching) SetCrouching(false);

        float speed = IsAiming ? AimSpeed
            : _crouching ? CrouchSpeed
            : sprinting ? SprintSpeed
            : WalkSpeed;

        float accel = IsOnFloor() ? Acceleration : Acceleration * AirControl;
        float blend = 1f - Mathf.Exp(-accel * dt);
        Vector3 target = direction * speed;
        velocity.X = Mathf.Lerp(velocity.X, target.X, blend);
        velocity.Z = Mathf.Lerp(velocity.Z, target.Z, blend);

        Velocity = velocity;
        MoveAndSlide();
    }

    private void UpdateFacing(float dt)
    {
        float targetYaw = _model.Rotation.Y;
        if (IsAiming)
        {
            targetYaw = _yaw;
        }
        else
        {
            Vector3 flat = new(Velocity.X, 0f, Velocity.Z);
            if (flat.LengthSquared() > 0.25f)
                targetYaw = Mathf.Atan2(-flat.X, -flat.Z); // model forward is -Z
        }
        float yaw = Mathf.LerpAngle(_model.Rotation.Y, targetYaw, 1f - Mathf.Exp(-TurnSpeed * dt));
        _model.Rotation = new Vector3(0f, yaw, 0f);
    }

    private void UpdateCamera(float dt)
    {
        float t = 1f - Mathf.Exp(-10f * dt);
        _arm.SpringLength = Mathf.Lerp(_arm.SpringLength, IsAiming ? AimArmLength : NormalArmLength, t);
        _arm.Position = _arm.Position.Lerp(IsAiming ? AimShoulderOffset : ShoulderOffset, t);
        _camera.Fov = Mathf.Lerp(_camera.Fov, IsAiming ? AimFov : NormalFov, t);
        float eye = _crouching ? CrouchEyeHeight : StandEyeHeight;
        _pivot.Position = new Vector3(0f, Mathf.Lerp(_pivot.Position.Y, eye, t), 0f);
    }

    private void SetCrouching(bool crouch)
    {
        if (crouch == _crouching) return;
        if (!crouch && !HasHeadroom()) return;

        _crouching = crouch;
        _capsule.Height = crouch ? CrouchHeight : StandHeight;
        _collision.Position = new Vector3(0f, _capsule.Height / 2f, 0f);
        _model.Scale = new Vector3(1f, crouch ? CrouchHeight / StandHeight : 1f, 1f);
    }

    private bool HasHeadroom()
    {
        var space = GetWorld3D().DirectSpaceState;
        var query = PhysicsRayQueryParameters3D.Create(
            GlobalPosition + Vector3.Up * CrouchHeight,
            GlobalPosition + Vector3.Up * (StandHeight + 0.05f),
            Layers.World);
        query.Exclude = new Godot.Collections.Array<Rid> { GetRid() };
        return space.IntersectRay(query).Count == 0;
    }

    private void UpdateInteractFocus()
    {
        Interactable best = null;
        float bestDistance = float.MaxValue;
        foreach (Area3D area in _sensor.GetOverlappingAreas())
        {
            if (area is not Interactable interactable || !interactable.CanInteract(this)) continue;
            float d = GlobalPosition.DistanceSquaredTo(interactable.GlobalPosition);
            if (d < bestDistance)
            {
                bestDistance = d;
                best = interactable;
            }
        }
        _focused = best;

        string prompt = best != null ? $"[E] {best.GetPrompt()}" : "";
        if (prompt != _lastPrompt)
        {
            _lastPrompt = prompt;
            EventBus.Instance.EmitSignal(EventBus.SignalName.InteractPromptChanged, prompt);
        }
    }

    private void Respawn()
    {
        GlobalPosition = _spawnPoint;
        Velocity = Vector3.Zero;
    }
}
