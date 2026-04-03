using Godot;
using System;

public partial class Sword : Node3D
{
	RayCast3D rayCast3D;
	public bool canDamage = false;

	public override void _Ready()
	{
		rayCast3D = this.GetNode<RayCast3D>("RayCast3D");
	}
	public override void _Process(double delta)
	{
		if (!canDamage) return;

		if (rayCast3D.IsColliding())
		{
			Enemy collider = (Enemy)rayCast3D.GetCollider();
			if (collider != null && collider.IsInGroup("Enemy"))
			{
				collider.Hit();
			}
		}
	}
}
