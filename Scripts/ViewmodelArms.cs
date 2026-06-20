using Godot;
using System;

public partial class ViewmodelArms : Node3D
{
	[Export] public string Id;
	[Export] public bool IsLeft = true;
	[ExportCategory(" ")]
	[Export] public Skeleton3D Skeleton;
	[Export] public Node3D RigTransform;
	[Export] public Node3D InnTransform;
	[Export] public Node3D MidTransform;
	[Export] public Node3D OutTransform;
	[ExportCategory(" ")]
	[Export] public SkeletonIK3D Ik;
	[Export] public Node3D IkTarget;
}
