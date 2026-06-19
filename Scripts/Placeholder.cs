using Godot;
using System;

public partial class Placeholder : Node3D
{
	private float DeltaTime;
	
	private float SpawnDelayTimer = 0f;
	[Export] private float SpawnDelay = 0.001f;
	
	[Export] PackedScene Actual;
	
	public override void _Process(double delta)
	{
		DeltaTime = (float)delta;
		
		SpawnDelayTimer += DeltaTime;
		if (SpawnDelayTimer >= SpawnDelay) Swap();
	}
	
	private void Swap()
	{
		Node3D NewMe = (Node3D)Actual.Instantiate();
		GetParent().AddChild(NewMe, true);
		NewMe.GlobalPosition = GlobalPosition;
		NewMe.GlobalRotation = GlobalRotation;
		NewMe.Scale = Scale;
		
		QueueFree();
	}
}
