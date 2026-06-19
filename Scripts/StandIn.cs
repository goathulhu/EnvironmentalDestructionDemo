using Godot;
using System;

public partial class StandIn : Node3D
{
	[Export] PackedScene Actual;
	
	public void New(Vector3 At, Vector3 Orientation, PackedScene Destruction, float ScaleMult)
	{
		Node3D NewMe = (Node3D)Actual.Instantiate();
		GetParent().AddChild(NewMe, true);
		NewMe.GlobalPosition = GlobalPosition;
		NewMe.GlobalRotation = GlobalRotation;
		NewMe.Scale = Scale;
		
		var Children = NewMe.GetChildren();
		foreach (Node Child in Children)
		{
			if (Child is Destructible) ((Destructible)Child).New(At, Orientation, Destruction, ScaleMult);
		}
		
		QueueFree();
	}
}
