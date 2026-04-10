using Godot;
using System;

public partial class Overworld : Level
{
	public void OnCastleDoorBodyEntered(Node3D _body)
	{
		this.SwitchLevel("dungeon");
	}
}
