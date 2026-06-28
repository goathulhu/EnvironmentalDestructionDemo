using Godot;
using System;

public partial class Collapsable : Node3D, ICollapsable
{
	protected GameManager GameManager;
	
	[Export] protected Node3D[] StructureProbes;
	[Export] protected Node3D[] VisualProbes;
	[Export] protected string Dest = "";
	
	protected bool PlsDie = false;
	protected bool Died = false;
	
	public override void _Ready()
	{
		GameManager = (GameManager)GetTree().Root.FindChild("GameManager", true, false);
		
		//EarlyCheck();
	}
	
	public override void _Process(double delta)
	{
		if (PlsDie && !Died) CommitDie();
	}
	
	void ICollapsable.Tag()
	{
		if (PlsDie) return;
		
		foreach (Node3D Probe in StructureProbes)
		{
			if (CheckProbe(Probe)) return;
		}
		
		if (Dest != "") GameManager.DestructionManager.QueueDestruction(GlobalPosition, Dest);
		
		PlsDie = true;
	}
	
	private bool CheckProbe(Node3D Probe)
	{
		return (GameManager.Raycast(Probe.GlobalPosition, Probe.GlobalPosition - Probe.GlobalBasis.Z * 0.15f).DidHit && 
			GameManager.Raycast(Probe.GlobalPosition, Probe.GlobalPosition + Probe.GlobalBasis.Z * 0.15f).DidHit);
	}
	
	private async void EarlyCheck()
	{
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		
		foreach (Node3D Probe in StructureProbes)
		{
			if (CheckProbe(Probe)) return;
		}
		
		QueueFree();
	}
	
	protected virtual void CommitDie()
	{
		QueueFree();
		
		Died = true;
	}
}
