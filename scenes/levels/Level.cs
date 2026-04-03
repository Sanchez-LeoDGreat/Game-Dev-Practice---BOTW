using Godot;
using System;

public partial class Level : Node3D
{
    PackedScene fireballScene = ResourceLoader.Load<PackedScene>("res://scenes/projectiles/fireball.tscn");
    Node3D projectiles;

    public override void _Ready()
    {
        projectiles = this.GetNode<Node3D>("Projectiles");
    }

    public void OnEntityCastSpell(string type, Vector3 pos, Vector2 direction, float size)
    {
        Fireball fireball = (Fireball)fireballScene.Instantiate();
        projectiles.AddChild(fireball);
        fireball.Setup(size);
        fireball.GlobalPosition = pos;
        fireball.direction = direction;
    }
}
