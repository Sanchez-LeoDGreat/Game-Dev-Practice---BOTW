using Godot;
using System;

public partial class Bone : Node3D
{
	RayCast3D raycast3D;
	public bool canDamage;
	public override void _Ready()
	{
		raycast3D = (RayCast3D)this.GetNode("RayCast3D");
	}
	public override void _Process(double delta)
	{
		if (!canDamage) return;

		if (raycast3D.IsColliding())
		{
			Player collider = (Player)raycast3D.GetCollider();

			if (collider != null && collider.IsInGroup("Player"))
			{
				collider.godetteSkin.Hit();
			}
		}
	}
}
