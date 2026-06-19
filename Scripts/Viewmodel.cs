using Godot;
using System;

public partial class Viewmodel : Node3D
{
	private float DeltaTime = 0f;
	
	GameManager GameManager;
	InputManager InputManager;
	Player Player;
	RandomNumberGenerator Rng;
	
	[Export] private ViewmodelArms ArmLeft;
	[Export] private ViewmodelArms ArmRight;
	
	[Export] private ViewmodelOverride ShotgunOverride;
	
	private ViewmodelOverride[] OverrideStack;
	private ViewmodelArms[] ArmsStack;
	
	float LRMouseHang = 0f;
	float UDMouseHang = 0f;
	float LRMoveHang = 0f;
	float FBMoveHang = 0f;
	float WalkAngle = 0f;
	(Vector3 Position, Vector3 Rotation) ViewmodelPivot = (Vector3.Zero, Vector3.Zero);
	float CrouchLerp = 0f;
	float BreatheTimer = 0f;
	float JitterTimer = 0f;
	Vector3 JitterRotTarget = Vector3.Zero;
	Vector3 JitterRot = Vector3.Zero;
	Vector3 JitterPosTarget = Vector3.Zero;
	Vector3 JitterPos = Vector3.Zero;
	float JumpHangRotTarget = 0f;
	float JumpHangRot = 0f;
	float JumpHangPosTarget = 0f;
	float JumpHangPos = 0f;
	
	public readonly string[] HandBones =
	{
		"hand",
		"palm.01",
		"palm.02",
		"palm.03",
		"palm.04",
		"f_index.01",
		"f_index.02",
		"f_index.03",
		"thumb.01",
		"thumb.02",
		"thumb.03",
		"f_middle.01",
		"f_middle.02",
		"f_middle.03",
		"f_ring.01",
		"f_ring.02",
		"f_ring.03",
		"f_pinky.01",
		"f_pinky.02",
		"f_pinky.03",
	};
	
	public override void _Ready()
	{
		GameManager = (GameManager)GetTree().Root.FindChild("GameManager", true, false);
		InputManager = GameManager.InputManager;
		Player = GameManager.Player;
		Rng = GameManager.Rng;
		
		Array.Resize(ref ArmsStack, 2);
		ArmsStack[0] = ArmLeft;
		ArmsStack[1] = ArmRight;
		
		Array.Resize(ref OverrideStack, 2);
		OverrideStack[0] = ShotgunOverride;
		
		Scale = Vector3.One * 2f;
	}
	
	public override void _Process(double delta)
	{
		DeltaTime = GameManager.GlobalDeltaTime;
		
		PseudoItem();
		
		ViewmodelState CurrentState = OverrideStack[0].GetState();
		
		/*for (int I = 0; I < OverrideStack.Length; I++)
		{
			if (I + 1 >= OverrideStack.Length) break;
			
			CurrentState = CurrentState.InterpolateWith(OverrideStack[I + 1].GetState());
		}*/
		
		foreach (ViewmodelArms Arms in ArmsStack)
		{
			if (Arms.IsLeft)
			{
				// bones
				for (int I = 0; I < HandBones.Length; I++)
				{
					int BoneIdLeft = Arms.Skeleton.FindBone(HandBones[I] + ".L");
					
					Arms.Skeleton.SetBonePosePosition(BoneIdLeft, CurrentState.Left.Bones[I].Position);
					Arms.Skeleton.SetBonePoseRotation(BoneIdLeft, CurrentState.Left.Bones[I].Rotation);
				}
				
				// ik
				Arms.IkTarget.Position = CurrentState.Left.Ik.Target;
				Arms.Ik.Magnet = CurrentState.Left.Ik.Pole;
				
				// orientation
				Arms.RigTransform.Position = CurrentState.Left.RigTransform.Position;
				Arms.RigTransform.Rotation = CurrentState.Left.RigTransform.Rotation;
				
				Arms.InnerTransform.Position = CurrentState.Left.InnerTransform.Position;
				Arms.InnerTransform.Rotation = CurrentState.Left.InnerTransform.Rotation;
				
				Arms.OuterTransform.Position = CurrentState.Left.OuterTransform.Position;
				Arms.OuterTransform.Rotation = CurrentState.Left.OuterTransform.Rotation;
			}
			else
			{
				// bones
				for (int I = 0; I < HandBones.Length; I++)
				{
					int BoneIdRight = Arms.Skeleton.FindBone(HandBones[I] + ".R");
					
					Arms.Skeleton.SetBonePosePosition(BoneIdRight, CurrentState.Right.Bones[I].Position);
					Arms.Skeleton.SetBonePoseRotation(BoneIdRight, CurrentState.Right.Bones[I].Rotation);
				}
				
				// ik
				Arms.IkTarget.Position = CurrentState.Right.Ik.Target;
				Arms.Ik.Magnet = CurrentState.Right.Ik.Pole;
				
				// orientation
				Arms.RigTransform.Position = CurrentState.Right.RigTransform.Position;
				Arms.RigTransform.Rotation = CurrentState.Right.RigTransform.Rotation;
				
				Arms.InnerTransform.Position = CurrentState.Right.InnerTransform.Position;
				Arms.InnerTransform.Rotation = CurrentState.Right.InnerTransform.Rotation;
				
				Arms.OuterTransform.Position = CurrentState.Right.OuterTransform.Position;
				Arms.OuterTransform.Rotation = CurrentState.Right.OuterTransform.Rotation;
			}
		}
	}
	
