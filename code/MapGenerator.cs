using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

public sealed class MapGenerator : Component
{
	[Property] public int MaxCellCount { get; set; } = 30;
	[Property] public Model CellModel { get; set; }
	[Property] public Model Cell1Way { get; set; }
	[Property] public Model Cell2Way { get; set; }
	[Property] public Model CellCorner { get; set; }
	[Property] public Model Cell3Way { get; set; }
	[Property] public Model Cell4Way { get; set; }
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
			_ = FlashTint( cell, VisualizeDelay * 4f, Color.Blue );
			await Task.DelaySeconds( VisualizeDelay );
		}
		foreach( var cell in AutotileCells() )
		{
			if ( camera.IsValid() )
			{
				camera.Transform.Rotation = Rotation.LookAt( cell.Transform.Position - Transform.Position );
			}
			_ = FlashTint( cell, VisualizeDelay * 4f, Color.Cyan );
			await Task.DelaySeconds( VisualizeDelay );
		}

		if ( light.IsValid() ) light.Enabled = false;
		if ( camera.IsValid() ) camera.Enabled = false;

		var spawnPos = _cells.Skip(1).First().Value.Transform.Position + new Vector3( 0, 0, 0 );
		var player = SceneUtility.Instantiate( PlayerPrefab, spawnPos );
	}

	public IEnumerable<GameObject> GenerateCells()
	{
		var currentPosition = Vector3Int.Zero;
		_cells[currentPosition] = GenerateCell( currentPosition );
		var direction = new Vector3Int( 1, 0 );

		while( _cells.Count < MaxCellCount )
		{
			if ( !TryGetNextDirection( currentPosition, ref direction ) )
			{
				// We've reached a dead end, so keep going until we find an open direction.
				currentPosition += direction;
				continue;
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
		cell.Transform.Position = new Vector3( position.x * 128, position.y * 128, 0 );
		var model = new GameObject( true, $"Model {position}" );
		model.Parent = cell;
		model.Transform.LocalPosition = new Vector3( -64, 64 );
		var renderer = model.Components.Create<ModelRenderer>();
		renderer.Model = CellModel;
		var collider = model.Components.Create<ModelCollider>();
		collider.Model = CellModel;
		return cell;
	}

	[Flags]
	private enum Neighbors : byte
	{
		None = 0,
		Forward = 1,
		Right = 2,
		Backward = 4,
		Left = 8
	}

	private IEnumerable<GameObject> AutotileCells()
	{
		foreach( KeyValuePair<Vector3Int, GameObject> kvp in _cells )
		{
			var neighbors = GetNeighbors( kvp.Key );
			// Handle dead ends.
			if ( BitOperations.PopCount( (byte)neighbors ) == 1 )
			{
				SetModel( kvp.Value, Cell1Way );
				// TODO: Find out why this sometimes isn't lining up.
				kvp.Value.Transform.Rotation = neighbors switch
				{
					Neighbors.Forward => Rotation.FromYaw( 0 ),
					Neighbors.Right => Rotation.FromYaw( 270 ),
					Neighbors.Backward => Rotation.FromYaw( 180 ),
					Neighbors.Left => Rotation.FromYaw( 90 ),
					_ => Rotation.FromYaw( 0 )
				};
				yield return kvp.Value;
				continue;
			}
			if ( BitOperations.PopCount( (byte)neighbors ) == 2 )
			{
				switch ( neighbors )
				{
					case Neighbors.Forward | Neighbors.Backward:
						SetModel( kvp.Value, Cell2Way );
						break;
					case Neighbors.Left | Neighbors.Right:
						SetModel( kvp.Value, Cell2Way );
						kvp.Value.Transform.Rotation *= Rotation.FromYaw( 90 );
						break;
					default:
						SetModel( kvp.Value, CellCorner );
						kvp.Value.Transform.Rotation = neighbors switch
						{
							Neighbors.Forward | Neighbors.Right => Rotation.FromYaw( 0 ),
							Neighbors.Right | Neighbors.Backward => Rotation.FromYaw( 270 ),
							Neighbors.Backward | Neighbors.Left => Rotation.FromYaw( 180 ),
							Neighbors.Left | Neighbors.Forward => Rotation.FromYaw( 90 ),
							_ => Rotation.FromYaw( 0 )
						};
						break;
				}
				yield return kvp.Value;
				continue;
			}
			if ( BitOperations.PopCount( (byte)neighbors ) == 3 )
			{
				SetModel( kvp.Value, Cell3Way );
				kvp.Value.Transform.Rotation = neighbors switch
				{
					Neighbors.Forward | Neighbors.Right | Neighbors.Backward => Rotation.FromYaw( 0 ),
					Neighbors.Forward | Neighbors.Right | Neighbors.Left => Rotation.FromYaw( 90 ),
					Neighbors.Forward | Neighbors.Backward | Neighbors.Left => Rotation.FromYaw( 180 ),
					Neighbors.Right | Neighbors.Backward | Neighbors.Left => Rotation.FromYaw( -90 ),
					_ => Rotation.FromYaw( 0 )
				};
				yield return kvp.Value;
				continue;
			}
			if ( BitOperations.PopCount( (byte)neighbors ) == 4 )
			{
				SetModel( kvp.Value, Cell4Way );
				yield return kvp.Value;
				continue;
			}
			SetModel( kvp.Value, CellModel );
			yield return kvp.Value;
		}

		void SetModel( GameObject cell, Model model )
		{
			var renderer = cell.Components.Get<ModelRenderer>( FindMode.EverythingInSelfAndDescendants );
			renderer.Model = model;
			var collider = cell.Components.Get<ModelCollider>( FindMode.EverythingInSelfAndDescendants );
			collider.Model = model;
		}

		Neighbors GetNeighbors( Vector3Int cell )
		{
			Neighbors neighbors = 0;
			if ( _cells.ContainsKey( cell + new Vector3Int( 1, 0 ) ) ) neighbors |= Neighbors.Forward;
			if ( _cells.ContainsKey( cell + new Vector3Int( 0, -1 ) ) ) neighbors |= Neighbors.Right;
			if ( _cells.ContainsKey( cell + new Vector3Int( -1, 0 ) ) ) neighbors |= Neighbors.Backward;
			if ( _cells.ContainsKey( cell + new Vector3Int( 0, 1 ) ) ) neighbors |= Neighbors.Left;
			return neighbors;
		}
	}

	private async Task FlashTint( GameObject go, float duration, Color tintColor ) 
	{
		var renderer = go.Components.Get<ModelRenderer>( FindMode.EverythingInSelfAndDescendants );
		if ( !renderer.IsValid() ) return;


		renderer.Tint = tintColor;
		TimeUntil undoTint = duration;
		while ( !undoTint )
		{
			renderer.Tint = Color.Lerp( tintColor, Color.White, undoTint.Fraction );
			await Task.Frame();
		}

		if ( !renderer.IsValid() ) return;
		renderer.Tint = Color.White;
	}
}
