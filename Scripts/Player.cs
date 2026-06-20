using Godot;
using System;

public partial class Player : Node3D
{
	private float DeltaTime;

	private GameManager GameManager;
	private InputManager InputManager;
	private Console Console;
	private RandomNumberGenerator Rng;
	private ProjectileManager ProjectileManager;
	private DestructionManager DestructionManager;
	private Hud Hud;
	private Viewmodel Viewmodel;

	public Vector3 LastPos;
	public Vector3 DeltaPos;
	public float MoveSpeed;
	
	public (float Current, float Target, float Idle, float Walk, float Crouch) Speed;
	public (float Jump, float AirGravity, float GroundGravity) Force;
	public (float Y, float YTarget, float YDefaultTarget, Vector3 XZ, Vector3 XZTarget, Vector3 XZActual) Velocity;
	public (float Y, float YTarget, Vector3 XZ, Vector3 XZTarget) LandHang;
	
	public Vector2 InputDirection = Vector2.Zero;
	public Vector3 GlobalInputDirection = Vector3.Zero;
	
	public Vector2 CurrentCameraRot = Vector2.Zero;
	public Vector2 LastCameraRot = Vector2.Zero;
	public Vector2 DeltaCameraRot = Vector2.Zero;
	
	[Export] public CharacterBody3D Controller;
	
	[ExportCategory("Body")]
	[Export] public Node3D BodyRotY;
	[Export] public CollisionShape3D Collider;
	public CapsuleShape3D ColliderShape;
	[Export] public Node3D CameraTargetPos;
	
	[ExportCategory("View")]
	[Export] public Node3D PlayerView;
	[Export] public Node3D CameraMotionGlobal;
	[Export] public Node3D CamRotY;
	[Export] public Node3D CamRotX;
	[Export] public Node3D CameraMotionLocal;
	[Export] public Camera3D PlayerCamera;
	[Export] public Node3D CameraOrientation;
	[Export] public Node3D SpreadR;
	[Export] public Node3D Director;
	
	// flags
	public bool CanMove;
	public bool CanLook;
	public bool CanGadget;
	
	public bool IsMenu;
	public bool IsConsole;
	
	public bool IsGrounded;
	public bool IsLanded;
	public bool IsWalking;
	public bool IsCrouching;
	public bool IsWalkingInput;
	
	public float StepTimer = 0f;

	private float CoyoteTime = 0.125f;
	private float CoyoteTimer;
	
	// recoil
	Vector2 LastRecoil = Vector2.Zero;
	Vector2 TargetRecoil = Vector2.Zero;
	Vector2 CurrentRecoil = Vector2.Zero;
	Vector2 DeltaRecoil = Vector2.Zero;

	public float WalkTimer = 0f;
	
	private float InputTimeoutTimer = 0f;
	
	[ExportCategory("Dev")]
	[Export] RichTextLabel DebugLabel;
	
	public override void _Ready()
	{
		GameManager = (GameManager)GetTree().Root.FindChild("GameManager", true, false);
		InputManager = GameManager.InputManager;
		Console = GameManager.Console;
		Rng = GameManager.Rng;
		ProjectileManager = GameManager.ProjectileManager;
		DestructionManager = GameManager.DestructionManager;
		Hud = GameManager.Hud;
		Viewmodel = GameManager.Viewmodel;
		
		ColliderShape = (CapsuleShape3D)Collider.Shape;
		
		Speed.Current = 0f;
		Speed.Target = 0f;
		Speed.Idle = 0f;
		Speed.Walk = 7f;
		Speed.Crouch = 3.5f;
		
		Force.Jump = 9f;
		Force.AirGravity = -39f;
		Force.GroundGravity = 0f;
		
		Velocity.Y = 0f;
		Velocity.YTarget = 0f;
		Velocity.XZ = Vector3.Zero;
		Velocity.XZTarget = Vector3.Zero;
		Velocity.XZActual = Vector3.Zero;
		
		LandHang.Y = 0f;
		LandHang.YTarget = 0f;
		LandHang.XZ = Vector3.Zero;
		LandHang.XZTarget = Vector3.Zero;
	}
	
