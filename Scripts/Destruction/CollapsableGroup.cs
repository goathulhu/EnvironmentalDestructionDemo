using Godot;
using System;

public partial class CollapsableGroup : Node3D
{
	[Export] public Collapsable[] Collapsables;
	[Export] public PhysicsCollapsable[] PhysicsCollapsables;
}
