using Godot;

public partial class Hud : Node
{
	private float DeltaTime;

	private GameManager GameManager;
	private Player Player;

	public override void _Ready()
	{
		GameManager = (GameManager)GetTree().Root.FindChild("GameManager", true, false);
		Player = GameManager.Player;
	}

	public override void _Process(double delta)
	{
		DeltaTime = GameManager.GlobalDeltaTime;
	}
}