	public override void _Process(double delta)
	{
		DeltaTime = GameManager.GlobalDeltaTime;
		
		Flags();
		
		Movement();
		
		Body();
		
		Camera();
		
		Actions();
	}
	
	private void Flags()
	{
		if (InputTimeoutTimer > 0f) InputTimeoutTimer -= DeltaTime;
		
		IsConsole = GameManager.IsConsoleOpen();
		
		CanMove = !IsConsole && !IsMenu && InputTimeoutTimer <= 0f;
		
		CanLook = !IsConsole && !IsMenu;
		
		IsGrounded = Controller.IsOnFloor() || CoyoteTimer > 0f;
		
		IsWalkingInput = InputManager.IsMovementInput();
		
		IsWalking = CanMove && IsGrounded && IsWalkingInput;
		
		IsCrouching = CanMove && IsGrounded && InputManager.Hold("crouch");
	}
	
	private void Movement()
	{
		InputDirection = Vector2.Zero;
		GlobalInputDirection = Vector3.Zero;
		if (CanMove && InputManager.IsMovementInput())
		{
			InputDirection = InputManager.GetMovementInputVector().Normalized();
			GlobalInputDirection = ((BodyRotY.GlobalBasis.X * InputDirection.X) + (-BodyRotY.GlobalBasis.Z * InputDirection.Y));
		}

		DeltaPos = Controller.GlobalPosition - LastPos;
		MoveSpeed = Controller.GlobalPosition.DistanceTo(LastPos);
		LastPos = Controller.GlobalPosition;
		
		if (IsWalking) WalkTimer += DeltaTime;
		else WalkTimer = 0f;
		
		if (Velocity.XZ.IsZeroApprox()) Velocity.XZActual = Vector3.Zero;
		else Velocity.XZActual = Velocity.XZ.Normalized();
		
		if (CanMove && IsGrounded) Step();
		
		if (Controller.IsOnFloor())
		{
			if (!IsLanded)
			{
				if (CoyoteTimer <= 0f) Land();
				
				IsLanded = true;
			}
			
			CoyoteTimer = CoyoteTime;
		}
		else
		{
			CoyoteTimer -= DeltaTime;
			IsLanded = false;
		}
		
		// gravity
		if (Controller.IsOnFloor()) Velocity.Y = Force.GroundGravity;
		else Velocity.Y += Force.AirGravity * DeltaTime;
		
		if (CanMove && IsGrounded)
		{
			if (InputManager.Down("jump")) Jump();
		}
		
		// set current speed
		if (IsWalkingInput)
		{
			if (IsCrouching) Speed.Current = Speed.Crouch;
			else Speed.Current = Speed.Walk;
		}
		else Speed.Current = Speed.Idle;
		
		Speed.Current *= GameManager.GlobalTimeScale;
		
		Velocity.XZTarget = (InputDirection.Y * -BodyRotY.GlobalBasis.Z + InputDirection.X * BodyRotY.GlobalBasis.X) * Speed.Current;
		
		if (IsGrounded) Velocity.XZ = Velocity.XZ.Lerp(Velocity.XZTarget, DeltaTime * 12f);
		else Velocity.XZ = Velocity.XZ.Lerp(Velocity.XZTarget, DeltaTime * 1f);
		
		Controller.Velocity = Velocity.XZ + (Vector3.Up * Velocity.Y);
		Controller.MoveAndSlide();
		
		
		//DebugLabel.SetText("R " + GameManager.SampleRawWallNoise(Controller.GlobalPosition).ToString() + "\nW " + GameManager.SampleWallNoise(Controller.GlobalPosition).ToString() + "\nX " + Controller.GlobalPosition.Y.ToString() + "\nY " + Controller.GlobalPosition.X.ToString() + "\nZ " + Controller.GlobalPosition.Z.ToString());
	}

