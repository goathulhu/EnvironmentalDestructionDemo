using Godot;

public partial class GameManager : Node3D
{
	private float DeltaTime = 0f;
	
	public float GlobalDeltaTime = 0f;
	public float GlobalTimeScale = 1f;

	// references
	public InputManager InputManager;
	public Console Console;
	public Player Player;
	public ProjectileManager ProjectileManager;
	public DestructionManager DestructionManager;
	public Hud Hud;
	public Viewmodel Viewmodel;

	public RandomNumberGenerator Rng = new RandomNumberGenerator();

	private bool ConsoleOpen;

	[ExportCategory("Fps Display")]
	[Export] private RichTextLabel FpsLabel;
	private float FpsTimer;
	private float CumFps;
	private int HowManyFps;

	[ExportCategory("Map Generation")]
	[Export] public PackedScene DummyMap;
	[Export] public PackedScene TerrainChunkPrefab;
	[Export] public PackedScene FoliageChunkPrefab;
	[Export] public PackedScene PropChunkPrefab;
	[Export] public PackedScene PoiChunkPrefab;
	public Map CurrentMap;

	[ExportCategory("PhysicsCasts")]
	[Export] RayCast3D Ray;
	[Export] ShapeCast3D Sphere;

	// noise
	private FastNoiseLite NoisePerlin = new FastNoiseLite();
	private FastNoiseLite NoiseWall = new FastNoiseLite();

	public override void _Ready()
	{
		InputManager = (InputManager)GetTree().Root.FindChild("InputManager", true, false);
		Console = (Console)GetTree().Root.FindChild("Console", true, false);
		Player = (Player)GetTree().Root.FindChild("Player", true, false);
		ProjectileManager = (ProjectileManager)GetTree().Root.FindChild("ProjectileManager", true, false);
		DestructionManager = (DestructionManager)GetTree().Root.FindChild("DestructionManager", true, false);
		Hud = (Hud)GetTree().Root.FindChild("Hud", true, false);
		Viewmodel = (Viewmodel)GetTree().Root.FindChild("Viewmodel", true, false);
		
		NoisePerlin.SetNoiseType(FastNoiseLite.NoiseTypeEnum.Perlin);
		NoisePerlin.SetFractalType(FastNoiseLite.FractalTypeEnum.None);
		
		NoiseWall.SetNoiseType(FastNoiseLite.NoiseTypeEnum.Simplex);
		NoiseWall.SetFractalType(FastNoiseLite.FractalTypeEnum.None);
		NoiseWall.SetSeed(696969);
		NoiseWall.SetFrequency(1024f);

		//var NewMap = DummyMap.Instantiate();
		//AddChild(NewMap);
		//CurrentMap = (Map)NewMap;
		//CurrentMap.Init(this, 696969);
	}

	public override void _Process(double delta)
	{
		GlobalDeltaTime = (float)delta * GlobalTimeScale;
		DeltaTime = GlobalDeltaTime;

		FpsTimer += DeltaTime;
		CumFps += 1f / DeltaTime;
		HowManyFps++;
		if (FpsTimer >= 0.2f)
		{
			FpsLabel.Text = "[color=666666]" + (int)(CumFps / HowManyFps) + "fps " + DeltaTime * 1000f + "ms ";
			CumFps = 0f;
			HowManyFps = 0;
			FpsTimer = 0f;
		}
	}
	
	public void OpenConsole()
	{
		ConsoleOpen = true;
	}

	public void CloseConsole()
	{
		ConsoleOpen = false;
	}

	public bool IsConsoleOpen()
	{
		return ConsoleOpen;
	}
	
	public (bool DidHit, Vector3 Position, Vector3 Normal, Node Collider) Raycast(Vector3 From, Vector3 To)
	{
		Ray.GlobalPosition = From;
		Ray.TargetPosition = To - From;
		
		Ray.ForceRaycastUpdate();
		
		return (Ray.IsColliding(), Ray.GetCollisionPoint(), Ray.GetCollisionNormal(), (Node)Ray.GetCollider());
	}
	
	public (bool DidHit, Node[] Colliders) Spherecast(Vector3 At, float Radius)
	{
		Sphere.GlobalPosition = At;
		((SphereShape3D)Sphere.Shape).Radius = Radius;
		
		Sphere.ForceShapecastUpdate();
		
		int Count = Sphere.GetCollisionCount();
		Node[] Colliders = new Node[Count];
		for (int i = 0; i < Count; i++)
		{
			Colliders[i] = (Node)Sphere.GetCollider(i);
		}
		
		return (Sphere.IsColliding(), Colliders);
	}

	public float SamplePerlinNoise(int Seed, Vector2 Position, float Scale, float Height)
	{
		NoisePerlin.SetSeed(Seed);

		return ((NoisePerlin.GetNoise2D(Position.X * Scale, Position.Y * Scale) + 1f) * 0.5f) * Height;
	}
	
	public int SampleWallNoise(Vector3 Position)
	{
		//if ((Mathf.Round((Position.X + 0.4f) * 1.25f + 0.1f) + Mathf.Round((Position.Y + 0.4f) * 1.25f + 0.1f)) % 2 == 0) return 0;
		//else return 1;
		//return Mathf.FloorToInt(NoiseWall.GetNoise2D(Mathf.Round((Position.X + 0.4f) * 2.5f), Mathf.Round((Position.Z + 0.4f) * 2.5f)) * 1.5f + 1.5f);
		//if (NoiseWall.GetNoise2D(Position.X, Position.Z) > 0f) return 0;
		//else return 1;
		return Mathf.FloorToInt(NoiseWall.GetNoise2D(Mathf.Round(Position.X * 8f), Mathf.Round(Position.Z * 8f)) * 1.5f + 1.5f);
	}
	
	public float SampleRawWallNoise(Vector3 Position)
	{
		return NoiseWall.GetNoise2D(Mathf.Round((Position.X + 0.4f) * 2.5f), Mathf.Round((Position.Z + 0.4f) * 2.5f)) * 1.5f + 1.5f;
	}
}
