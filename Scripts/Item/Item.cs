using Godot;
using System;

public abstract partial class Item : Node3D
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
	
	[Export] public string Id = "ItemId";
	
	private bool Active = false;
	private ItemAction CurrentAction;
	private ItemAction QueuedAction;
	
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
	}
	
	public override void _Process(double delta)
	{
		DeltaTime = GameManager.GlobalDeltaTime;
		
		Update();
		
		ProgressAction();
	}
	
	protected virtual void Update()
	{
		
	}
	
	private void ProgressAction()
	{
		CurrentAction.Duration -= DeltaTime;
		
		if (CurrentAction.Duration <= 0f && QueuedAction.Name != "") PlayAction(QueuedAction);
	}
	
	public void SetActive(bool NewActive)
	{
		Active = NewActive;
	}
	
	public bool IsActive()
	{
		return Active;
	}
	
	public void QueueAction(ItemAction NewAction)
	{
		QueuedAction = NewAction;
	}
	
	public void PlayAction(ItemAction NewAction)
	{
		CurrentAction = NewAction;
		if (NewAction.Override != null && NewAction.Animation != "") NewAction.Override.Play(NewAction.Animation);
	}
	
	protected virtual void EventCallback(string EventName)
	{
		
	}
}
