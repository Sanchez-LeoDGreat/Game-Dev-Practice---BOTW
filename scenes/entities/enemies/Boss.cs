using Godot;
using Godot.Collections;
using System;

public partial class Boss : Enemy
{
    [Export]
    private float spinSpeed = 6.0f;
    private bool spinning = false;
    private bool canDamage = false;
    private RayCast3D rayCast3D;
    Dictionary<string, string> simpleAttacks = new Dictionary<string, string>
    {
        ["Slice"] = "2H_Melee_Attack_Slice",
        ["Spin"] = "2H_Melee_Attack_Spin",
        ["Range"] = "1H_Melee_Attack_Stab"
    };
    public override void _Ready()
    {
        this.canLaunchProjectiles = true;
        base._Ready();
        this.walkSpeed = 5f;
        rayCast3D = this.GetNode<RayCast3D>("skin/Rig/Skeleton3D/Nagonford_Axe/RayCast3D");
        marker3D = this.GetNode<Marker3D>("skin/Rig/Skeleton3D/Nagonford_Axe/Nagonford_Axe/Marker3D");
    }

    public override void _PhysicsProcess(double delta)
    {
        this.MoveToPlayer(delta);
    }

    public override void _Process(double delta)
    {
        if (!canDamage) return;

        if (rayCast3D.IsColliding())
        {
            Player collider = (Player)rayCast3D.GetCollider();

            if (collider != null && collider.IsInGroup("Player"))
            {
                collider.godetteSkin.Hit();
            }
        }
    }

    public void OnAttackTimerTimeout()
    {
        this.RandomizeAttackTime(4f, 5.5f);
        if (this.isPlayerWithinNoticeRange())
        {
            if (this.isPlayerWithinAttackRange())
            {
                MeleeAttackAnimation();
            }
            else
            {
                // if (Convert.ToBoolean(this.RNG.Randi() % 2))
                // {
                //     RangeAttackAnimation();
                // }
                // else
                // {
                //     SpinAttackAnimation();
                // }
                RangeAttackAnimation();
            }
        }
    }

    public void SpinAttackAnimation()
    {
        Tween tween = this.CreateTween();
        tween.TweenProperty(this, "speed", spinSpeed, 0.5f);
        tween.TweenMethod(new Callable(this, nameof(_SpinTransition)), 0f, 1f, 0.3f);
        this.attackTimer.Stop();
        spinning = true;
    }

    private void _SpinTransition(float value)
    {
        this.animTree.Set("parameters/SpinBlend/blend_amount", value);
    }

    public void RangeAttackAnimation()
    {
        this.StopMovement(1.5f, 1.5f);
        this.attackAnimation.Animation = simpleAttacks["Range"];
        this.FireAttackOneShot();
    }

    public void MeleeAttackAnimation()
    {
        this.attackAnimation.Animation = simpleAttacks[Convert.ToBoolean(this.RNG.Randi() % 2) ? "Slice" : "Spin"];
        this.FireAttackOneShot();
    }

    public async void OnArea3DBodyEntered(Node3D body)
    {
        if (spinning)
        {
            SceneTreeTimer timer = this.GetTree().CreateTimer(this.RNG.RandfRange(1f, 2f));
            await ToSignal(timer, "timeout");
            Tween tween = this.CreateTween();
            tween.TweenProperty(this, "speed", walkSpeed, 0.5f);
            tween.TweenMethod(new Callable(this, nameof(_SpinTransition)), 1f, 0f, 0.3f);
            spinning = false;
            this.attackTimer.Start();
        }
    }

    public void CanDamage(bool value)
    {
        canDamage = value;
    }
}
