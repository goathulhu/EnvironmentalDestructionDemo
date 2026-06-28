using Godot;
using System;

public partial class DestructibleWall : Destructible
{
	enum Type
	{
		Top,
		Switch,
		Bottom
	}
	
	[Export] Type MyType;
	[Export] Node3D BeamsPlaceholderMesh;
	[Export] Node3D[] BeamSpots;
	
	[Export] PackedScene[] BeamPrefabs;
	
	private bool Beamed = false;
	
	public override void New(Vector3 At, Vector3 Orientation, PackedScene Destruction, float ScaleMult)
	{
		Node3D NewDest = (Node3D)Destruction.Instantiate();
		AddChild(NewDest);
		NewDest.GlobalPosition = At;
		NewDest.GlobalRotation = Orientation;
		NewDest.Scale *= DestScalMult * ScaleMult;
		
		BakeTime = GameManager.Rng.RandfRange(0.5f, 1f);
		
		if (Beamed) return;
		
		foreach (Node3D Spot in BeamSpots)
		{
			Node3D NewBeam = (Node3D)BeamPrefabs[GameManager.SampleWallNoise(Spot.GlobalPosition)].Instantiate();
			GetParent().AddChild(NewBeam);
			NewBeam.GlobalPosition = Spot.GlobalPosition;
			NewBeam.GlobalRotation = Spot.GlobalRotation;
			
			if (NewBeam is CollapsableGroup)
				GameManager.DestructionManager.QueueCollapsable(((CollapsableGroup)NewBeam).PhysicsCollapsables);
			
			Spot.QueueFree();
		}
		
		BeamsPlaceholderMesh.QueueFree();
		
		Beamed = true;
	}
}
