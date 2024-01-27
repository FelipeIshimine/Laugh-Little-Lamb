using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Controllers.Entities;
using Cysharp.Threading.Tasks;
using Models;
using UnityEngine;
using UnityEngine.Assertions;

namespace Controllers.AI
{
	public class EnemyAiController : MonoBehaviour, IPlayer
	{
		[SerializeField] private int moveSpeed = 2;
		
		private IReadOnlyList<EnemyEntityModel> enemies;
		private IReadOnlyList<SheepEntityModel> sheeps;
		
		private TilemapModel tilemapModel;
		
		private PathfinderModel normalPathfinder;

		private int HighValue;
		private List<int> path = new List<int>();
		private EntitiesController entitiesController;
		private TilemapController tilemapController;

		public void Initialize(EntitiesController entities,TilemapModel tilemapModel, IReadOnlyList<EnemyEntityModel> enemies, IReadOnlyList<SheepEntityModel> sheepEntityModels, TilemapController tilemapController)
		{
			this.tilemapController = tilemapController;
			this.entitiesController = entities;
			this.tilemapModel = tilemapModel;
			this.enemies = enemies;
			this.sheeps = sheepEntityModels;

			HighValue = Mathf.CeilToInt(Mathf.Sqrt(tilemapModel.Count*2));
			
			normalPathfinder = new PathfinderModel(
				this.tilemapModel.Count, 
				x=>this.tilemapModel.GetNeighbours(x).ToArray(),
				this.tilemapModel.IsFloor,
				CalculateNormalTileCost,
				CalculateNormalDistanceBetween);
		}

		private float CalculateNormalDistanceBetween(int from, int to) => Vector2.Distance(tilemapModel.IndexToCoordinate(from), tilemapModel.IndexToCoordinate(to));

		private float CalculateNormalTileCost(int index)
		{
			if (tilemapModel.IsFloor(index))
			{
				if (tilemapModel.IsIlluminated(index))
				{
					return HighValue;
				}
				return 1;
			}
			return int.MaxValue;
		}


		public UniTask TakeTurnAsync(CancellationToken token)
		{
			for (int move = 0; move < moveSpeed; move++)
			{
				int[] targetPositions = new int[sheeps.Count];
				for (var index = 0; index < sheeps.Count; index++)
				{
					targetPositions[index] = sheeps[index].PositionIndex;
				}

				Orientation[] moveDirections = new Orientation[enemies.Count];
				for (var i = 0; i < enemies.Count; i++)
				{
					normalPathfinder.CalculateAdjacencies();
					normalPathfinder.CalculateCosts();

					bool IsWalkable(int index)
					{
						var entity = tilemapModel.GetEntity(index);
						return entity == null || entity is SheepEntityModel || entity == enemies[i];
					}

					var enemyEntityModel = enemies[i];
					Pathfinding.AStar.TryFindMultiPath(
						enemyEntityModel.PositionIndex,
						targetPositions,
						IsWalkable,
						index => normalPathfinder.GetNeighbours(index),
						index => normalPathfinder.GetDistance(enemyEntityModel.PositionIndex, index),
						normalPathfinder.GetCost,
						normalPathfinder.Length,
						ref path,
						out var pathCost);

					if (path.Count > 1)
					{
						var startColor = Color.blue;
						var endColor = Color.red;

						for (int j = 0; j < path.Count - 1; j++)
						{
							Debug.DrawLine(
								tilemapController.GetWorldPosition(path[j]),
								tilemapController.GetWorldPosition(path[j + 1]),
								Color.Lerp(startColor, endColor, (float)j / (path.Count - 1)));
						}

						Assert.AreEqual(enemyEntityModel.PositionIndex, path[0]);
						var offset = tilemapModel.IndexToCoordinate(path[1]) - tilemapModel.IndexToCoordinate(path[0]);
						moveDirections[i] = OffsetToOrientation(offset);
					}
					else
					{
						moveDirections[i] = Orientation.None;
					}
				}

				entitiesController.MoveTogether(enemies.ToArray(), moveDirections);
			}

			return UniTask.CompletedTask;
		}

			

		private Orientation OffsetToOrientation(Vector2Int offset)
		{
			if (offset == Vector2Int.zero)  return Orientation.Right;
			if (offset == Vector2Int.right) return Orientation.Right;
			if (offset == Vector2Int.left) return Orientation.Left;
			if (offset == Vector2Int.up) return Orientation.Up;
			if (offset == Vector2Int.down) return Orientation.Down;
			throw new Exception($"{offset} is invalid value");
		}
	}
}