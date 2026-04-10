using Godot;
using System;

public partial class Dungeon : Level
{
	public void OnStaticBody3DBodyEntered(Node3D _body)
	{
		this.SwitchLevel("overworld");
	}
}
