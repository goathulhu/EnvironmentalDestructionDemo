using Godot;
using System;

public partial class DestructibleNonCsg : Node3D
{
	private float DeltaTime;
	
	private GameManager GameManager;
	
	[Export] float InstaKillDistance = 0f;
	
	public override void _Ready()
	{
		GameManager = (GameManager)GetTree().Root.FindChild("GameManager", true, false);
	}
	
	public void New(Vector3 At)
	{
		if (InstaKillDistance > 0f && GlobalPosition.DistanceTo(At) <= InstaKillDistance) QueueFree();
	}
}