	public void Register(ViewmodelOverride Override)
	{
		foreach (ViewmodelOverride Ride in OverrideStack)
		{
			if (Ride.Id == Override.Id) return;
		}
		Array.Resize(ref OverrideStack, OverrideStack.Length + 1);
		OverrideStack[OverrideStack.Length - 1] = Override;
	}
	
	public void Unregister(ViewmodelOverride Override)
	{
		for (int I = 0; I < OverrideStack.Length; I++)
		{
			if (OverrideStack[I].Id == Override.Id)
			{
				for (int J = I; J < OverrideStack.Length; J++)
				{
					if (J + 1 >= OverrideStack.Length)
					{
						Array.Resize(ref OverrideStack, OverrideStack.Length - 1);
						return;
					}
					
					OverrideStack[J] = OverrideStack[Mathf.Clamp(J + 1, I, OverrideStack.Length - 1)];
				}
				
				Array.Resize(ref OverrideStack, OverrideStack.Length - 1);
				return;
			}
		}
	}
	
	public void Land()
	{
		JumpHangRotTarget = Mathf.DegToRad(-2f);
		JumpHangPosTarget = -0.05f;
	}
	
	private void PseudoItem()
	{
		LRMouseHang += Mathf.DegToRad(Player.DeltaCameraRot.Y);
		LRMouseHang = Mathf.Clamp(Mathf.Lerp(LRMouseHang, 0f, DeltaTime * 24f), -0.5f, 0.5f);
		
		UDMouseHang += Mathf.DegToRad(Player.DeltaCameraRot.X);
		UDMouseHang = Mathf.Clamp(Mathf.Lerp(UDMouseHang, 0f, DeltaTime * 24f), -0.5f, 0.5f);
		
		CrouchLerp = Mathf.Lerp(CrouchLerp, (Player.IsCrouching ? 1f : 0f), DeltaTime * 12f);
		
		LRMoveHang = Mathf.Lerp(LRMoveHang, Player.InputDirection.X, DeltaTime * 16f);
		FBMoveHang = Mathf.Lerp(FBMoveHang, Player.InputDirection.Y, DeltaTime * 16f);
		
		// breathe
		BreatheTimer += DeltaTime * 1.5f;
		JitterTimer -= DeltaTime;
		float JitterStrengthRot = Player.WalkTimer > 0f ? 1.5f : 0.25f;
		float JitterStrengthPos = Player.WalkTimer > 0f ? 0.75f : 0.1f;
		if (JitterTimer <= 0f)
		{
			JitterRotTarget = new Vector3(
				Mathf.DegToRad(Rng.RandfRange(-JitterStrengthRot, JitterStrengthRot)),  
				Mathf.DegToRad(Rng.RandfRange(-JitterStrengthRot, JitterStrengthRot)), 
				Mathf.DegToRad(Rng.RandfRange(-JitterStrengthRot, JitterStrengthRot)));
			JitterPosTarget = new Vector3(
				Mathf.DegToRad(Rng.RandfRange(-JitterStrengthPos, JitterStrengthPos)), 
				Mathf.DegToRad(Rng.RandfRange(-JitterStrengthPos, JitterStrengthPos)), 
				Mathf.DegToRad(Rng.RandfRange(-JitterStrengthPos, JitterStrengthPos)));
			JitterTimer = 0.125f;
		}
		
		float YVelocity = Mathf.Clamp(Player.Velocity.Y - (Player.IsGrounded ? Player.Force.GroundGravity : 0f), -6.66f, 10f);
		
		JitterRot = JitterRot.Lerp(JitterRotTarget, DeltaTime * 8f);
		JitterPos = JitterPos.Lerp(JitterPosTarget, DeltaTime * 8f);
		
		// jump/fall
		JumpHangRotTarget = Mathf.Lerp(JumpHangRotTarget, Mathf.DegToRad(YVelocity * -1.8f), DeltaTime * 16f);
		JumpHangPosTarget = Mathf.Lerp(JumpHangPosTarget, YVelocity * -0.009f, DeltaTime * 32f);
		
		JumpHangRot = Mathf.Lerp(JumpHangRot, JumpHangRotTarget, DeltaTime * 32f);
		JumpHangPos = Mathf.Lerp(JumpHangPos, JumpHangPosTarget, DeltaTime * 32f);
		
		// pivot
		ShotgunOverride.Get.Transform = ShotgunOverride.Skeleton.GetBoneGlobalPose(ShotgunOverride.Skeleton.FindBone("viewmodel_pivot"));
		ViewmodelPivot = (ShotgunOverride.Get.Position, ShotgunOverride.Get.Rotation);
		ShotgunOverride.RigTransform.Position = -ViewmodelPivot.Position;
		ShotgunOverride.InnerTransform.Position = ViewmodelPivot.Position;
		
		if (Player.WalkTimer > 0f)
		{
			WalkAngle = Mathf.Clamp(Mathf.Lerp(WalkAngle, Player.InputDirection.X, DeltaTime * 8f), -1f, 1f);
			
			ShotgunOverride.InnerTransform.Rotation = ShotgunOverride.InnerTransform.Rotation.Lerp(
				new Vector3(
					UDMouseHang * -0.25f + Mathf.DegToRad(Mathf.Sin(Player.WalkTimer * 20f) * -0.5f) + Mathf.Clamp(Mathf.DegToRad(Player.CurrentCameraRot.X * 0.05f), 0f, 90f) + JitterRot.X + JumpHangRot * -0.0333f, 
					Mathf.DegToRad(Mathf.Sin(Player.WalkTimer * 10f) * 0.5f) + JitterRot.Y, 
					Mathf.DegToRad(LRMoveHang * -7.5f) + Mathf.DegToRad(Mathf.Sin(Player.WalkTimer * 10f) * 6.66f) + JitterRot.Z
				), DeltaTime * 24f);
			
			ShotgunOverride.OuterTransform.Position = ShotgunOverride.OuterTransform.Position.Lerp(
				new Vector3(
					LRMoveHang * 0.01f + LRMouseHang * -0.1f + JitterPos.X, 
					Mathf.Lerp(0f, -0.025f, CrouchLerp) + Mathf.Sin(BreatheTimer) * 0.0075f + JitterPos.Y + JumpHangPos + 0.0333f, 
					FBMoveHang * 0.025f + Mathf.Lerp(0f, 0.1f, CrouchLerp) + Mathf.Abs(Player.CurrentCameraRot.X * 0.001f) + JitterPos.Z + 0.0666f
				), DeltaTime * 24f);
			ShotgunOverride.OuterTransform.Rotation = ShotgunOverride.OuterTransform.Rotation.Lerp(
				new Vector3(
					Mathf.DegToRad(Mathf.Sin(Player.WalkTimer * 20f) * -(Mathf.Sin(Player.WalkTimer * 20f) < 0f ? 0f : 2.5f)) + UDMouseHang * 0.25f + Mathf.DegToRad(Player.CurrentCameraRot.X * -0.05f), 
					Mathf.DegToRad(Mathf.Sin(Player.WalkTimer * 10f + 5f) * -3.75f) + LRMouseHang * 0.25f, 
					Mathf.Lerp(0f, Mathf.DegToRad(15f), CrouchLerp) + LRMouseHang * -0.25f
				), DeltaTime * 24f);
		}
		else
		{
			ShotgunOverride.InnerTransform.Rotation = ShotgunOverride.InnerTransform.Rotation.Lerp(
				new Vector3(
					UDMouseHang * -0.25f + Mathf.Clamp(Mathf.DegToRad(Player.CurrentCameraRot.X * 0.05f), 0f, 90f) + JitterRot.X + JumpHangRot * -0.0333f, 
					JitterRot.Y, 
					Mathf.DegToRad(LRMoveHang * -7.5f) + JitterRot.Z
				), DeltaTime * 24f);
			
			ShotgunOverride.OuterTransform.Position = ShotgunOverride.OuterTransform.Position.Lerp(
				new Vector3(
					LRMoveHang * 0.01f + LRMouseHang * -0.1f + JitterPos.X, 
					Mathf.Lerp(0f, -0.025f, CrouchLerp) + Mathf.Sin(BreatheTimer) * 0.0075f + JitterPos.Y + JumpHangPos + 0.0333f, 
					FBMoveHang * 0.025f + Mathf.Lerp(0f, 0.1f, CrouchLerp) + Mathf.Abs(Player.CurrentCameraRot.X * 0.001f) + JitterPos.Z + 0.0666f
				), DeltaTime * 24f);
			ShotgunOverride.OuterTransform.Rotation = ShotgunOverride.OuterTransform.Rotation.Lerp(
				new Vector3(
					UDMouseHang * 0.25f + Mathf.DegToRad(Player.CurrentCameraRot.X * -0.05f), 
					LRMouseHang * 0.125f, 
					Mathf.DegToRad(LRMoveHang * -5f) + Mathf.Lerp(0f, Mathf.DegToRad(15f), CrouchLerp) + LRMouseHang * -0.25f
				), DeltaTime * 24f);
		}
	}
}
