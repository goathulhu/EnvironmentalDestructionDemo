using Godot;
using System;

public partial class test1 : AnimationPlayer
{
	GameManager GameManager;
	
	public override void _Ready()
	{
		GameManager = (GameManager)GetTree().Root.FindChild("GameManager", true, false);
	}
	
	public override void _Process(double delta)
	{
		if (GameManager.InputManager.Down("lmb"))
		{
			Stop();
			Play("Shoot");
		}
	}
}
