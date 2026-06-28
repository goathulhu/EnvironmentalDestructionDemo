using System;
using Godot;

public abstract partial class Map : Node3D
{
	protected float DeltaTime;
	
	protected GameManager GameManager;
	protected Console Console;
	
	private RandomNumberGenerator Rng;
	public RandomNumberGenerator FoliageRng = new RandomNumberGenerator();
	public RandomNumberGenerator PropRng = new RandomNumberGenerator();
	public RandomNumberGenerator PoiRng = new RandomNumberGenerator();

	SurfaceTool SurfaceTool = new SurfaceTool();
	
	protected bool DoMapGen;
	protected bool StepSetup;
	protected int GenStep;
	protected int GenX;
	protected int GenY;
	protected int TotalSteps;
	protected float GenTimer;
	
	protected (int X, int Y) ChunkPerMap;
	protected int TilePerChunk;
	protected float TileScale;

	protected int Seed;
	
	public override void _Ready()
	{
		
	}

	public override void _Process(double delta)
	{
		DeltaTime = GameManager.GlobalDeltaTime;

		if (DoMapGen)
		{
			GenTimer += DeltaTime;
			TotalSteps++;
			Progress();
			NextChunk();
		}
	}

	protected virtual void Progress()
	{
		
	}

	public void Init(GameManager NewGameManager, int NewSeed)
	{
		GameManager = NewGameManager;
		Console = GameManager.Console;
		Rng = GameManager.Rng;
		
		Seed = NewSeed;
		
		FoliageRng.Seed = (uint)Seed;
		PropRng.Seed = (uint)Seed;
		PoiRng.Seed = (uint)Seed;
		
		DoMapGen = true;
		
		Setup();
	}

	public virtual void Setup()
	{
		
	}

	public virtual float GetTerrainHeightAt(string Type, Vector2 Sp)
	{
		return 0f;
	}
	
	protected virtual bool GetDoFoliageAt(string Type, Vector2 Sp, float Angle, float Height)
	{
		return true;
	}
	
	protected void NextChunk()
	{
		GenX++;
		if (GenX > ChunkPerMap.X - 1)
		{
			GenX = 0;
			GenY++;
		}

		if (GenY > ChunkPerMap.Y - 1)
		{
			NextStep();
		}
	}

	protected void NextStep()
	{
		GenX = 0;
		GenY = 0;
		GenStep++;
		StepSetup = false;
	}

	protected void GenStop()
	{
		NextStep();
		
		Console.Log(Name, "Generation Done");
		Console.Log(Name, TotalSteps + " Steps In " + GenTime() + "s");
		
		DoMapGen = false;
	}

	protected float GenTime()
	{
		return (((int)(GenTimer * 100)) / 100f);
	}

	protected void NewTerrainChunk(
		int InX, 
		int InY, 
		string Type, 
		int Tiles, 
		float TilesSize, 
		bool Inverted, 
		bool DoCollision, 
		bool DoPoi, 
		float MinPoiHeight, 
		float MaxPoiHeight, 
		Material TerrainMaterial, 
		float DrawDistance)
	{
		var ChunkOrigin = new Vector3(InX * Tiles * TilesSize, 0f, InY * Tiles * TilesSize);
		var ChunkName = "TerrainChunk_" + Type + "_X" + (InX < 10 ? "0" + InX : InX) + "_Y" + (InY < 10 ? "0" + InY : InY);
		
		var NewTerrainMesh = GenerateTerrainMesh(Type, Tiles, TilesSize, ChunkOrigin, Inverted);
		
		// determine if flat enough for poi spawnage
		var CanPoi = DoPoi && Math.Abs(NewTerrainMesh.Highest - NewTerrainMesh.Lowest) < 0.01f && NewTerrainMesh.Lowest > MinPoiHeight && NewTerrainMesh.Highest < MaxPoiHeight;
		
		var NewTerrainChunk = (MeshInstance3D)GameManager.TerrainChunkPrefab.Instantiate();
		AddChild(NewTerrainChunk);
		NewTerrainChunk.Position = ChunkOrigin;
		NewTerrainChunk.Name = ChunkName;

		NewTerrainChunk.Mesh = NewTerrainMesh.Mesh;
		NewTerrainChunk.MaterialOverride = TerrainMaterial;
		NewTerrainChunk.VisibilityRangeEnd = DrawDistance;
		if (DoCollision) NewTerrainChunk.CreateTrimeshCollision();

		//Chunks[InX, InY].Terrain = NewTerrainChunk;
		//Chunks[InX, InY].CanPoi = CanPoi;
	}

