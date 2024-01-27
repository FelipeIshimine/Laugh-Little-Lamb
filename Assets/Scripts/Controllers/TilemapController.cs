using System;
using System.Collections.Generic;
using System.Linq;
using Models;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Controllers
{
	public class TilemapController : MonoBehaviour
	{
		private static readonly TileBase[] AllTiles = new TileBase[1024];

		[SerializeField, FoldoutGroup("Component")] private Transform from;
		[SerializeField, FoldoutGroup("Component")] private Transform to;

		[SerializeField, FoldoutGroup("Tiles")] private List<Tile> floorTiles;
		[SerializeField, FoldoutGroup("Tiles")] private Tile playerTile;
		[SerializeField, FoldoutGroup("Tiles")] private Tile enemyTile;
		[SerializeField, FoldoutGroup("Tiles")] private Tile exitTile;
		
		[SerializeField, FoldoutGroup("Tiles")] private Tilemap terrainTilemap;
		[SerializeField, FoldoutGroup("Tiles")] private Tilemap contentTilemap;

		[SerializeField, BoxGroup("Models")] private PathfinderModel pathfinderModel;
		[SerializeField, BoxGroup("Models")] private TilemapModel tilemapModel;
		
		[SerializeField, FoldoutGroup("Debug")] private List<int> path;
		[SerializeField, FoldoutGroup("Debug")] private int debugNeighbours = 0;
		[SerializeField, FoldoutGroup("Debug")] private DebugModes debugModes;

		[Button]
		public TilemapModel ProcessTileMaps()
		{
			terrainTilemap.CompressBounds();
	   
			var bounds = terrainTilemap.cellBounds;
			var count = terrainTilemap.GetTilesBlockNonAlloc(bounds, AllTiles);
			
			List<int> floor = new List<int>(count);
			for (var index = 0; index < count; index++)
			{
				var tile = AllTiles[index];
				if (floorTiles.Exists( x => x == tile))
				{
					floor.Add(index);
				}
			}

			contentTilemap.GetTilesBlockNonAlloc(bounds, AllTiles);
			EntityModel[] entities = new EntityModel[count];
			for (var index = 0; index < count; index++)
			{
				var tile = AllTiles[index];
				if (tile == enemyTile)
				{
					var enemy= new EnemyEntityModel(index);
					entities[index] = enemy;
				}
				else if (tile == playerTile)
				{
					var player = new PlayerEntityModel(index);
					entities[index] = player;
				}
				else if(tile == exitTile)
				{
					var door = new DoorModel(index);
					entities[index] = door;
				}
			}
			
			pathfinderModel = new PathfinderModel(
				tilemapModel.Count, 
				index => tilemapModel.GetNeighbours(index).ToArray(),
				tilemapModel.IsFloor,
				CalculatedDistance);
			
			 tilemapModel = new TilemapModel(bounds, floor, entities);

			 contentTilemap.gameObject.SetActive(false);
			 return tilemapModel;
		}

		private float CalculatedDistance(int fromIndex, int toIndex) =>
			Vector2.Distance(
				tilemapModel.IndexToCoordinate(fromIndex), 
				tilemapModel.IndexToCoordinate(toIndex));

		[Button] public EntityModel GetContent(int index) => tilemapModel.GetContent(index);

		private void OnDrawGizmos()
		{
			if (terrainTilemap)
			{
				Gizmos.color = Color.magenta;
				terrainTilemap.CompressBounds();
				var bounds = terrainTilemap.cellBounds;
				Gizmos.DrawLineStrip(new[]
				{
					terrainTilemap.CellToWorld(new Vector3Int(bounds.min.x,bounds.min.y)),
					terrainTilemap.CellToWorld(new Vector3Int(bounds.min.x,bounds.max.y)),
					terrainTilemap.CellToWorld(new Vector3Int(bounds.max.x,bounds.max.y)),
					terrainTilemap.CellToWorld(new Vector3Int(bounds.max.x,bounds.min.y)),
				}, true);

				var count = terrainTilemap.GetTilesBlockNonAlloc(terrainTilemap.cellBounds, AllTiles);

				if ((debugModes & DebugModes.Coordinates) != 0)
				{
					for (var index = 0; index < count; index++)
					{
						var coordinate = bounds.min + new Vector3Int(index % bounds.size.x, index / bounds.size.x);
						var center = terrainTilemap.GetCellCenterWorld(coordinate);
#if UNITY_EDITOR
						UnityEditor.Handles.Label(center, $"{coordinate.x},{coordinate.y}");
#endif
					}  
				}

				if ((debugModes & DebugModes.Index) != 0)
				{
					for (var index = 0; index < count; index++)
					{
						var coordinate = bounds.min + new Vector3Int(index % bounds.size.x, index / bounds.size.x);
						var center = terrainTilemap.GetCellCenterWorld(coordinate);
#if UNITY_EDITOR
						UnityEditor.Handles.Label(center, $"{index}");
#endif
					}  
				}

				if (tilemapModel != null && (debugModes & DebugModes.WalkableTiles) != 0)
				{
					var color = new Color(0,1,0,.5f);
					Gizmos.color = color;
					foreach (int index in tilemapModel.FloorTiles)
					{
						var coordinate = tilemapModel.IndexToCoordinate(index);
						terrainTilemap.GetCellCenterWorld(coordinate);
						var center = terrainTilemap.GetCellCenterWorld(coordinate);
						Gizmos.DrawCube(center, Vector3.one);
					}
				}

				List<Vector3> points = new List<Vector3>();
				if (tilemapModel != null && pathfinderModel != null && (debugModes & DebugModes.Adjacencies) != 0)
				{
					var color = new Color(1,1,1,1f);
					Gizmos.color = color;

					for (var index = 0; index < pathfinderModel.adjacencies.Length; index++)
					{
						var coordinate = tilemapModel.IndexToCoordinate(index);
						var worldPos = terrainTilemap.GetCellCenterWorld(coordinate);

						var neighbours = pathfinderModel.adjacencies[index];

						foreach (int neighbour in neighbours)
						{
							points.Add(worldPos);
							points.Add(terrainTilemap.GetCellCenterWorld(tilemapModel.IndexToCoordinate(neighbour)));
						}
					}
				}
				Gizmos.DrawLineList(points.ToArray());
			}


			if ((debugModes & DebugModes.Path) != 0 && tilemapModel != null && pathfinderModel!= null)
			{
				var fromIndex = tilemapModel.CoordinateToIndex((Vector2Int)terrainTilemap.WorldToCell(from.position));
				var toIndex = tilemapModel.CoordinateToIndex((Vector2Int)terrainTilemap.WorldToCell(to.position));

				//Debug.Log($"{fromIndex}>{toIndex}");
				if (fromIndex >= 0 && fromIndex < tilemapModel.Count &&
				    toIndex >= 0 && toIndex < tilemapModel.Count)
				{
					bool success = TryFindPath(fromIndex, toIndex, ref path);
					
					//Debug.Log(path.Count);
					Gizmos.color = success ? Color.yellow : Color.red;
					if (path != null && path.Count > 0)
					{
						Gizmos.DrawLineStrip(
							path.ConvertAll(x => terrainTilemap.GetCellCenterWorld(tilemapModel.IndexToCoordinate(x)))
							    .ToArray(),
							false);
					}
				}
			}

			if (debugNeighbours >= 0 && debugNeighbours < pathfinderModel.adjacencies.Length)
			{
				Gizmos.color = Color.cyan;
				var center = terrainTilemap.GetCellCenterWorld(tilemapModel.IndexToCoordinate(debugNeighbours));
				foreach (var neighbour in pathfinderModel.adjacencies[debugNeighbours])
				{
					var other =terrainTilemap.GetCellCenterWorld(tilemapModel.IndexToCoordinate(neighbour));
					Gizmos.DrawLine(center, other);
				}
			}
		}

		public bool TryFindPath(int fromIndex, int toIndex, ref List<int> resultPath)
		{
			return Pathfinding.AStar.TryFindPath(
				fromIndex, 
				toIndex, 
				pathfinderModel.GetNeighbours,
				x => pathfinderModel.distances[toIndex][x],
				tilemapModel.Count,
				ref resultPath, 
				out _);
		}

		[Flags]
		public enum DebugModes
		{
			None = 0,
			Index = 1,
			Coordinates = 2,
			WalkableTiles = 4,
			Adjacencies = 8,
			Path = 16
		}

		public Vector3 GetWorldPosition(int positionIndex) => terrainTilemap.GetCellCenterWorld(tilemapModel.IndexToCoordinate(positionIndex));
		public Vector3 GetWorldPosition(Vector2Int coordinate) => terrainTilemap.GetCellCenterWorld((Vector3Int)coordinate);
	}
}