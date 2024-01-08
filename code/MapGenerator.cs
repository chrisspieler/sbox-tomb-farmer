using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

public sealed class MapGenerator : Component
{
	[Property] public int MaxCellCount { get; set; } = 30;
	[Property] public Model CellModel { get; set; }
	[Property] public float VisualizeDelay { get; set; } = 0.5f;
	[Property] public GameObject PlayerPrefab { get; set; }

	private Dictionary<Vector3Int, GameObject> _cells = new();

	protected override void OnStart()
	{
		VisualizeGeneration();
	}

	public async void VisualizeGeneration()
	{
		var light = Scene.GetAllComponents<DirectionalLight>().FirstOrDefault();
		var camera = Components.Get<CameraComponent>();
		foreach(var cell in GenerateCells() )
		{
			if ( camera.IsValid() )
			{
				camera.Transform.Rotation = Rotation.LookAt( cell.Transform.Position - Transform.Position );
			}
			var renderer = cell.Components.Get<ModelRenderer>();
			renderer.Tint = Color.Blue;
			await Task.DelaySeconds( VisualizeDelay );
			renderer.Tint = Color.White;
		}
		if ( light.IsValid() ) light.Enabled = false;
		if ( camera.IsValid() ) camera.Enabled = false;

		var spawnPos = _cells.Skip(1).First().Value.Transform.Position + new Vector3( 64, -64, 0 );
		var player = SceneUtility.Instantiate( PlayerPrefab, spawnPos );
	}

	public IEnumerable<GameObject> GenerateCells()
	{
		var currentPosition = Vector3Int.Zero;
		_cells[currentPosition] = new GameObject( true, $"Cell {currentPosition}" );
		var direction = new Vector3Int( 1, 0 );

		while( _cells.Count < MaxCellCount )
		{
			if ( !TryGetNextDirection( currentPosition, ref direction ) )
			{
				// TODO: Backtrack to the last cell that has an available direction.
				break;
			}
			currentPosition += direction;
			_cells[currentPosition] = GenerateCell( currentPosition );
			yield return _cells[currentPosition];
		}
	}

	private bool TryGetNextDirection( Vector3Int position, ref Vector3Int direction )
	{
		bool IsBlocked( Vector3Int checkDirection )
		{
			var nextPosition = position + checkDirection;
			return _cells.ContainsKey( nextPosition );
		}

		// If the starting direction isn't blocked and it's not time to pick a random direction, just go forward.
		if ( !IsBlocked( direction ) && Game.Random.Int( 0, 2 ) < 1 )
		{
			return true;
		}
		// Otherwise, pick randomly from any of the available directions.
		var directions = new Vector3Int[3]
		{
			direction,
			direction * Rotation.FromYaw( 90 ),
			direction * Rotation.FromYaw( -90 )
		}.OrderBy( _ => Guid.NewGuid() );
		foreach( var dir in directions )
		{
			if ( !IsBlocked( dir ) )
			{
				direction = dir;
				return true;
			}
		}
		// We've snaked our way in to a dead end.
		return false;
	}

	private GameObject GenerateCell( Vector3Int position )
	{
		var cell = new GameObject( true, $"Cell {position}" );
		var renderer = cell.Components.Create<ModelRenderer>();
		renderer.Model = CellModel;
		var collider = cell.Components.Create<ModelCollider>();
		collider.Model = CellModel;
		cell.Transform.Position = new Vector3Int( position.x * 128, position.y * 128, 0 );
		return cell;
	}
}
