using Godot;
using System;

public partial class Destructible : CsgMesh3D
{
	protected GameManager GameManager;
	
	[Export] public float DestScalMult = 1f;
	protected float BakeTime = 0f;
	
	public override void _Ready()
	{
		GameManager = (GameManager)GetTree().Root.FindChild("GameManager", true, false);
	}
	
	public override void _Process(double delta)
	{
		if (BakeTime <= 0f) return;
		
		BakeTime -= (float)delta;
		
		if (BakeTime <= 0f)
		{
			Mesh = BakeStaticMesh();
			
			var MyChildren = GetChildren();
			foreach (Node Child in MyChildren)
			{
				Child.QueueFree();
			}
		}
	}
	
	public virtual void New(Vector3 At, Vector3 Orientation, PackedScene Destruction, float ScaleMult)
	{
		Node3D NewDest = (Node3D)Destruction.Instantiate();
		AddChild(NewDest);
		NewDest.GlobalPosition = At;
		NewDest.GlobalRotation = Orientation;
		NewDest.Scale *= DestScalMult * ScaleMult;
		
		BakeTime = GameManager.Rng.RandfRange(0.5f, 1f);
	}
}
