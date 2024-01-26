using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Models
{
	[System.Serializable]
	public class TilemapTerrainModel
	{
		[SerializeField] private List<int> floorTiles;
		[SerializeField] private TileContent[] content;
		[SerializeField] private BoundsInt bounds;

		public IReadOnlyList<int> FloorTiles => floorTiles;

		public TilemapTerrainModel(BoundsInt bounds, List<int> floorTiles, TileContent[] content) 
		{
			this.bounds = bounds;
			this.floorTiles = floorTiles;
			this.content = content;
		}

		public bool IsFloor(int neighbour) => floorTiles.Contains(neighbour);
		public int Count => bounds.size.x * bounds.size.y;
		[Button] public int CoordinateToIndex(Vector2Int coordinate) => coordinate.x - bounds.x + (coordinate.y - bounds.y) * bounds.size.x;
		[Button] public Vector2Int IndexToCoordinate(int index) => new Vector2Int(index % bounds.size.x, index / bounds.size.x) + new Vector2Int(bounds.min.x,bounds.min.y);
		public IEnumerable<int> GetNeighbours(int index)
		{
			var center = IndexToCoordinate(index);
			var neighbour = center + Vector2Int.left;
			if (bounds.Contains((Vector3Int)neighbour))
			{
				yield return CoordinateToIndex(neighbour);
			}

			neighbour = center + Vector2Int.right;	
			if (bounds.Contains((Vector3Int)neighbour))
			{
				yield return CoordinateToIndex(center + Vector2Int.right);
			}

			neighbour = center + Vector2Int.up;	
			if (bounds.Contains((Vector3Int)neighbour))
			{
				yield return CoordinateToIndex(center + Vector2Int.up);
			}

			neighbour = center + Vector2Int.down;
			if (bounds.Contains((Vector3Int)neighbour))
			{
				yield return CoordinateToIndex(center + Vector2Int.down);
			}
		}
	}
}