	private (ArrayMesh Mesh, float Highest, float Lowest) GenerateTerrainMesh(
		string Type, 
		int Tiles, 
		float TilesSize, 
		Vector3 Origin, 
		bool Inverted)
	{
		Vector3[] TerrainVertices;
		Vector3[] TerrainTriangles;

		Tiles += 1;

		Vector3 Highest = new Vector3(0f, -9999f, 0f);
		Vector3 Lowest = new Vector3(0f, 9999f, 0f);
		
		// setup
		TerrainVertices = new Vector3[Tiles * Tiles];
		TerrainTriangles = new Vector3[TerrainVertices.Length * 6];
		for (int I = 0; I < TerrainTriangles.Length; I++)
		{
			TerrainTriangles[I] = Vector3.Zero;
		}

		// generate heightmap
		for (int Y = 0; Y < Tiles; Y++)
		{
			for (int X = 0; X < Tiles; X++)
			{
				float Sx = Origin.X + X * TilesSize;
				float Sy = Origin.Z + Y * TilesSize;
				Vector2 Sp = new Vector2(Sx, Sy);

				Vector3 VertexPos = new Vector3(
					X * TilesSize,
					GetTerrainHeightAt(Type, Sp),
					Y * TilesSize);

				if (VertexPos.Y > Highest.Y) Highest = VertexPos;
				if (VertexPos.Y < Lowest.Y) Lowest = VertexPos;

				TerrainVertices[Y * Tiles + X] = VertexPos;
			}
		}

		// construct mesh
		int TriPointer = 0;
		for (int Y = 0; Y < Tiles - 1; Y++)
		{
			for (int X = 0; X < Tiles - 1; X++)
			{
				// terrain
				if (Mathf.Abs(TerrainVertices[X + Y * Tiles].Y - TerrainVertices[X + Y * Tiles + Tiles + 1].Y) < Mathf.Abs(TerrainVertices[X + Y * Tiles + 1].Y - TerrainVertices[X + Y * Tiles + Tiles].Y))
				{ 
					if (Inverted)
					{ 
						TerrainTriangles[TriPointer + 5] = TerrainVertices[X + Y * Tiles]; 
						TerrainTriangles[TriPointer + 4] = TerrainVertices[X + Y * Tiles + 1]; 
						TerrainTriangles[TriPointer + 3] = TerrainVertices[X + Y * Tiles + Tiles + 1];
						
						TerrainTriangles[TriPointer + 2] = TerrainVertices[X + Y * Tiles]; 
						TerrainTriangles[TriPointer + 1] = TerrainVertices[X + Y * Tiles + Tiles + 1]; 
						TerrainTriangles[TriPointer] = TerrainVertices[X + Y * Tiles + Tiles]; 
					}
					else 
					{ 
						TerrainTriangles[TriPointer] = TerrainVertices[X + Y * Tiles]; 
						TerrainTriangles[TriPointer + 1] = TerrainVertices[X + Y * Tiles + 1]; 
						TerrainTriangles[TriPointer + 2] = TerrainVertices[X + Y * Tiles + Tiles + 1];
						
						TerrainTriangles[TriPointer + 3] = TerrainVertices[X + Y * Tiles]; 
						TerrainTriangles[TriPointer + 4] = TerrainVertices[X + Y * Tiles + Tiles + 1]; 
						TerrainTriangles[TriPointer + 5] = TerrainVertices[X + Y * Tiles + Tiles]; 
					} 
				}
				else
				{
					if (Inverted)
					{
						TerrainTriangles[TriPointer + 5] = TerrainVertices[X + Y * Tiles + Tiles];
						TerrainTriangles[TriPointer + 4] = TerrainVertices[X + Y * Tiles];
						TerrainTriangles[TriPointer + 3] = TerrainVertices[X + Y * Tiles + 1];

						TerrainTriangles[TriPointer + 2] = TerrainVertices[X + Y * Tiles + 1];
						TerrainTriangles[TriPointer + 1] = TerrainVertices[X + Y * Tiles + Tiles + 1];
						TerrainTriangles[TriPointer] = TerrainVertices[X + Y * Tiles + Tiles];
					}
					else
					{
						TerrainTriangles[TriPointer] = TerrainVertices[X + Y * Tiles + Tiles];
						TerrainTriangles[TriPointer + 1] = TerrainVertices[X + Y * Tiles];
						TerrainTriangles[TriPointer + 2] = TerrainVertices[X + Y * Tiles + 1];

						TerrainTriangles[TriPointer + 3] = TerrainVertices[X + Y * Tiles + 1];
						TerrainTriangles[TriPointer + 4] = TerrainVertices[X + Y * Tiles + Tiles + 1];
						TerrainTriangles[TriPointer + 5] = TerrainVertices[X + Y * Tiles + Tiles];
					}
				}

				TriPointer += 6;
			}
		}

		// finalizing
		var TerrainMesh = new ArrayMesh();
		var Arrays = new Godot.Collections.Array();
		Arrays.Resize((int)Mesh.ArrayType.Max);
		Arrays[(int)Mesh.ArrayType.Vertex] = TerrainTriangles;

		TerrainMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, Arrays);

