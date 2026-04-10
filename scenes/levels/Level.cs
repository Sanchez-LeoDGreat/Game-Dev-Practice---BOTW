using Godot;
using System;
using System.Collections.Generic;

public partial class Level : Node3D
{
    PackedScene fireballScene = ResourceLoader.Load<PackedScene>("res://scenes/projectiles/fireball.tscn");
    Node3D projectiles;
    Dictionary<string, string> scenes = new Dictionary<string, string>
    {
        ["dungeon"] = "res://scenes/levels/dungeon.tscn",
        ["overworld"] = "res://scenes/levels/overworld.tscn"
    };

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

    public void SwitchLevel(String target)
    {
        this.CallDeferred(nameof(_SwitchLevel), target);
    }

    private void _SwitchLevel(String target)
    {
        this.GetTree().ChangeSceneToFile(scenes[target]);
    }
}
