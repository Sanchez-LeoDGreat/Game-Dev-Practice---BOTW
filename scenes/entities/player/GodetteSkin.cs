using Godot;
using Godot.Collections;
using System;
public partial class GodetteSkin : Node3D
{
	AnimationTree animTree;
	AnimationNodeBlendTree blendTree;
	AnimationNodeStateMachinePlayback moveStateMachine;
	AnimationNodeStateMachinePlayback attackStateMachine;
	AnimationNodeAnimation extraAnimation;
	StandardMaterial3D faceMaterial;
	Sword sword;
	Node3D wand;
	Timer secondAttackTimer;
	Timer blinkTimer;
	Marker3D marker3D;
	public bool attacking = false;
	Player player;
	private float _squashAndStretch = 1.0f;
	public float SquashAndStretch
	{
		get => _squashAndStretch;
		set
		{
			_squashAndStretch = value;
			float negative = 1.0f + (1.0f - _squashAndStretch);
			this.Scale = new Vector3(negative, _squashAndStretch, 1);
		}
	}
	Dictionary faces = new Dictionary
	{
		["default"] = Vector3.Zero,
		["blink"] = new Vector3(0, 0.5f, 0)
	};
	RandomNumberGenerator RNG = new RandomNumberGenerator();

	public override void _Ready()
	{
		animTree = this.GetNode<AnimationTree>("AnimationTree");
		blendTree = (AnimationNodeBlendTree)animTree.TreeRoot;

		moveStateMachine = (AnimationNodeStateMachinePlayback)animTree.Get("parameters/MoveStateMachine/playback");
		attackStateMachine = (AnimationNodeStateMachinePlayback)animTree.Get("parameters/AttackStateMachine/playback");

		extraAnimation = (AnimationNodeAnimation)blendTree.GetNode("ExtraAnimation");

		sword = this.GetNode<Sword>("Rig/Skeleton3D/RightHandSlot/Sword");
		wand = this.GetNode<Node3D>("Rig/Skeleton3D/RightHandSlot/wand2");

		secondAttackTimer = this.GetNode<Timer>("SecondAttackTimer");
		blinkTimer = this.GetNode<Timer>("BlinkTimer");

		MeshInstance3D headMesh = this.GetNode<MeshInstance3D>("Rig/Skeleton3D/Godette_Head");
		faceMaterial = (StandardMaterial3D)headMesh.GetSurfaceOverrideMaterial(0);

		marker3D = this.GetNode<Marker3D>("Rig/Skeleton3D/RightHandSlot/wand2/wand/Marker3D");
		player = (Player)this.GetParent();
	}

	public void SetMoveState(string stateName)
	{
		moveStateMachine.Travel(stateName);
	}

	public void Attack()
	{
		if (!attacking)
		{
			attackStateMachine.Travel(Convert.ToBoolean(secondAttackTimer.TimeLeft) ? "Slice" : "Chop");
			animTree.Set("parameters/AttackOneShot/request", (int)AnimationNodeOneShot.OneShotRequest.Fire);
		}
	}

	public void AttackToggle(bool value)
	{
		attacking = value;
	}

	public void Defend(bool forward)
	{
		Tween tween = this.CreateTween();
		tween.TweenMethod(new Callable(this, nameof(_DefendChange)), 1.0f - Convert.ToSingle(forward), Convert.ToSingle(forward), 0.25f);
	}

	public void _DefendChange(float value)
	{
		animTree.Set("parameters/ShieldBlend/blend_amount", value);
	}

	public void SwitchWeapon(bool weaponActive)
	{
		if (weaponActive)
		{
			sword.Show();
			wand.Hide();
		}
		else
		{
			sword.Hide();
			wand.Show();
		}
		DoSquashAndStretch(1.2f, 0.15f);
	}

	public void CastSpell()
	{
		if (!attacking)
		{
			extraAnimation.Animation = "Spellcast_Shoot";
			animTree.Set("parameters/ExtraOneShot/request", (int)AnimationNodeOneShot.OneShotRequest.Fire);
		}
	}

	public void ShootFireball()
	{
		player.ShootFireball(marker3D.GlobalPosition);
	}

	public void Hit()
	{
		extraAnimation.Animation = "Hit_A";
		animTree.Set("parameters/ExtraOneShot/request", (int)AnimationNodeOneShot.OneShotRequest.Fire);
		animTree.Set("parameters/AttackOneShot/request", (int)AnimationNodeOneShot.OneShotRequest.Abort);
		attacking = false;
	}

	public void DoSquashAndStretch(float value, float duration = 0.1f)
	{
		Tween tween = CreateTween();
		tween.TweenProperty(this, nameof(SquashAndStretch), value, duration);
		tween.TweenProperty(this, nameof(SquashAndStretch), 1.0, duration * 1.8f).SetEase(Tween.EaseType.Out);
	}

	public void ChangeFace(string expression)
	{
		faceMaterial.Uv1Offset = (Vector3)faces[expression];
	}

	public async void OnBlinkTimerTimeout()
	{
		ChangeFace("blink");
		SceneTreeTimer timer = this.GetTree().CreateTimer(0.2);
		await ToSignal(timer, "timeout");
		ChangeFace("default");
		blinkTimer.WaitTime = RNG.RandfRange(1.5f, 3.0f);
	}

	public void CanDamage(bool value)
	{
		sword.canDamage = value;
	}
}