	private void Body()
	{
		// player height adjustment for crouching
		float StanceLerpTime = DeltaTime * 16f;
		float ColliderHeight = 1.95f;
		
		if (IsCrouching) ColliderHeight = 1.333f;
		
		ColliderShape.Height = Mathf.Clamp(Mathf.Lerp(ColliderShape.Height, ColliderHeight, StanceLerpTime), 0.9f, 1.95f);
		Collider.Position = Collider.Position.Lerp(new Vector3(0f, ColliderHeight * 0.5f, 0f), StanceLerpTime);
		CameraTargetPos.Position = CameraTargetPos.Position.Lerp(new Vector3(0f, ColliderHeight - 0.25f, 0f), StanceLerpTime);
		
		BodyRotY.Rotation = new Vector3(0f, CamRotY.Rotation.Y, 0f);
		
		while (GameManager.Raycast(Controller.GlobalPosition + Vector3.Up * (ColliderShape.Height - 0.1f), Controller.GlobalPosition + Vector3.Up * 0.1f).DidHit)
			Controller.GlobalPosition += Vector3.Up * 0.1f;
	}
	
	private void Camera()
	{
		// update rotation
		if (CanLook)
		{
			CamRotY.Rotation = new Vector3(0f, CamRotY.Rotation.Y - Mathf.DegToRad(InputManager.MouseDelta().X * InputManager.Sensitivity), 0f);
			CamRotX.Rotation = new Vector3(CamRotX.Rotation.X - Mathf.DegToRad(InputManager.MouseDelta().Y * InputManager.Sensitivity), 0f, 0f);
			CamRotX.Rotation = new Vector3(Mathf.Clamp(CamRotX.Rotation.X, Mathf.DegToRad(-80f), Mathf.DegToRad(80f)), 0f, 0f);
		}
		
		// recoil
		DeltaRecoil = LastRecoil - CurrentRecoil;
		LastRecoil = CurrentRecoil;
		
		CurrentRecoil = CurrentRecoil.Lerp(TargetRecoil, DeltaTime * 16f);
		
		CamRotX.Rotation += new Vector3(Mathf.DegToRad(-DeltaRecoil.Y), 0f, 0f);
		CamRotY.Rotation += new Vector3(0f, Mathf.DegToRad(DeltaRecoil.X), 0f);
		
		// delta
		CurrentCameraRot = new Vector2(Mathf.RadToDeg(CamRotX.Rotation.X), Mathf.RadToDeg(CamRotY.Rotation.Y));
		DeltaCameraRot = LastCameraRot - CurrentCameraRot;
		LastCameraRot = CurrentCameraRot;
		
		// land hang
		LandHang.YTarget = Mathf.Lerp(LandHang.YTarget, 0f, DeltaTime * 16f);
		LandHang.Y = Mathf.Lerp(LandHang.Y, Mathf.Clamp(LandHang.YTarget, -1.75f, 0f), DeltaTime * 32f);
		
		LandHang.XZTarget = LandHang.XZTarget.Lerp(Vector3.Zero, DeltaTime * 16f);
		LandHang.XZ = LandHang.XZ.Lerp(LandHang.XZTarget, DeltaTime * 32f);
		
		// update position
		PlayerView.GlobalPosition = PlayerView.GlobalPosition.Lerp(
			CameraTargetPos.GlobalPosition + Vector3.Up * LandHang.Y + LandHang.XZ
			, DeltaTime * 16f);
		
		// global camera movement
		CameraMotionGlobal.Rotation = CameraMotionGlobal.Rotation.Lerp(
			new Vector3(
				Mathf.DegToRad(Velocity.XZ.Z * 0.333f + LandHang.XZ.Z * -8f), 
				0f, 
				Mathf.DegToRad(Velocity.XZ.X * -0.333f + LandHang.XZ.X * 8f)), DeltaTime * 8f);
		
		// local camera movement
		float YVelocity = Mathf.Clamp(Velocity.Y - (IsGrounded ? Force.GroundGravity : 0f), -10f, 10f) * 1.25f;
	}
	
	private void Jump()
	{
		// jump animation stuff
		
		IsLanded = false;
		Velocity.Y = Force.Jump;
		CoyoteTimer = 0f;
	}
	
