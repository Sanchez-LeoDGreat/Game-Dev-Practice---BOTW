using Godot;
using System;
public partial class SkeletonMage : Enemy
{
	[Export]
	public float moveAwayRange = 10f;
	[Export]
	public float backwardWalkSpeed = 0.8f;
	public override void _Ready()
	{
		this.canLaunchProjectiles = true;
		base._Ready();
		this.marker3D = this.GetNode<Marker3D>("skin/Rig/Skeleton3D/BoneAttachment3D/wand2/wand/Marker3D");
		this._health = 2;
	}

	public bool isPlayerTooClose()
	{
		return this.Position.DistanceTo(player.Position) < moveAwayRange;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (this.Position.DistanceTo(player.Position) < moveAwayRange)
		{
			MoveAwayFromPlayer(delta);
		}
		else
		{
			this.MoveToPlayer(delta);
		}
	}

	private void MoveAwayFromPlayer(double delta)
	{
		if (this.isPlayerWithinNoticeRange())
		{
			Vector2 targetVec2 = this.GetTargetVector2();
			this.Rotation = this.GetRotationToFacePlayer(delta);

			this.Velocity = new Vector3(-targetVec2.X, 0, -targetVec2.Y) * backwardWalkSpeed;
			moveStateMachine.Travel("Walk_Backwards");
			this.MoveAndSlide();
		}
	}

	public void OnAttackTimerTimeout()
	{
		this.RandomizeAttackTime(3f, 5.5f);
		if (isPlayerWithinAttackRange())
		{
			RangeAttackAnimation();
		}
	}

	public void RangeAttackAnimation()
	{
		this.FireAttackOneShot();
	}
}
