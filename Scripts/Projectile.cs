using Godot;

public partial class Projectile : Node3D
{
	private float DeltaTime;

	private GameManager GameManager;

	private string Type;
	public Vector3 InternalPosition = Vector3.Zero;
	private Vector3 VisualPosition = Vector3.Zero;
	private Vector3 Velocity = Vector3.Zero;
	private Vector3 Gravity = new Vector3(0f, -48f, 0f);
	private float Lifetime;
	private float LifetimeMax;
	private Vector3 LastPos = Vector3.Zero;
	private int Bounces;
	private float DistanceMax;
	private float DistanceTraveled;
	private bool DoTick = false;
	public bool JustBounced = false;

	public override void _Ready()
	{
		GameManager = (GameManager)GetTree().Root.FindChild("GameManager", true, false);
	}
	
	public override void _Process(double delta)
	{
		DeltaTime = GameManager.GlobalDeltaTime;
		
		if (DoTick) Tick(DeltaTime);
	}
	
	public void Tick(float Delta)
	{
		Lifetime += Delta;
		
		if (Lifetime >= LifetimeMax || DistanceTraveled >= DistanceMax)
		{
			// out of time/distance callback
			QueueFree();
			return;
		}
		
		DistanceTraveled += InternalPosition.DistanceTo(LastPos);
		
		Vector3 Damping = Vector3.Zero;//-Velocity * 0.1f;// * (1f - Velocity.Dot(Vector3.Down)) * 0.1f;
		LastPos = InternalPosition;
		Velocity += (Gravity + Damping) * Delta;
		
		//LookAt(InternalPosition + Velocity.Normalized(), GlobalBasis.Y);
		InternalPosition += Velocity * Delta;
		VisualPosition += Velocity * Delta;
		
		JustBounced = false;
		var Result = GameManager.Raycast(LastPos, InternalPosition);
		if (Result.DidHit)
		{
			GameManager.DestructionManager.New(Result.Position, "DestructionBlob");
			
			if (Bounces <= 0 || Result.Collider is Destructible)
			{
				QueueFree();
				
				return;
			}
			
			InternalPosition = Result.Position + Velocity.Normalized().Bounce(Result.Normal) * 0.01f;// + Velocity.Normalized() * InternalPosition.DistanceTo(Result.Position);
			LastPos = Result.Position;
			Velocity = Velocity.Normalized().Bounce(Result.Normal) * Velocity.Length() * 0.666f;// * 0.333f;
			JustBounced = true;
			
			Bounces--;
		}
		
		GlobalPosition = VisualPosition.Lerp(InternalPosition, Mathf.Clamp(Lifetime * 2f, 0f, 1f));
	}
	
	public void Reset(
		string NewType, 
		float Force, 
		Vector3 Direction, 
		Vector3 NewPosition, 
		Vector3 NewVisualPosition, 
		Vector3 NewVisualRotation, 
		int NewBounces, 
		float NewDistanceMax, 
		float NewLifetimeMax, 
		bool NewDoTick)
	{
		Type = NewType;
		Velocity = Direction * Force;
		InternalPosition = NewPosition;
		LastPos = NewPosition;
		LookAt(NewPosition + Direction, Vector3.Up);
		VisualPosition = NewVisualPosition;
		//Visuals.GlobalRotation = NewVisualRotation;
		Bounces = NewBounces;
		DistanceMax = NewDistanceMax;
		Lifetime = 0f;
		LifetimeMax = NewLifetimeMax;
		DoTick = NewDoTick;
	}
}
