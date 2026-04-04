using Godot;
using System;

public partial class Heart : TextureRect
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void ChangeAlpha(float target)
	{
		Tween tween = this.CreateTween();
		tween.TweenMethod(new Callable(this, nameof(_ChangeAlpha)), 1.0 - target, target, 0.4f);
	}

	private void _ChangeAlpha(float value)
	{
		ShaderMaterial material = (ShaderMaterial)this.Material;
		material.SetShaderParameter("alpha", value);
		material.SetShaderParameter("progress", 1.0 - value);
	}
}
