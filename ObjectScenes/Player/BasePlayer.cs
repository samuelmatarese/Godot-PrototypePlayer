using Godot;
using System;
using System.Diagnostics;

public partial class BasePlayer : CharacterBody3D
{
	[Export] public Vector3 StartPosition { get; set; } = Vector3.Zero;
	[Export] public bool CanMove { get; set; }
	[Export] public bool CanJump { get; set; }
	[Export] public bool CanSpectate { get; set; }
	[Export] public bool CanRun { get; set; }
	[Export] public float Speed { get; set; } = 5;
	[Export] public float JumpSpeed { get; set; } = 100;
	[Export] public float MouseSensivity { get; set; } = 0.5f;
	[Export] public float MaxUpwardRotation { get; set; } = 50;
	[Export] public float MaxDownRotation { get; set; } = -50;
	[Export] public string MoveForwardAction { get; set; } = "MoveForward";
	[Export] public string MoveBackwardsAction { get; set; } = "MoveBackwards";
	[Export] public string MoveLeftAction { get; set; } = "MoveLeft";
	[Export] public string MoveRightAction { get; set; } = "MoveRight";
	[Export] public string JumpAction { get; set; } = "Jump";

	public float Gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

	private Camera3D _playerView;
	private Vector2 _cameraMovement;
	private float _verticalVelocity = 0f;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Input.MouseMode = Input.MouseModeEnum.Hidden;
		Input.MouseMode = Input.MouseModeEnum.Captured;

		var camera = GetNodeOrNull<Camera3D>("PlayerView");

		if (camera == null)
		{
			throw new ArgumentException("Camera node could not be found.");
		}

		_playerView = camera;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public override void _PhysicsProcess(double delta)
	{
		ProcessRotation();
		ProcessMovement(delta);
		base._PhysicsProcess(delta);
	}

	private void ProcessRotation()
	{
		if (_cameraMovement.Y + _playerView.RotationDegrees.X < MaxUpwardRotation &&
			_cameraMovement.Y + _playerView.RotationDegrees.X > MaxDownRotation)
		{
			var cameraRotation = new Vector3(_cameraMovement.Y, 0.0f, 0.0f);
			_playerView.RotationDegrees += cameraRotation;
		}

		RotationDegrees += new Vector3(0.0f, _cameraMovement.X, 0.0f);
		_cameraMovement = Vector2.Zero;
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseMotion inputEventMouseMotion)
		{
			_cameraMovement.X = -inputEventMouseMotion.Relative.X * MouseSensivity;
			_cameraMovement.Y = -inputEventMouseMotion.Relative.Y * MouseSensivity;
		}
	}

	private void ProcessMovement(double delta)
	{
		var direction = Vector3.Zero;

		if (CanMove)
		{
			if (Input.IsActionPressed(MoveForwardAction))
			{
				direction.Z -= 1;
			}

			if (Input.IsActionPressed(MoveBackwardsAction))
			{
				direction.Z += 1;
			}

			if (Input.IsActionPressed(MoveLeftAction))
			{
				direction.X -= 1;
			}

			if (Input.IsActionPressed(MoveRightAction))
			{
				direction.X += 1;
			}
		}

		if (Input.IsActionPressed(JumpAction) && IsOnFloor() && CanJump)
		{
			_verticalVelocity = JumpSpeed;
		}

		if (_verticalVelocity > 0)
		{
			_verticalVelocity -= Gravity * 0.08f * (float)delta;
		}
		else
		{
			_verticalVelocity -= Gravity * 0.1f * (float)delta;
		}

		if (direction.Length() > 0)
		{
			direction = direction.Normalized();
		}

		direction.X *= Speed;
		direction.Z *= Speed;

		var relativeDirection = GlobalTransform.Basis * direction;
		Velocity = new Vector3(relativeDirection.X, _verticalVelocity, relativeDirection.Z);
		MoveAndSlide();

		if (IsOnFloor())
		{
			_verticalVelocity = 0;
		}
	}
}
