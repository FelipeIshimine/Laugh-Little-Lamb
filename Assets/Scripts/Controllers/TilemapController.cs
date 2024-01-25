using System;
using System.Collections;
using System.Collections.Generic;
using Models;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

namespace Controllers
{
	public class TilemapController : MonoBehaviour, ISerializationCallbackReceiver
	{
		private static readonly TileBase[] AllTiles = new TileBase[1024];
   
		[SerializeField] private List<Tile> floorTiles;
		[SerializeField] private List<Tile> wallTiles;
   
		[SerializeField] private Tile playerTile;
		[SerializeField] private Tile exitTile;
   
		[SerializeField] private Tilemap tilemap;

		[SerializeField] public TilemapModel tilemapModel;

		private Pathfinder pathfinder;
   
		public DebugMode debugMode;

		[Flags]
		public enum DebugMode
		{
			None = 0,
			Coordinates = 1,
			WalkableTiles = 2,
			Pathfinding = 4
		}
   
		[Button]
		public void ScanTilemap()
		{
			tilemap.CompressBounds();

	   
			var count = tilemap.GetTilesBlockNonAlloc(tilemap.cellBounds, AllTiles);
			var bounds = tilemap.cellBounds;

			List<int> floor = new List<int>();
	   
			for (var index = 0; index < count; index++)
			{
				//var coordinate = bounds.min + new Vector3Int(index % bounds.size.x, index / bounds.size.x);
				var tile = AllTiles[index];
				if (floorTiles.Exists( x => x == tile))
				{
					floor.Add(index);
				}
			}

			tilemapModel = new TilemapModel(bounds, floor);

			pathfinder = new Pathfinder(tilemapModel);
		}

		private void OnDrawGizmos()
		{
			if (tilemap)
			{
				Gizmos.color = Color.magenta;
				tilemap.CompressBounds();
				var bounds = tilemap.cellBounds;
				Gizmos.DrawLineStrip(new[]
				{
					tilemap.CellToWorld(new Vector3Int(bounds.min.x,bounds.min.y)),
					tilemap.CellToWorld(new Vector3Int(bounds.min.x,bounds.max.y)),
					tilemap.CellToWorld(new Vector3Int(bounds.max.x,bounds.max.y)),
					tilemap.CellToWorld(new Vector3Int(bounds.max.x,bounds.min.y)),
				}, true);

				var count = tilemap.GetTilesBlockNonAlloc(tilemap.cellBounds, AllTiles);

				if ((debugMode & DebugMode.Coordinates) != 0)
				{
					for (var index = 0; index < count; index++)
					{
						var coordinate = bounds.min + new Vector3Int(index % bounds.size.x, index / bounds.size.x);
						var center = tilemap.GetCellCenterWorld(coordinate);
#if UNITY_EDITOR
						UnityEditor.Handles.Label(center, $"{coordinate.x},{coordinate.y}");
#endif
					}  
				}

				if (tilemapModel != null && (debugMode & DebugMode.WalkableTiles) != 0)
				{
					var color = new Color(0,1,0,.5f);
					Gizmos.color = color;
					foreach (int index in tilemapModel.FloorTiles)
					{
						var coordinate = tilemapModel.IndexToCoordinate(index);
						tilemap.GetCellCenterWorld(coordinate);
						var center = tilemap.GetCellCenterWorld(coordinate);
						Gizmos.DrawCube(center, Vector3.one);
					}
				}

				List<Vector3> points = new List<Vector3>();
				if (tilemapModel != null && pathfinder != null && (debugMode & DebugMode.Pathfinding) != 0)
				{
					var color = new Color(1,1,1,1f);
					Gizmos.color = color;

					for (var index = 0; index < pathfinder.adjacency.Length; index++)
					{
						var coordinate = tilemapModel.IndexToCoordinate(index);
						var worldPos = tilemap.GetCellCenterWorld(coordinate);

						var neighbours = pathfinder.adjacency[index];

						foreach (int neighbour in neighbours)
						{
							points.Add(worldPos);
							points.Add(tilemap.GetCellCenterWorld(tilemapModel.IndexToCoordinate(neighbour)));
						}
					}
				}
				Gizmos.DrawLineList(points.ToArray());
			}
		}

		public void OnBeforeSerialize()
		{
		}

		public void OnAfterDeserialize()
		{
			pathfinder = new Pathfinder(tilemapModel);
		}
	}

	[System.Serializable]
	public class Pathfinder
	{
		public Neighbours[] adjacency;
		public Distances[] distances;
		
		public Pathfinder(TilemapModel model)
		{
			adjacency = new Neighbours[model.Count];

			for (int i = 0; i < adjacency.Length; i++)
			{
				adjacency[i] = new Neighbours();
			}
			
			foreach (int index in model.FloorTiles)
			{
				var neighbours = adjacency[index];
				foreach (var neighbour in model.GetNeighbours(index))
				{
					if (model.IsFloor(neighbour))
					{
						neighbours.Add(neighbour);
					}
				}
			}

			distances = new Distances[model.Count];
			for (int i = 0; i < adjacency.Length; i++)
			{
				distances[i] = new Distances(adjacency.Length);
			}
			
			for (var x = 0; x < model.FloorTiles.Count; x++)
			{
				for (int y = x; y < model.FloorTiles.Count; y++)
				{
					//distances[x][y] = 
				}
			}
		}


		/*public bool TryFindPath(int start, int destination)
		{
		}*/
		
		[System.Serializable]
		public class Neighbours : IEnumerable<int>
		{
			public List<int> values = new List<int>();
			public void Add(int value) => values.Add(value);
			public void Remove(int value) => values.Remove(value);

			public IEnumerator<int> GetEnumerator() => values.GetEnumerator();

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}
		
		[System.Serializable]
		public class Distances
		{
			public float[] values;
			public Distances(int count)
			{
				values = new float[count];
			}

			public float this[int i]
			{
				get => values[i];
				set { throw new NotImplementedException(); }
			}
		}
	}
}