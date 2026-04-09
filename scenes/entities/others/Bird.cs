using Godot;
using System;

public partial class Bird : PathFollow3D
{
	float speed = 40;
	public override void _Ready()
	{
	}

	public override void _Process(double delta)
	{
		this.Progress += speed * (float)delta;
	}
}
