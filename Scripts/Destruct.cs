using Godot;
using System;

public partial class Destruct : Node
{
	[Export] float Timer = 0f;
	[Export] Node[] Refs;
	
	public override void _Ready()
	{
		//QueueFree();
	}
	
	public override void _Process(double delta)
	{
		Timer -= (float)delta;
		if (Timer <= 0f)
		{
			foreach (Node Ref in Refs)
			{
				Ref.QueueFree();
			}
		}
	}
}
