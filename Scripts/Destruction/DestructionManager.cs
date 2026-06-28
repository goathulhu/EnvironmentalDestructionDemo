using Godot;
using System;

public partial class DestructionManager : Node
{
	GameManager GameManager;
	Console Console;
	ProjectileManager ProjectileManager;
	
	[Export] PackedScene[] Destructions;
	
	private ICollapsable[] CollapsableQueue = new ICollapsable[64];
	//private Collapsable[] CollapsableQueue = new Collapsable[64];
	private bool HasQueuedCollapsables = false;
	private int CurrentCollapsables = 0;
	private PhysicsCollapsable[] PhysicsCollapsableQueue = new PhysicsCollapsable[64];
	private bool HasQueuedPhysicsCollapsables = false;
	private int CurrentPhysicsCollapsables = 0;
	private (Vector3 At, string Type, float ScaleMult)[] DestructibleQueue = new (Vector3 At, string Type, float ScaleMult)[64];
	private bool HasQueuedDestructions = false;
	private int CurrentDestructions = 0;
	
	private float CheckTimer = 0f;
	
	public override void _Ready()
	{
		GameManager = (GameManager)GetTree().Root.FindChild("GameManager", true, false);
		Console = (Console)GetTree().Root.FindChild("Console", true, false);
		ProjectileManager = (ProjectileManager)GetTree().Root.FindChild("ProjectileManager", true, false);
	}
	
	public override void _Process(double delta)
	{
		if (HasQueuedCollapsables) CycleCollapsables();
		if (HasQueuedDestructions) CycleDestructions();
	}
	
	// --------------------------------------------------
	// destructions
	// --------------------------------------------------
	public void QueueDestruction((Vector3 At, string Type, float ScaleMult)[] Dests)
	{
		CurrentDestructions += Dests.Length;
		for (int I = 0; I < CurrentDestructions; I++)
		{
			DestructibleQueue[CurrentDestructions - 1 - I] = Dests[I];
		}
		
		HasQueuedDestructions = true;
	}
	
	public void QueueDestruction(Vector3 At, string Type, float ScaleMult = 1f)
	{
		CurrentDestructions += 1;
		DestructibleQueue[CurrentDestructions - 1] = (At, Type, ScaleMult);
		
		HasQueuedDestructions = true;
	}
	
	private void CycleDestructions()
	{
		for (int I = 0; I < CurrentDestructions; I++)
		{
			(Vector3 At, string Type, float ScaleMult) QueDest = DestructibleQueue[I];
			(bool Success, PackedScene Scene) Dest = GetDestruction(QueDest.Type);
			if (Dest.Success)
			{
				Vector3 Orientation = new Vector3(
					Mathf.DegToRad(GameManager.Rng.RandfRange(0f, 360f)), 
					Mathf.DegToRad(GameManager.Rng.RandfRange(0f, 360f)), 
					Mathf.DegToRad(GameManager.Rng.RandfRange(0f, 360f)));
			
				Node[] Hits = GrabDest(QueDest.At, 2f);
				foreach (Node Hit in Hits)
				{
					if (Hit is Destructible) ((Destructible)Hit).New(QueDest.At, Orientation, Dest.Scene, QueDest.ScaleMult);
					else if (Hit is ICollapsable) QueueCollapsable((ICollapsable)Hit);
				}
			}
		}
		
		CurrentDestructions = 0;
		HasQueuedDestructions = false;
	}
	
	// --------------------------------------------------
	// collapsables
	// --------------------------------------------------
	public void QueueCollapsable(ICollapsable[] Colls)
	{
		foreach (ICollapsable Coll in Colls)
		{
			bool NotFound = true;
			
			for (int I = 0; I < CurrentCollapsables; I++)
			{
				if (CollapsableQueue[I] == Coll) NotFound = false;
			}
			
			if (NotFound)
			{
				CurrentCollapsables += 1;
				CollapsableQueue[CurrentCollapsables - 1] = Coll;
			}
		}
		
		HasQueuedCollapsables = true;
	}
	
	private void QueueCollapsable(ICollapsable Coll)
	{
		for (int I = 0; I < CurrentCollapsables; I++)
		{
			if (CollapsableQueue[I] == Coll) return;
		}
		
		CurrentCollapsables += 1;
		CollapsableQueue[CurrentCollapsables - 1] = Coll;
		
		HasQueuedCollapsables = true;
	}
	
	private void CycleCollapsables()
	{
		for (int I = 0; I < CurrentCollapsables; I++)
		{
			CollapsableQueue[I].Tag();
		}
		
		CurrentCollapsables = 0;
		HasQueuedCollapsables = false;
	}
	
	private (bool Success, PackedScene Scene) GetDestruction(string Type)
	{
		foreach (PackedScene Dest in Destructions)
			if (Dest.GetState().GetNodeName(0) == Type) return (true, Dest);
		
		return (false, null);
	}
	
	private Node[] GrabDest(Vector3 At, float SphereSize)
	{
		Node[] Hits = new Node[0];
		
		var Result = GameManager.Spherecast(At, SphereSize);
		if (Result.DidHit)
		{
			foreach (Node Hit in Result.Colliders)
			{
				Array.Resize(ref Hits, Hits.Length + 1);
				Hits[Hits.Length - 1] = Hit;
			}
		}
		
		return Hits;
	}
}
