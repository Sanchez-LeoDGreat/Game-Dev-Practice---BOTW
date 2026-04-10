using Godot;
using Godot.Collections;
using System;
using System.Linq;

public partial class Player : CharacterBody3D
{
	[Export]
	public float baseSpeed = 4f;
	[Export]
	public float runSpeed = 10.0f;
	[Export]
	public float defendSpeed = 1f;
	[Export]
	public float jumpHeight = 2.25f;
	[Export]
	public float jumpTimeToPeak = 0.4f;
	[Export]
	public float jumpTimeToDescent = 0.3f;
	public float speedModifier = 1.0f;
	private float jumpVelocity;
	private float jumpGravity;
	private float fallGravity;
	private int _health = 5;
	public int Health
	{
		get => _health;
		set
		{
			ui.UpdateHealth(value, value - _health);
			_health = value;

			if (_health <= 0)
			{
				this.GetTree().Quit();
			}
		}
	}
	private int _energy = 100;
	public int Energy
	{
		get => _energy;
		set
		{
			ui.UpdateEnergy(value);
			_energy = Mathf.Min(value, 100);
		}
	}
	private int _stamina = 100;
	public int Stamina
	{
		get => _stamina;
		set
		{
			ui.UpdateStamina(_stamina, value);
			if (_stamina == 100 && value < 100)
			{
				ui.ChangeStaminaAlpha(1);
			}
			if (value == 100)
			{
				ui.ChangeStaminaAlpha(0);
			}

			_stamina = Mathf.Clamp(value, 0, 100);
		}
	}
	private bool _defending;
	public bool Defending
	{
		get => _defending;
		set
		{
			if (_defending == value) return;
			_defending = value;

			godetteSkin.Defend(value);
		}
	}
	private bool _weaponActive = true;
	public bool WeaponActive
	{
		get => _weaponActive;
		set
		{
			ui.spellControl.Visible = !value;
			_weaponActive = value;
		}
	}
	public Vector2 lastMovementInput = new Vector2(0, 1);
	Camera3D camera;
	public GodetteSkin godetteSkin;
	AnimationPlayer playerAnimation;
	Vector2 movementInput = Vector2.Zero;
	Ui ui;
	public Timer invulTimer;
	Timer energyRecoveryTimer;
	Timer staminaRecoveryTimer;
	[Signal]
	public delegate void CastSpellEventHandler(string type, Vector3 pos, Vector2 direction, float size);
	public override void _Ready()
	{
		jumpVelocity = (2.0f * jumpHeight) / jumpTimeToPeak;
		jumpGravity = (2.0f * jumpHeight) / (jumpTimeToPeak * jumpTimeToPeak);
		fallGravity = (2.0f * jumpHeight) / (jumpTimeToDescent * jumpTimeToDescent);

		camera = this.GetNode<Camera3D>("CameraController/Camera3D");
		godetteSkin = this.GetNode<GodetteSkin>("GodetteSkin");
		playerAnimation = this.GetNode<AnimationPlayer>("GodetteSkin/AnimationPlayer");
		invulTimer = this.GetNode<Timer>("Timers/InvulTimer");
		energyRecoveryTimer = this.GetNode<Timer>("Timers/EnergyRecoveryTimer");
		staminaRecoveryTimer = this.GetNode<Timer>("Timers/StaminaRecoveryTimer");
		ui = this.GetNode<Ui>("UI");

		ui.Setup(Health);
	}

	public override void _PhysicsProcess(double delta)
	{
		MoveLogic(delta);
		JumpLogic(delta);
		AbilityLogic();
		this.MoveAndSlide();
		this.PhysicsLogic();
	}

