using Godot;
using Godot.Collections;
using System;
using System.Linq;

public partial class SkeletonWarrior : Enemy
{
    private Bone bone;
    Dictionary<string, string> meleeAttacks = new Dictionary<string, string>
    {
        ["Chop"] = "1H_Melee_Attack_Chop",
        ["Slice1"] = "1H_Melee_Attack_Slice_Diagonal",
        ["Slice2"] = "1H_Melee_Attack_Slice_Horizontal",
        ["Stab"] = "1H_Melee_Attack_Stab"
    };

    public override void _Ready()
    {
        base._Ready();
        bone = (Bone)this.GetNode<Bone>("skin/Rig/Skeleton3D/Skeleton_Sword/Bone");
        this._health = 3;
    }

    public override void _PhysicsProcess(double delta)
    {
        this.MoveToPlayer(delta);
    }

    public void OnAttackTimerTimeout()
    {
        this.RandomizeAttackTime();
        if (this.isPlayerWithinNoticeRange() && this.isPlayerWithinAttackRange())
        {
            MeleeAttackAnimation();
        }
    }

    public void MeleeAttackAnimation()
    {
        string[] attacks = meleeAttacks.Values.Cast<string>().ToArray();
        attackAnimation.Animation = attacks[this.RNG.RandiRange(0, meleeAttacks.Count - 1)];
        this.FireAttackOneShot();
    }

    public void CanDamage(bool value)
    {
        bone.canDamage = value;
    }
}
