using System;
using System.Collections.Generic;
using Model;
using Models;
using Sirenix.OdinInspector;
using UnityEngine;
using Views;

namespace Controllers.Entities
{
	public class EntitiesController : MonoBehaviour
	{
		private CommandsController commandsController;
		
		[ShowInInspector] private TilemapModel tilemapModel;
		[ShowInInspector] private List<PlayerEntityModel> playerModels;
		[ShowInInspector] private List<EnemyEntityModel> enemyModels;
		[ShowInInspector] private List<DoorModel> doorModels;
		[ShowInInspector] private List<TreeModel> treeModels;

		[SerializeField] private EnemyEntityView enemyViewPrefab;
		[SerializeField] private PlayerEntityView playerViewPrefab;
		[SerializeField] private DoorEntityView doorViewPrefab;
		[SerializeField] private TreeEntityView treeViewPrefab;

		private Dictionary<EntityModel, EntityView> modelToView = new Dictionary<EntityModel, EntityView>();

		private TilemapController tilemapController;

		private AnimationSystem animationSystem;
		
		public void Initialize(CommandsController commands, TilemapModel model, TilemapController tilemapController, AnimationSystem animationSystem)
		{
			this.animationSystem = animationSystem;
			this.tilemapController = tilemapController;
			commandsController = commands;
			tilemapModel = model;
			
			enemyModels = new List<EnemyEntityModel>();
			playerModels = new List<PlayerEntityModel>();
			doorModels = new List<DoorModel>();
			treeModels = new List<TreeModel>();
			
			foreach (var entity in model.GetAllEntities())
			{
				if (entity is TreeModel treeModel)
				{
					treeModels.Add(treeModel);
					var view = Instantiate(
						enemyViewPrefab,
						GetWorldPosition(entity.PositionIndex),
						Quaternion.identity);
					modelToView.Add(entity, view);
				}
				else if (entity is EnemyEntityModel enemy)
				{
					enemyModels.Add(enemy);
					var view = Instantiate(
						enemyViewPrefab,
						GetWorldPosition(entity.PositionIndex),
						Quaternion.identity);
					modelToView.Add(entity, view);
				}
				else if (entity is PlayerEntityModel playerModel)
				{
					playerModels.Add(playerModel);
					var view = Instantiate(
						playerViewPrefab,
						GetWorldPosition(entity.PositionIndex),
						Quaternion.identity);
					playerModel.LookDirection.OnUpdate += x => view.SetLookDirection((int)x);
					modelToView.Add(entity, view);
				}
				else if (entity is DoorModel door)
				{
					doorModels.Add(door);
					var view = Instantiate(
						doorViewPrefab,
						GetWorldPosition(entity.PositionIndex),
						Quaternion.identity);
					view.SetOpen(true);
					modelToView.Add(entity, view);
				}

				entity.OnPositionChange += EntityPositionChanged;
			}
		}

		private void EntityPositionChanged<T>(T entity, int position) where T : EntityModel<T>
		{
			modelToView[entity].transform.position = GetWorldPosition(position);
			
			
			//TODO add animationsystem
		}

		private Vector3 GetWorldPosition(int positionIndex) => tilemapController.GetWorldPosition(positionIndex);
		private Vector3 GetWorldPosition(Vector2Int coordinate) => tilemapController.GetWorldPosition(coordinate);
		
		public ICommand CreateMoveCommand<T>(T entityModel, Orientation direction) where T : EntityModel<T>, IMove
		{
			Debug.Log($"MOVE:{entityModel} {direction}");
			var coordinate = tilemapModel.IndexToCoordinate(entityModel.PositionIndex);
			var targetCoordinate = coordinate + direction.ToVector2Int();
			var targetPosition = tilemapModel.CoordinateToIndex(targetCoordinate);
			
			if (tilemapModel.IsEmpty(targetPosition))
			{
				if (entityModel is PlayerEntityModel playerEntityModel)
				{
					return new MoveAndLookCommand(
							playerEntityModel,
							tilemapModel,
							direction,
							targetPosition);
				}
				return new MoveCommand(
					entityModel,
					tilemapModel,
					targetPosition);
			}
			else
			{
				if (entityModel is PlayerEntityModel playerEntityModel)
				{
					return new LookCommand(
						playerEntityModel,
						direction);
				}
				return new WaitCommand();
			}
		}

		public void MoveTogether<T>(T[] entityModels, Orientation direction) where T : EntityModel<T>, IMove
		{
			CompositeCommand command = new CompositeCommand(
				Array.ConvertAll(entityModels, x => CreateMoveCommand(x, direction)));
		}
		
		/*public void MovePlayers(Orientation result)
		{
			Debug.Log("MOVE PLAYERS");
			foreach (PlayerEntityModel model in playerModels)
			{
				CreateMoveCommand(model,result);
			}
		}*/
	}
}