	private void Step()
	{
		int StepStages = 10;
		float StepHeightPerStage = 0.05f;
		float StepDistance = 0.425f;
		float LongStepDistance = 0.475f;
		int LastHit = 0;
		
		for (int I = 0; I < StepStages; I++)
		{
			var Result = GameManager.Raycast(
				Controller.GlobalPosition + Vector3.Up * (I * StepHeightPerStage), 
				Controller.GlobalPosition + (Velocity.XZActual * StepDistance) + Vector3.Up * (I * StepHeightPerStage));
			
			if (Result.DidHit) LastHit = I;
		}
		
		if (LastHit > 0)
		{
			var Result = GameManager.Raycast(
				Controller.GlobalPosition + Vector3.Up * ((LastHit + 1) * StepHeightPerStage), 
				Controller.GlobalPosition + (Velocity.XZActual * LongStepDistance) + Vector3.Up * ((LastHit + 1) * StepHeightPerStage));
			
			if (Result.DidHit) return;
			
			//if (CoyoteTimer <= 0f) Viewmodel.Land();
			if (GameManager.Raycast(Controller.GlobalPosition, Controller.GlobalPosition + Vector3.Up * -0.25f).DidHit)
			{
				CoyoteTimer = CoyoteTime;
				Velocity.Y = Velocity.YTarget = Force.GroundGravity;
			}
			Controller.GlobalPosition += Vector3.Up * (LastHit * StepHeightPerStage);
		}
	}
	
	private void InputTimeout(float NewTime)
	{
		if (InputTimeoutTimer <= 0f) InputTimeoutTimer = NewTime;
		else InputTimeoutTimer += NewTime;
	}
	
	private void Actions()
	{
		if (InputManager.Down("lmb"))
		{
			float PalletSpread = 10f;
			float PalletRange = 16f;
			int PalletCount = 1;
			
			(Vector3 At, string Type, float ScaleMult)[] Hits = new (Vector3 At, string Type, float ScaleMult)[PalletCount];
			int Landed = 0;
			
			for (int I = PalletCount; I > 0; I--)
			{
				SpreadR.Rotation = new Vector3(0f, 0f, Mathf.DegToRad(Rng.RandfRange(0f, 360f)));
				Director.Rotation = new Vector3(Mathf.DegToRad(Rng.RandfRange(0f, PalletSpread)), 0f, 0f);
				
				var Result = I == PalletCount ? GameManager.Raycast(CameraOrientation.GlobalPosition, CameraOrientation.GlobalPosition - CameraOrientation.GlobalBasis.Z * PalletRange) : 
					GameManager.Raycast(CameraOrientation.GlobalPosition, CameraOrientation.GlobalPosition - Director.GlobalBasis.Z * PalletRange);
				if (Result.DidHit)
				{
					//Hits[Landed] = (Result.Position, "DestructionBlob", (PalletRange - Result.Position.DistanceTo(CameraOrientation.GlobalPosition)) / PalletRange);
					Hits[Landed] = (Result.Position, "DestructionBlob", 1f);
					Landed += 1;
				}
			}
			
			if (Landed > 0)
			{
				Array.Resize(ref Hits, Landed);
				DestructionManager.New(Hits);
			}
		}
	}
	
	private void Land()
	{
		Viewmodel.Land();
		
		LandHang.YTarget = Mathf.Abs(Velocity.Y * Velocity.Y) * -0.01f;
	}
	
	void RecoilImpulse(Vector2 Impulse)
	{
		TargetRecoil += Impulse;
	}
	
	// --------------------------------------------------------------------------------------------------------------------- //
	// -------------------------------------------------- utility section -------------------------------------------------- //
	// --------------------------------------------------------------------------------------------------------------------- //
	
	public void TeleportTo(Vector3 NewPosition)
	{
		LastPos = NewPosition;
		Controller.GlobalPosition = NewPosition;
		PlayerView.GlobalPosition = CameraTargetPos.GlobalPosition;
	}
}
