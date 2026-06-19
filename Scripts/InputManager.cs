using Godot;

public partial class InputManager : Node
{
	private float DeltaTime = 0f;

	private GameManager GameManager;

	private InputBind[] Binds;

	private float DoubleTapThreshold = 0.2f;

	public float Sensitivity = 0.05f;
	public float TabletSensitivity = 0.0025f;

	private Vector2 LastMouseInputValue = Vector2.Zero;
	private Vector2 CurrentMouseInputValue = Vector2.Zero;
	private Vector2 DeltaMouseInputValue = Vector2.Zero;
	
	private float LastMouseWheelValue = 0f;
	private float CurrentMouseWheelValue = 0f;
	private float DeltaMouseWheelValue = 0f;

	public override void _Ready()
	{
		SetBinds();

		GameManager = (GameManager)GetTree().Root.FindChild("GameManager", true, false);
		
		Input.MouseMode = Input.MouseModeEnum.Captured;
	}
	
	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseMotion EventMouseMotion)
			CurrentMouseInputValue += EventMouseMotion.Relative;
		
		if (@event is InputEventMouseButton EventMouseButton)
		{
			if (EventMouseButton.ButtonIndex == MouseButton.WheelUp)
				CurrentMouseWheelValue += 1f;
			if (EventMouseButton.ButtonIndex == MouseButton.WheelDown)
				CurrentMouseWheelValue -= 1f;
		}
	}
	
	public override void _Process(double delta)
	{
		DeltaTime = GameManager.GlobalDeltaTime;

		DeltaMouseInputValue = CurrentMouseInputValue - LastMouseInputValue;
		LastMouseInputValue = CurrentMouseInputValue;
		
		DeltaMouseWheelValue = CurrentMouseWheelValue - LastMouseWheelValue;
		LastMouseWheelValue = CurrentMouseWheelValue;

		for (int I = 0; I < Binds.Length; I++)
		{
			Binds[I].IsInput = false;
			for (int J = 0; J < Binds[I].Keys.Length; J++)
			{
				if (Binds[I].Keys[J].KeyBind != Key.None)
					if (Input.IsKeyPressed(Binds[I].Keys[J].KeyBind))
						Binds[I].IsInput = true;
				if (Binds[I].Keys[J].ButtonBind != MouseButton.None)
					if (Input.IsMouseButtonPressed(Binds[I].Keys[J].ButtonBind))
						Binds[I].IsInput = true;
			}

			Binds[I].Down = !Binds[I].Hold && Binds[I].IsInput; // check if the action has just been pressed
			Binds[I].Up = Binds[I].Hold && !Binds[I].IsInput; // check if the action has just stopped
			Binds[I].Hold = Binds[I].IsInput; // check if the action is being pressed
			Binds[I].DoubleTap = Binds[I].DoubleTapTimer > 0f && Binds[I].Down; // check if the action has been double tapped

			if (Binds[I].Down) Binds[I].DoubleTapTimer = DoubleTapThreshold; // starts the doubletap timer
			if (Binds[I].DoubleTap) Binds[I].DoubleTapTimer = 0f; // resets the doubletap timer
			if (Binds[I].DoubleTapTimer > 0f) Binds[I].DoubleTapTimer -= DeltaTime; // updates the doubletap timer
		}
	}

	public bool Down(string Action)
	{
		foreach (InputBind BIND in Binds)
		{
			if (BIND.Action == Action) return BIND.Down;
		}

		return false;
	}

	public bool Hold(string Action)
	{
		foreach (InputBind BIND in Binds)
		{
			if (BIND.Action == Action) return BIND.Hold;
		}

		return false;
	}

	public bool Up(string Action)
	{
		foreach (InputBind BIND in Binds)
		{
			if (BIND.Action == Action) return BIND.Up;
		}

		return false;
	}

	public bool DoubleTap(string Action)
	{
		foreach (InputBind BIND in Binds)
		{
			if (BIND.Action == Action) return BIND.DoubleTap;
		}

		return false;
	}
	
	public float MouseWheelDelta()
	{
		return DeltaMouseWheelValue;
	}

	public Vector2 MouseDelta()
	{
		return DeltaMouseInputValue;
	}

	public Vector2 GetMovementInputVector()
	{
		Vector2 MovementInputVector = Vector2.Zero;

		if (Hold("forward")) MovementInputVector.Y += 1f;
		if (Hold("left")) MovementInputVector.X -= 1f;
		if (Hold("backward")) MovementInputVector.Y -= 1f;
		if (Hold("right")) MovementInputVector.X += 1f;

		return MovementInputVector;
	}

	public bool IsMovementInput()
	{
		return Hold("forward") || Hold("left") || Hold("backward") || Hold("right");
	}

	private void SetBinds()
	{
		Binds = new InputBind[]
		{
			// static
			new InputBind("arrowup", Key.Up),
			new InputBind("arrowdown", Key.Down),
			new InputBind("arrowleft", Key.Left),
			new InputBind("arrowright", Key.Right),
			new InputBind("return", Key.Enter),
			new InputBind("esc", Key.Escape),
			new InputBind("lmb", MouseButton.Left),
			new InputBind("rmb", MouseButton.Right),
			new InputBind("mmb", MouseButton.Middle),
			
			// rebindable
			new InputBind("primary", MouseButton.Left),
			new InputBind("secondary", MouseButton.Right),
			new InputBind("forward", Key.W),
			new InputBind("left", Key.A),
			new InputBind("backward", Key.S),
			new InputBind("right", Key.D),
			new InputBind("jump", Key.Space),
			new InputBind("crouch", Key.C).AddBind(Key.Ctrl).AddBind(MouseButton.Xbutton2),
			new InputBind("push", Key.E),
			new InputBind("tablet", Key.Tab).AddBind(Key.O),
			
			new InputBind("slot1", Key.Key1),
			new InputBind("slot2", Key.Key2),
			new InputBind("slot3", Key.Key3),
			new InputBind("slot4", Key.Key4),
			new InputBind("slot5", Key.Key5),
			new InputBind("slot6", Key.Key6),
			new InputBind("slot7", Key.Key7),
			new InputBind("slot8", Key.Key8),
			new InputBind("slot9", Key.Key9),
			new InputBind("slot10", Key.Key0),
		};
	}
}
