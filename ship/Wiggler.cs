using Godot;
using System;

public partial class Wiggler : Node3D
{

	private Node3D modelChild;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		modelChild = GetChild<Node3D>(0);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}
}
