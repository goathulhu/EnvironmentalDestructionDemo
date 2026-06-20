using Godot;
using System;

public partial class ViewmodelOverride : Node3D
{
	private float DeltaTime = 0f;
	
	GameManager GameManager;
	InputManager InputManager;
	Player Player;
	RandomNumberGenerator Rng;
	Viewmodel Viewmodel;
	
	[Export] public string Id;
	[ExportCategory(" ")]
	[Export] public AnimationPlayer Animator;
	[Export] public Skeleton3D Skeleton;
	[Export] public Node3D RigTransform;
	[Export] public Node3D InnTransform;
	[Export] public Node3D MidTransform;
	[Export] public Node3D OutTransform;
	[Export] public Node3D Get;
	
	public override void _Ready()
	{
		GameManager = (GameManager)GetTree().Root.FindChild("GameManager", true, false);
		InputManager = GameManager.InputManager;
		Player = GameManager.Player;
		Rng = GameManager.Rng;
		Viewmodel = GameManager.Viewmodel;
	}
	
	public (Vector3 Position, Vector3 Rotation) GetBoneLocals(string BoneName)
	{
		Transform3D GlobalBoneTransform = Skeleton.GetBoneGlobalPose(Skeleton.FindBone(BoneName));
		
		return (GlobalBoneTransform.Origin, GlobalBoneTransform.Basis.GetRotationQuaternion().GetEuler());
	}
	
	public (Vector3 Position, Vector3 Rotation) GetBoneGlobals(string BoneName)
	{
		Transform3D GlobalBoneTransform = Skeleton.GlobalTransform * Skeleton.GetBoneGlobalPose(Skeleton.FindBone(BoneName));
		
		return (GlobalBoneTransform.Origin, GlobalBoneTransform.Basis.GetRotationQuaternion().GetEuler());
	}
	
	public ViewmodelState GetState()
	{
		ViewmodelState State = new ViewmodelState();
		
		// bones
		Array.Resize(ref State.Left.Bones, Viewmodel.HandBones.Length);
		Array.Resize(ref State.Right.Bones, Viewmodel.HandBones.Length);
		
		for (int I = 0; I < Viewmodel.HandBones.Length; I++)
		{
			int BoneIdLeft = Skeleton.FindBone(Viewmodel.HandBones[I] + ".L");
			int BoneIdRight = Skeleton.FindBone(Viewmodel.HandBones[I] + ".R");
			
			State.Left.Bones[I] = (Skeleton.GetBonePosePosition(BoneIdLeft), Skeleton.GetBonePoseRotation(BoneIdLeft));
			State.Right.Bones[I] = (Skeleton.GetBonePosePosition(BoneIdRight), Skeleton.GetBonePoseRotation(BoneIdRight));
		}
		
		// ik
		State.Left.Ik = (Skeleton.GetBonePosePosition(Skeleton.FindBone("ik_pole.L")), Skeleton.GetBonePosePosition(Skeleton.FindBone("ik_target.L")));
		State.Right.Ik = (Skeleton.GetBonePosePosition(Skeleton.FindBone("ik_pole.R")), Skeleton.GetBonePosePosition(Skeleton.FindBone("ik_target.R")));
		
		// orientation
		State.Left.RigTransform = (RigTransform.Position, RigTransform.Rotation);
		State.Left.InnTransform = (InnTransform.Position, InnTransform.Rotation);
		State.Left.MidTransform = (MidTransform.Position, MidTransform.Rotation);
		State.Left.OutTransform = (OutTransform.Position, OutTransform.Rotation);
		
		State.Right.RigTransform = (RigTransform.Position, RigTransform.Rotation);
		State.Right.InnTransform = (InnTransform.Position, InnTransform.Rotation);
		State.Right.MidTransform = (MidTransform.Position, MidTransform.Rotation);
		State.Right.OutTransform = (OutTransform.Position, OutTransform.Rotation);
		
		// control
		State.Left.Control = Mathf.Clamp(Skeleton.GetBonePosePosition(Skeleton.FindBone("control.L")).Y, 0f, 1f);
		State.Right.Control = Mathf.Clamp(Skeleton.GetBonePosePosition(Skeleton.FindBone("control.R")).Y, 0f, 1f);
		
		return State;
	}
}