	private void MoveLogic(double delta)
	{
		Vector3 velocity = Velocity;
		Vector2 inputDir = Input.GetVector("left", "right", "forward", "backward").Rotated(-camera.GlobalRotation.Y);
		Vector2 velocity2D = new Vector2(velocity.X, velocity.Z);
		bool isRunning = Input.IsActionPressed("run");
		float movementSpeed = isRunning ? runSpeed : baseSpeed;
		movementSpeed = Defending ? defendSpeed : movementSpeed;

		if (inputDir != Vector2.Zero)
		{
			lastMovementInput = inputDir;
			velocity2D += inputDir * movementSpeed * (float)delta * 8.0f;
			velocity2D = velocity2D.LimitLength(movementSpeed) * speedModifier;
			godetteSkin.SetMoveState("Running");

			float targetAngle = -inputDir.Angle() + Mathf.Pi / 2;
			Vector3 rotation = godetteSkin.Rotation;
			rotation.Y = Mathf.RotateToward(rotation.Y, targetAngle, 6.0f * (float)delta);

			godetteSkin.Rotation = rotation;
		}
		else
		{
			velocity2D = velocity2D.MoveToward(Vector2.Zero, baseSpeed * 4.0f * (float)delta);
			godetteSkin.SetMoveState("Idle");
		}

		velocity.X = velocity2D.X;
		velocity.Z = velocity2D.Y;

		this.Velocity = velocity;
	}

	private void JumpLogic(double delta)
	{
		Vector3 velocity = this.Velocity;
		if (IsOnFloor())
		{
			if (Input.IsActionJustPressed("jump") && Stamina >= 20)
			{
				velocity.Y = jumpVelocity;
				godetteSkin.DoSquashAndStretch(1.2f, 0.15f);
				Stamina -= 20;
			}
		}
		else
		{
			godetteSkin.SetMoveState("Jump");
		}
		float gravity = (velocity.Y > 0.0f) ? jumpGravity : fallGravity;
		velocity.Y -= gravity * (float)delta;
		this.Velocity = velocity;
	}

	private void AbilityLogic()
	{
		// Actual Attack
		if (Input.IsActionJustPressed("ability"))
		{
			if (WeaponActive)
			{
				godetteSkin.Attack();
			}
			else
			{
				if (Energy >= 20)
				{
					godetteSkin.CastSpell();
					StopMovement(0.3f, 0.3f);
					Energy -= 20;
				}
			}
		}

		// Defending
		Defending = Input.IsActionPressed("block");

		// Switch Weapon/Magic
		if (Input.IsActionJustPressed("switch weapon") && !godetteSkin.attacking)
		{
			WeaponActive = !WeaponActive;
			godetteSkin.SwitchWeapon(WeaponActive);
		}
		if (Input.IsActionJustPressed("switch spell") && !godetteSkin.attacking)
		{
			ui.currentSpell = ui.Spells.Keys.ToArray()[(ui.currentSpell + 1) % ui.Spells.Count];
			ui.UpdateSpell(ui.Spells, ui.currentSpell);
		}
	}

	public void StopMovement(float startDuration, float endDuration)
	{
		Tween tween = CreateTween();
		tween.TweenProperty(this, "speedModifier", 0.0, startDuration);
		tween.TweenProperty(this, "speedModifier", 1.0, endDuration);
	}

	public void ShootMagic(Vector3 pos)
	{
		if (ui.currentSpell == ui.GetSpellKey("Fireball"))
		{
			this.EmitSignal(SignalName.CastSpell, "fireball", pos, lastMovementInput, 1f);
		}
		else if (ui.currentSpell == ui.GetSpellKey("Heal"))
		{
			Health += 1;
		}
	}

	public void OnEnergyRecoveryTimerTimeout()
	{
		Energy += 1;
	}

	public void OnStaminaRecoveryTimerTimeout()
	{
		Stamina += 1;
	}

	public void PhysicsLogic()
	{
		for (int i = 1; i < this.GetSlideCollisionCount(); i++)
		{
			GodotObject collider = this.GetSlideCollision(i).GetCollider();
			if (collider is RigidBody3D)
			{
				RigidBody3D coll = (RigidBody3D)collider;
				coll.ApplyCentralImpulse(-this.GetSlideCollision(i).GetNormal());
			}
		}
	}
}
