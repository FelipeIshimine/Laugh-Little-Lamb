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

		[SerializeField] private Transform from;
		[SerializeField] private Transform to;
		
		[SerializeField] private List<Tile> floorTiles;
		[SerializeField] private List<Tile> wallTiles;
   
		[SerializeField] private Tile playerTile;
		[SerializeField] private Tile exitTile;
   
		[SerializeField] private Tilemap tilemap;

		[SerializeField] public TilemapModel tilemapModel;

		[SerializeField]private Pathfinder pathfinder;

		[SerializeField] private List<int> path;
		
		public DebugMode debugMode;

		[SerializeField] private int debugNeighbours = 0;
		
		[Flags]
		public enum DebugMode
		{
			None = 0,
			Index = 1,
			Coordinates = 2,
			WalkableTiles = 4,
			Adjacencies = 8,
			Path = 16
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

				if ((debugMode & DebugMode.Index) != 0)
				{
					for (var index = 0; index < count; index++)
					{
						var coordinate = bounds.min + new Vector3Int(index % bounds.size.x, index / bounds.size.x);
						var center = tilemap.GetCellCenterWorld(coordinate);
#if UNITY_EDITOR
						UnityEditor.Handles.Label(center, $"{index}");
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
				if (tilemapModel != null && pathfinder != null && (debugMode & DebugMode.Adjacencies) != 0)
				{
					var color = new Color(1,1,1,1f);
					Gizmos.color = color;

					for (var index = 0; index < pathfinder.adjacencies.Length; index++)
					{
						var coordinate = tilemapModel.IndexToCoordinate(index);
						var worldPos = tilemap.GetCellCenterWorld(coordinate);

						var neighbours = pathfinder.adjacencies[index];

						foreach (int neighbour in neighbours)
						{
							points.Add(worldPos);
							points.Add(tilemap.GetCellCenterWorld(tilemapModel.IndexToCoordinate(neighbour)));
						}
					}
				}
				Gizmos.DrawLineList(points.ToArray());
			}


			if ((debugMode & DebugMode.Path) != 0 && tilemapModel != null && pathfinder!= null)
			{
				var fromIndex = tilemapModel.CoordinateToIndex((Vector2Int)tilemap.WorldToCell(from.position));
				var toIndex = tilemapModel.CoordinateToIndex((Vector2Int)tilemap.WorldToCell(to.position));

				//Debug.Log($"{fromIndex}>{toIndex}");
				if (fromIndex >= 0 && fromIndex < tilemapModel.Count &&
				    toIndex >= 0 && toIndex < tilemapModel.Count)
				{
					bool success = pathfinder.TryFindPath(fromIndex, toIndex, out path, out _);
					
					//Debug.Log(path.Count);
					Gizmos.color = success ? Color.yellow : Color.red;
					if (path != null && path.Count > 0)
					{
						Gizmos.DrawLineStrip(
							path.ConvertAll(x => tilemap.GetCellCenterWorld(tilemapModel.IndexToCoordinate(x)))
							    .ToArray(),
							false);
					}
				}
			}


			if (debugNeighbours >= 0 && debugNeighbours < pathfinder.adjacencies.Length)
			{
				Gizmos.color = Color.cyan;
				var center = tilemap.GetCellCenterWorld(tilemapModel.IndexToCoordinate(debugNeighbours));
				foreach (var neighbour in pathfinder.adjacencies[debugNeighbours])
				{
					var other =tilemap.GetCellCenterWorld(tilemapModel.IndexToCoordinate(neighbour));
					Gizmos.DrawLine(center, other);
				}

				
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
		public Neighbours[] adjacencies;
		public Distances[] distances;
		private TilemapModel model;
		
		public Pathfinder(TilemapModel model)
		{
			this.model = model;
			adjacencies = new Neighbours[model.Count];

			for (int i = 0; i < adjacencies.Length; i++)
			{
				adjacencies[i] = new Neighbours();
			}
			
			foreach (int index in model.FloorTiles)
			{
				var neighbours = adjacencies[index];
				foreach (var neighbour in model.GetNeighbours(index))
				{
					if (model.IsFloor(neighbour))
					{
						neighbours.Add(neighbour);
					}
				}
			}

			distances = new Distances[model.Count];
			for (int i = 0; i < adjacencies.Length; i++)
			{
				distances[i] = new Distances(adjacencies.Length);
			}
			
			for (var x = 0; x < model.Count; x++)
			{
				for (int y = x+1; y < model.Count; y++)
				{
					var xCoord = model.IndexToCoordinate(x);
					var yCoord = model.IndexToCoordinate(y);
					var distance = Vector2.Distance(xCoord,yCoord);
					distances[y][x] = distances[x][y] = distance;
				}
			}
		}

		public bool TryFindPath(int start, int destination, out List<int> path, out int pathCost)
		{
			Debug.Log($"Find Path {start} => {destination}");
			int[] previous = new int[model.Count];
			int[] bestCost = new int[model.Count];
			float[] distance = distances[destination].values;

			for (int i = 0; i < model.Count; i++)
			{
				bestCost[i] = int.MaxValue;
				previous[i] = -1;
			}
			
			PriorityQueue<int> next = new MinPriorityQueue<int>(model.Count);
			bestCost[start] = 0;
			next.Enqueue(0, start);

			int closestIndex = start;
			
			while (next.Count > 0)
			{
				var (cost, index) = next.Dequeue();

				if (index == destination)
				{
					closestIndex = index;
					//Debug.Log("FOUND");
					break;
				}
				
				var neighbours = adjacencies[index];

				//Debug.Log($"{index} neighbours {neighbours.values.Count}");
				foreach (int neighbour in neighbours)
				{
					var dist = distance[neighbour];
					var nCost = cost + 1 + (int)(dist);
					if (nCost < bestCost[neighbour])
					{
						if (dist < distance[closestIndex])
						{
							closestIndex = neighbour;
						}
						else
						{
							Debug.Log($"[{neighbour}]:{dist}>{closestIndex}:{distance[closestIndex]}");
						}
						
						bestCost[neighbour] = nCost;
						previous[neighbour] = index;
						next.Enqueue(nCost, neighbour);
					}
				}
			}


			path = new List<int>();
			bool success = closestIndex == destination;

			if (closestIndex != -1)
			{
				Debug.Log($"Backtrack Start {closestIndex}");
				pathCost = bestCost[closestIndex];

				while (closestIndex != -1)
				{
					Debug.Log($"BTrack {closestIndex}");
					path.Add(closestIndex);
					closestIndex = previous[closestIndex];
				}
			}
			else
			{
				pathCost = -1;
			}
			return success;
		}
		
		
	
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
				set => values[i] = value;
			}
		}
	}
}