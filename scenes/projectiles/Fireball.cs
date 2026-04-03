using Godot;
using System;

public partial class Fireball : Area3D
{
	public Vector2 direction;
	const float speed = 5.0f;

	public override void _Ready()
	{
		this.Scale = new Vector3(0.1f, 0.1f, 0.1f);
	}

	public override void _Process(double delta)
	{
		this.Position += new Vector3(direction.X, 0, direction.Y) * speed * (float)delta;
	}

	public void OnBodyEntered(Node3D body)
	{
		if (body is Enemy enemy)
		{
			enemy.Hit();
		}
		else if (body is Player player)
		{
			player.godetteSkin.Hit();
		}
		this.QueueFree();
	}

	public void Setup(float size = 1f)
	{
		Tween tween = this.CreateTween();
		tween.TweenProperty(this, "scale", Vector3.One * size, 0.5f);
	}
}
