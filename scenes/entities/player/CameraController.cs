using Godot;
using System;
public partial class CameraController : Node3D
{
	[Export]
	public float sensitivity = 0.005f;
	[Export]
	public float minLimitX;
	[Export]
	public float maxLimitX;
	public float horizontalAcceleration = 2f;
	public float verticalAcceleration = 2f;

	public override void _Process(double delta)
	{
		var input = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
		RotateFromVector(input * (float)delta * new Vector2(horizontalAcceleration, verticalAcceleration));
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseMotion mouseMotion)
		{
			RotateFromVector(mouseMotion.Relative * sensitivity);
		}
	}

	private void RotateFromVector(Vector2 vector)
	{
		if (vector.Length() == 0) return;
		var rotation = this.Rotation;
		rotation.Y -= vector.X;
		rotation.X -= vector.Y;
		rotation.X = Mathf.Clamp(rotation.X, minLimitX, maxLimitX);
		this.Rotation = rotation;
	}
}
