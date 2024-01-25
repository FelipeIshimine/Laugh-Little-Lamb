using System;
using System.Collections;
using System.Collections.Generic;
using Model;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Models
{
	[System.Serializable]
	public class TilemapModel
	{
		public event Action<TilemapModel> OnUpdate;
		
		public BoundsInt Bounds;
		public List<int> FloorTiles = new List<int>();

		public int Count => Bounds.size.x * Bounds.size.y;

		public TilemapModel(BoundsInt bounds, List<int> floorTiles)
		{
			this.Bounds = bounds;
			this.FloorTiles = floorTiles;
		}

		[Button] public int CoordinateToIndex(Vector2Int coordinate) => coordinate.x - Bounds.x + (coordinate.y - Bounds.y) * Bounds.size.x;
		[Button] public Vector2Int IndexToCoordinate(int index) => new Vector2Int(index % Bounds.size.x, index / Bounds.size.x) + new Vector2Int(Bounds.min.x,Bounds.min.y);

		public IEnumerable<int> GetNeighbours(int index)
		{
			var center = IndexToCoordinate(index);
			var neighbour = center + Vector2Int.left;
			if (Bounds.Contains((Vector3Int)neighbour))
			{
				yield return CoordinateToIndex(neighbour);
			}

			neighbour = center + Vector2Int.right;	
			if (Bounds.Contains((Vector3Int)neighbour))
			{
				yield return CoordinateToIndex(center + Vector2Int.right);
			}

			neighbour = center + Vector2Int.up;	
			if (Bounds.Contains((Vector3Int)neighbour))
			{
				yield return CoordinateToIndex(center + Vector2Int.up);
			}

			neighbour = center + Vector2Int.down;
			if (Bounds.Contains((Vector3Int)neighbour))
			{
				yield return CoordinateToIndex(center + Vector2Int.down);
			}
		}

		public bool IsFloor(int neighbour) => FloorTiles.Contains(neighbour);
	}
}