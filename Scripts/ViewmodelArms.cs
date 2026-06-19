using Godot;
using System;

public partial class ViewmodelArms : Node3D
{
	[Export] public string Id;
	[Export] public Skeleton3D Skeleton;
	[Export] public Node3D RigTransform;
	[Export] public Node3D InnerTransform;
	[Export] public Node3D OuterTransform;
	[Export] public SkeletonIK3D Ik;
	[Export] public Node3D IkTarget;
	[Export] public bool IsLeft = true;
}
