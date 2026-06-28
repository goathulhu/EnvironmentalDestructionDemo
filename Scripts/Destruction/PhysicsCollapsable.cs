using Godot;
using System;

public partial class PhysicsCollapsable : RigidBody3D, ICollapsable
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
		
		EarlyCheck();
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
			if (CheckPoint(Probe)) return;
		}
		
		if (Dest != "") GameManager.DestructionManager.QueueDestruction(GlobalPosition, Dest);
		
		PlsDie = true;
	}
	
	private bool CheckPoint(Node3D Probe)
	{
		return (GameManager.Raycast(Probe.GlobalPosition, Probe.GlobalPosition - Probe.GlobalBasis.Z * 0.15f).DidHit && 
			GameManager.Raycast(Probe.GlobalPosition, Probe.GlobalPosition + Probe.GlobalBasis.Z * 0.15f).DidHit);
	}
	
	private async void EarlyCheck()
	{
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		
		foreach (Node3D Probe in StructureProbes)
		{
			if (CheckPoint(Probe)) return;
		}
		
		QueueFree();
	}
	
	protected virtual void CommitDie()
	{
		SetFreezeEnabled(false);
		SetCollisionLayerValue(1, false);
		SetCollisionLayerValue(3, true);
		SetAxisVelocity(new Vector3(
			GameManager.Rng.RandfRange(-1f, 1f), 
			1f, 
			GameManager.Rng.RandfRange(-1f, 1f)).Normalized() * 2f);
		ApplyTorqueImpulse(new Vector3(
			GameManager.Rng.RandfRange(-1f, 1f), 
			0f, 
			GameManager.Rng.RandfRange(-1f, 1f)) * 0.25f);
		
		Died = true;
	}
}
