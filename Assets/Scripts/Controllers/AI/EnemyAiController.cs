using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Controllers.Commands;
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
		private PathfinderModel scaredPathfinder;

		private int HighValue;
		private List<int> path = new List<int>();
		private EntitiesController entitiesController;
		private TilemapController tilemapController;

		private CommandsController commandsController;
	
		public void Initialize(EntitiesController entities,TilemapModel tilemapModel, IReadOnlyList<EnemyEntityModel> enemies, IReadOnlyList<SheepEntityModel> sheepEntityModels, TilemapController tilemapController, CommandsController commandsController)
		{
			this.commandsController = commandsController;
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
			
			scaredPathfinder = new PathfinderModel(
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
			//Orientation[] moveDirections = new Orientation[enemies.Count];
			
			commandsController.Do(new EnemyTurnStart());
			
			for (var i = 0; i < enemies.Count; i++)
			{
				for (int move = 0; move < moveSpeed; move++)
				{
					if (IsEnemyScared(i))
					{
						ProcessScaredEnemy(i);
					}
					else
					{
						ProcessNormalEnemy(i);
					}
					//Debug.Break();
				}
			}

			//entitiesController.MoveTogether(enemies.ToArray(), moveDirections);
			commandsController.Do(new EnemyTurnEnd());
			return UniTask.CompletedTask;
		}

		private bool IsEnemyScared(int i)
		{
			var enemyPosition = enemies[i];
			return tilemapModel.IsIlluminated(enemyPosition.PositionIndex);
		}

		private void ProcessNormalEnemy(int i)
		{
			//Debug.Log($"Process Normal Enemy {i}");
			var enemyIndex = i;
	
			bool IsWalkable(int index)
			{
				var entity = tilemapModel.GetEntity(index);
				if (tilemapModel.IsIlluminated(index))
				{
					return false;
				}
				return entity == null || entity is SheepEntityModel || entity == enemies[enemyIndex];
			}
			
			int[] targetPositions = new int[sheeps.Count];
			for (var index = 0; index < sheeps.Count; index++)
			{
				targetPositions[index] = sheeps[index].PositionIndex;
			}
			
			ProcessEnemy(i, normalPathfinder, IsWalkable,targetPositions);
		}

		private void ProcessEnemy(int i, PathfinderModel pathFinderModel, Predicate<int> isWalkable, int[] targetPositions)
		{
			pathFinderModel.CalculateAdjacencies();
			pathFinderModel.CalculateCosts();
			Orientation moveDirection;
					
			var enemyEntityModel = enemies[i];
			bool success = Pathfinding.AStar.TryFindMultiPath(
				enemyEntityModel.PositionIndex,
				targetPositions,
				isWalkable,
				pathFinderModel.GetNeighbours,
				index => pathFinderModel.GetDistance(enemyEntityModel.PositionIndex, index),
				pathFinderModel.GetCost,
				pathFinderModel.Length,
				ref path,
				out var pathCost);

			if (success && path.Count > 1)
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
				moveDirection = OffsetToOrientation(offset);
			}
			else
			{
				moveDirection = Orientation.None;
			}
			entitiesController.Move(enemies[i], moveDirection);
		}

		private void ProcessScaredEnemy(int i)
		{
			Debug.Log("ProcessScaredEnemy");
			int enemyIndex = i;
			bool IsWalkable(int index)
			{
				var entity = tilemapModel.GetEntity(index);
				return entity == null || entity == enemies[enemyIndex];
			}

			var currentEnemy = enemies[enemyIndex];
			List<int> targetPositions = new List<int>(tilemapModel.FloorTiles.Count);
			foreach (int tileIndex in tilemapModel.FloorTiles)
			{
				var otherEntity = tilemapModel.GetEntity(tileIndex);
				if (!tilemapModel.IsIlluminated(tileIndex) && (otherEntity == null ||  otherEntity != currentEnemy))
				{
					targetPositions.Add(tileIndex);
				}
			}
			ProcessEnemy(i, scaredPathfinder, IsWalkable, targetPositions.ToArray());
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