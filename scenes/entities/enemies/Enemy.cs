using Godot;
using System;
public partial class Enemy : CharacterBody3D
{
    [Export]
    public float walkSpeed = 1f;
    [Export]
    public float speed;
    [Export]
    public float noticeRadius = 30f;
    [Export]
    public float attackRadius = 1.5f;
    private float speedModifier = 1.0f;
    public CharacterBody3D player;
    public Node3D skin;
    public AnimationTree animTree;
    public AnimationNodeBlendTree blendTree;
    public AnimationNodeStateMachinePlayback moveStateMachine;
    public AnimationNodeAnimation attackAnimation;
    public Timer attackTimer;
    public Timer invulTimer;
    public RandomNumberGenerator RNG = new RandomNumberGenerator();
    public Marker3D marker3D;
    public int _health = 5;
    public int Health
    {
        get => _health;
        set
        {
            _health = value;

            if (_health <= 0)
            {
                this.QueueFree();
            }
        }
    }
    private float _squashAndStretch = 1.0f;
    public float SquashAndStretch
    {
        get => _squashAndStretch;
        set
        {
            _squashAndStretch = value;
            float negative = 1.0f + (1.0f - _squashAndStretch);
            skin.Scale = new Vector3(negative, _squashAndStretch, 1);
        }
    }
    [Signal]
    public delegate void CastSpellEventHandler(string type, Vector3 pos, Vector2 direction, float size);
    public override void _Ready()
    {
        speed = walkSpeed;
        player = (CharacterBody3D)this.GetTree().GetFirstNodeInGroup("Player");
        skin = (Node3D)this.GetNode("skin");
        animTree = this.GetNode<AnimationTree>("AnimationTree");
        blendTree = (AnimationNodeBlendTree)animTree.TreeRoot;
        moveStateMachine = (AnimationNodeStateMachinePlayback)animTree.Get("parameters/MoveStateMachine/playback");
        attackAnimation = (AnimationNodeAnimation)this.blendTree.GetNode("AttackAnimation");
        attackTimer = (Timer)this.GetNode("Timers/AttackTimer");
        invulTimer = (Timer)this.GetNode("Timers/InvulTimer");
    }

    public bool isPlayerWithinNoticeRange()
    {
        return this.Position.DistanceTo(player.Position) < noticeRadius;
    }

    public bool isPlayerWithinAttackRange()
    {
        return this.Position.DistanceTo(player.Position) < attackRadius;
    }

    public void RandomizeAttackTime(float from = 2f, float to = 3f)
    {
        attackTimer.WaitTime = RNG.RandfRange(from, to);
    }

    public void MoveToPlayer(double delta)
    {
        if (isPlayerWithinNoticeRange())
        {
            Vector2 targetVec2 = GetTargetVector2();
            this.Rotation = GetRotationToFacePlayer(delta);

            if (!this.isPlayerWithinAttackRange())
            {
                this.Velocity = new Vector3(targetVec2.X, 0, targetVec2.Y) * speed * speedModifier;
                moveStateMachine.Travel("Walk");
            }
            else
            {
                this.Velocity = Vector3.Zero;
                moveStateMachine.Travel("Idle");
            }
        }
        else
        {
            moveStateMachine.Travel("Idle");
        }

        Vector3 velocity = this.Velocity;
        velocity.Y = !this.IsOnFloor() ? -2 : 0;
        this.Velocity = velocity;
        this.MoveAndSlide();
    }
    public Vector2 GetTargetVector2()
    {
        Vector3 targetDir = (player.Position - this.Position).Normalized();
        return new Vector2(targetDir.X, targetDir.Z);
    }

    public Vector3 GetRotationToFacePlayer(double delta)
    {
        Vector2 targetVec2 = GetTargetVector2();
        Vector3 rotation = this.Rotation;
        float targetAngle = -targetVec2.Angle() + MathF.PI / 2;
        float rotationSpeed = 6.0f;
        rotation.Y = Mathf.RotateToward(rotation.Y, targetAngle, (float)delta * rotationSpeed);
        return rotation;
    }

    public void FireAttackOneShot()
    {
        animTree.Set("parameters/AttackOneShot/request", (int)AnimationNodeOneShot.OneShotRequest.Fire);
    }

    public void AbortAttackOneShot()
    {
        animTree.Set("parameters/AttackOneShot/request", (int)AnimationNodeOneShot.OneShotRequest.Abort);
    }

    public void StopMovement(float startDuration, float endDuration)
    {
        Tween tween = CreateTween();
        tween.TweenProperty(this, "speedModifier", 0.0, startDuration);
        tween.TweenProperty(this, "speedModifier", 1.0, endDuration);
    }

    public void DoSquashAndStretch(float value, float duration = 0.1f)
    {
        Tween tween = CreateTween();
        tween.TweenProperty(this, nameof(SquashAndStretch), value, duration);
        tween.TweenProperty(this, nameof(SquashAndStretch), 1.0, duration * 1.8f).SetEase(Tween.EaseType.Out);
    }

    public void Hit()
    {
        if (!Convert.ToBoolean(invulTimer.TimeLeft))
        {
            DoSquashAndStretch(1.2f, 0.15f);
            Health -= 1;
            invulTimer.Start();
        }
    }

    public void ShootFireball()
    {
        float size = this.Name == "Boss" ? 3f : 1f;
        this.EmitSignal(SignalName.CastSpell, "fireball", marker3D.GlobalPosition, GetTargetVector2(), size);
    }
}
