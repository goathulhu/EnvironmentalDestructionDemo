using Godot;

public partial class ProjectileManager : Node3D
{
	[Export] private PackedScene ProjectilePrefab;
	[Export] private PackedScene[] ProjectileVisuals;

	public void New(
		string NewType, 
		float NewForce, 
		Vector3 NewDirection, 
		Vector3 NewPosition, 
		Vector3 NewVisualPosition, 
		//Vector3 NewVisualRotation, 
		int NewBounces, 
		float NewDistanceMax, 
		float NewLifetimeMax, 
		bool NewDoTick)
	{
		Projectile NewProjectileRef = (Projectile)ProjectilePrefab.Instantiate();
		AddChild(NewProjectileRef);
		NewProjectileRef.Reset(
			NewType, 
			NewForce, 
			NewDirection, 
			NewPosition, 
			NewVisualPosition, 
			Vector3.Zero,//NewVisualRotation, 
			NewBounces, 
			NewDistanceMax, 
			NewLifetimeMax, 
			NewDoTick);
	}
}