		SurfaceTool.CreateFrom(TerrainMesh, 0);
		SurfaceTool.GenerateNormals();

		TerrainMesh = SurfaceTool.Commit();

		// output
		return (TerrainMesh, Highest.Y, Lowest.Y);
	}

	protected void NewFoliageChunk(
		int InX, 
		int InY, 
		string Type, 
		int Tiles, 
		float TilesSize, 
		(float Min, float Max) RandomWidth, 
		(float Min, float Max) RandomHeight, 
		Mesh FoliageMesh, 
		Material FoliageMaterial, 
		float DrawDistance)
	{
		var ChunkOrigin = new Vector3(InX * Tiles * TilesSize, 0f, InY * Tiles * TilesSize);
		var ChunkName = "FoliageChunk_" + Type + "_X" + (InX < 10 ? "0" + InX : InX) + "_Y" + (InY < 10 ? "0" + InY : InY);
		
		var NewFoliageMesh = GenerateFoliageMesh(Type, Tiles, TilesSize, ChunkOrigin, RandomWidth, RandomHeight, FoliageMesh);
		
		if (NewFoliageMesh.IsEmpty) return;
		
		var NewFoliageChunk = (MultiMeshInstance3D)GameManager.FoliageChunkPrefab.Instantiate();
		AddChild(NewFoliageChunk);
		NewFoliageChunk.Position = ChunkOrigin;
		NewFoliageChunk.Name = ChunkName;

		NewFoliageChunk.Multimesh = NewFoliageMesh.Mesh;
		NewFoliageChunk.MaterialOverride = FoliageMaterial;
		NewFoliageChunk.VisibilityRangeEnd = DrawDistance;
	}
	
	private (MultiMesh Mesh, bool IsEmpty) GenerateFoliageMesh(
		string Type, 
		int Tiles, 
		float TilesSize, 
		Vector3 Origin, 
		(float Min, float Max) RandomWidth, 
		(float Min, float Max) RandomHeight, 
		Mesh FoliageMesh)
	{
		(int FoliageId, Transform3D FoliageTransform)[] FoliageInstances = [];

		for (int Y = 0; Y < Tiles; Y++)
		{
			for (int X = 0; X < Tiles; X++)
			{
				// sample positions
				float Sx = Origin.X + X * TilesSize + FoliageRng.RandfRange(0, TilesSize);
				float Sz = Origin.Z + Y * TilesSize + FoliageRng.RandfRange(0, TilesSize);
				
				var Result = GameManager.Raycast(new Vector3(Sx, 256f, Sz), new Vector3(Sx, -256f, Sz));
				
				if (Result.DidHit)
				{
					if (GetDoFoliageAt(Type, new Vector2(Result.Position.X, Result.Position.Z), Result.Normal.AngleTo(Vector3.Up), Result.Position.Y))
					{
						Transform3D FoliageTransform = new Transform3D(
						RotateBasisToUpDirection(Basis.Identity, Result.Normal), Result.Position - Origin);
						
						FoliageTransform.Basis.X *= FoliageRng.RandfRange(RandomWidth.Min, RandomWidth.Max);
						FoliageTransform.Basis.Y *= FoliageRng.RandfRange(RandomHeight.Min, RandomHeight.Max);
						FoliageTransform.Basis.Z *= FoliageRng.RandfRange(RandomWidth.Min, RandomWidth.Max);
						
						FoliageTransform = FoliageTransform.RotatedLocal(Basis.Y, Mathf.DegToRad(FoliageRng.Randf() * 360f));
						
						Array.Resize(ref FoliageInstances, FoliageInstances.Length + 1);
						FoliageInstances[FoliageInstances.Length - 1] = (FoliageInstances.Length - 1, FoliageTransform);
					}
				}
			}
		}
		
		MultiMesh FoliageMultiMesh = new MultiMesh();

		if (FoliageInstances.Length > 0)
		{
			FoliageMultiMesh.SetTransformFormat(MultiMesh.TransformFormatEnum.Transform3D);
			FoliageMultiMesh.InstanceCount = FoliageInstances.Length;
			FoliageMultiMesh.Mesh = FoliageMesh;

			foreach ((int FoliageId, Transform3D FoliageTransform) Instance in FoliageInstances)
				FoliageMultiMesh.SetInstanceTransform(Instance.FoliageId, Instance.FoliageTransform);

			return (FoliageMultiMesh, false);
		}
		else
			return (FoliageMultiMesh, true);
	}

	protected void NewPropChunk(
		int InX, 
		int InY, 
		string Type, 
		int Tiles, 
		float TilesSize, 
		float MinHeight, 
		float MaxHeight, 
		float MinAngle, 
		float MaxAngle, 
		PackedScene[] Props)
	{
		var ChunkOrigin = new Vector3(InX * Tiles * TilesSize, 0f, InY * Tiles * TilesSize);
		var ChunkName = "PropChunk_" + Type + "_X" + (InX < 10 ? "0" + InX : InX) + "_Y" + (InY < 10 ? "0" + InY : InY);
		
		var NewPropChunk = (Node3D)GameManager.PropChunkPrefab.Instantiate();
		AddChild(NewPropChunk);
		NewPropChunk.Position = ChunkOrigin;
		NewPropChunk.Name = ChunkName;
		
		bool HasPropped = false;

		for (int Y = 0; Y < Tiles; Y++)
		{
			for (int X = 0; X < Tiles; X++)
			{
				// sample positions
				float Sx = ChunkOrigin.X + X * TilesSize + PropRng.RandfRange(0, TilesSize);
				float Sz = ChunkOrigin.Z + Y * TilesSize + PropRng.RandfRange(0, TilesSize);

				var Result = GameManager.Raycast(new Vector3(Sx, 256f, Sz), new Vector3(Sx, -256f, Sz));

				if (Result.DidHit)
				{
					if (Result.Position != Vector3.Zero && 
						Result.Normal.AngleTo(Vector3.Up) >= MaxAngle && 
						Result.Normal.AngleTo(Vector3.Up) <= MinAngle && 
						Result.Position.Y >= MaxHeight && 
						Result.Position.Y <= MinHeight) break;

					Node3D NewProp = (Node3D)Props[PropRng.RandiRange(0, Props.Length - 1)].Instantiate();
					NewPropChunk.AddChild(NewProp);

					Transform3D PropTransform = new Transform3D(RotateBasisToUpDirection(Basis.Identity, Result.Normal), Result.Position - ChunkOrigin);
					NewProp.Transform = PropTransform.RotatedLocal(Basis.Y, Mathf.DegToRad(PropRng.Randf() * 360f));
					
					HasPropped = true;
				}
			}
		}

		if (!HasPropped) NewPropChunk.QueueFree();
	}

	protected void NewPoiChunk()
	{
		
	}
	
	private Basis RotateBasisToUpDirection(Basis CurrentBasis, Vector3 TargetUpDirection)
	{
		return (new Basis(new Quaternion(CurrentBasis.Y, TargetUpDirection.Normalized())) * CurrentBasis).Orthonormalized();
	}
}
