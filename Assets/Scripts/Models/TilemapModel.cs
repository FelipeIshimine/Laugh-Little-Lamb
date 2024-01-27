﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Models
{
	[System.Serializable]
	public class TilemapModel
	{
		[SerializeField] private List<int> floorTiles;
		[SerializeField] private List<int> illuminatedTiles;
		[SerializeField] private EntityModel[] entities;
		[SerializeField] private BoundsInt bounds;

		public IReadOnlyList<int> FloorTiles => floorTiles;

		public TilemapModel(BoundsInt bounds, List<int> floorTiles, EntityModel[] entities) 
		{
			this.bounds = bounds;
			this.floorTiles = floorTiles;
			this.entities = entities;
			illuminatedTiles = new List<int>();
		}

		public bool IsFloor(int index) => floorTiles.Contains(index);

		public int Count => bounds.size.x * bounds.size.y;
		public int CoordinateToIndex(Vector2Int coordinate) => coordinate.x - bounds.x + (coordinate.y - bounds.y) * bounds.size.x;
		public Vector2Int IndexToCoordinate(int index) => new Vector2Int(index % bounds.size.x, index / bounds.size.x) + new Vector2Int(bounds.min.x,bounds.min.y);

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


		public EntityModel GetEntity(int index) => entities[index];


		public IEnumerable<EntityModel> GetAllEntities()
		{
			foreach (EntityModel entity in entities)
			{
				if (entity != null)
				{
					yield return entity;
				}
			}
		}

		public bool TryGetEntity(Vector2Int coordinate, out EntityModel content)
		{
			if (bounds.Contains((Vector3Int)coordinate))
			{
				var index = CoordinateToIndex(coordinate);
				content = entities[index];
				return true;
			}
			content = null;
			return false;
		}

		public bool TryMoveEntity(EntityModel entityModel, int targetPosition)
		{
			if (IsEmpty(targetPosition))
			{
				entities[targetPosition] = entityModel;
				entityModel.PositionIndex = -1;
				return true;
			}
			return false;
		}

		public void RemoveEntity(EntityModel entityModel)
		{
			entities[entityModel.PositionIndex] = null;
			entityModel.PositionIndex = -1;
		}

		public void AddEntity(SheepEntityModel entityModel, int startPosition)
		{
			entities[startPosition] = entityModel;
			entityModel.PositionIndex = startPosition;
		}
		
		public bool IsEmpty(int index) => entities[index] == null;


		public void SwapEntities(int startPosition, int endPosition)
		{
			(entities[startPosition], entities[endPosition]) = (entities[endPosition], entities[startPosition]);

			if (entities[startPosition] != null)
			{
				entities[startPosition].PositionIndex = startPosition;
			}

			if (entities[endPosition] != null)
			{
				entities[endPosition].PositionIndex = endPosition;
			}
		}

		public bool IsEmpty(Vector2Int targetCoordinate) => IsEmpty(CoordinateToIndex(targetCoordinate));


		public void Illuminate(params int[] indexes)
		{
			illuminatedTiles.AddRange(indexes);
		}

		public void Obscure(params int[] indexes)
		{
			foreach (int index in indexes)
			{
				illuminatedTiles.Remove(index);
			}
		}

		public bool IsIlluminated(int index) => illuminatedTiles.Contains(index);

		public bool Contains(int targetPosition) => bounds.Contains((Vector3Int)IndexToCoordinate(targetPosition));

	
	}
}