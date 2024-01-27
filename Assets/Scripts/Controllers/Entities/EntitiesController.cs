using System;
using System.Collections.Generic;
using System.Linq;
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

		[SerializeField,FoldoutGroup("Prefabs")] private EnemyEntityView enemyViewPrefab;
		[SerializeField,FoldoutGroup("Prefabs")] private SheepEntityView sheepViewPrefab;
		[SerializeField,FoldoutGroup("Prefabs")] private DoorEntityView doorViewPrefab;
		[SerializeField,FoldoutGroup("Prefabs")] private TreeEntityView treeViewPrefab;
		
		[ShowInInspector,FoldoutGroup("Models")] private TilemapModel tilemapModel;
		[ShowInInspector,FoldoutGroup("Models")] private List<SheepEntityModel> sheepModels;
        public IReadOnlyList<SheepEntityModel> SheepEntityModels => sheepModels;
		[ShowInInspector,FoldoutGroup("Models")] private List<EnemyEntityModel> enemyModels;
        public IReadOnlyList<EnemyEntityModel> EnemyEntityModels => enemyModels;
		[ShowInInspector,FoldoutGroup("Models")] private List<DoorEntityModel> doorModels;
        public IReadOnlyList<DoorEntityModel> DoorModels => doorModels;
		[ShowInInspector,FoldoutGroup("Models")] private List<TreeEntityModel> treeModels;
        public IReadOnlyList<TreeEntityModel> TreeModels => treeModels;

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
			sheepModels = new List<SheepEntityModel>();
			doorModels = new List<DoorEntityModel>();
			treeModels = new List<TreeEntityModel>();
			
			foreach (var entity in model.GetAllEntities())
			{
				if (entity is TreeEntityModel treeModel)
				{
					treeModels.Add(treeModel);
					var view = Instantiate(
						treeViewPrefab,
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
				else if (entity is SheepEntityModel playerModel)
				{
					sheepModels.Add(playerModel);
					var view = Instantiate(
						sheepViewPrefab,
						GetWorldPosition(entity.PositionIndex),
						Quaternion.identity);
					playerModel.LookDirection.OnUpdate += x =>
					{
						if(x != Orientation.None)
						{view.SetLookDirection((int)x - 1);}
					};
					modelToView.Add(entity, view);
				}
				else if (entity is DoorEntityModel door)
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

		private void EntityPositionChanged(ITileEntity tileEntity, int position)
		{
			if (tileEntity is EntityModel model)
			{
				modelToView[model].transform.position = GetWorldPosition(position);
			}
			
			//TODO add animationsystem
		}

		private Vector3 GetWorldPosition(int positionIndex) => tilemapController.GetWorldPosition(positionIndex);
		private Vector3 GetWorldPosition(Vector2Int coordinate) => tilemapController.GetWorldPosition(coordinate);
		
		public ICommand CreateMoveCommand(EntityModel entityModel, Orientation direction)
		{
			Debug.Log($"MOVE:{entityModel} {direction}");
			var coordinate = tilemapModel.IndexToCoordinate(entityModel.PositionIndex);

			if (direction == Orientation.None)
			{
				return new WaitCommand();
			}
			
			var targetCoordinate = coordinate + direction.ToVector2Int();
			var targetPosition = tilemapModel.CoordinateToIndex(targetCoordinate);
			
			if (tilemapModel.IsEmpty(targetPosition))
			{
				if (entityModel is SheepEntityModel playerEntityModel)
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
				if (entityModel is SheepEntityModel playerEntityModel)
				{
					return new LookCommand(
						playerEntityModel,
						direction);
				}
				return new WaitCommand();
			}
		}


		public void MoveTogether<T>(T[] entityModels, Orientation[] direction) where T : EntityModel, IMove
		{
			ICommand[] commands = new ICommand[entityModels.Length];
			for (int i = 0; i < entityModels.Length; i++)
			{
				commands[i]=CreateMoveCommand(entityModels[i], direction[i]);
			}
			commandsController.Do(new CompositeCommand(commands.ToArray()));
			
		}
		public void MoveTogether<T>(IEnumerable<T> entityModels, Orientation direction) where T : EntityModel, IMove
		{
			MoveTogether<T>(Array.ConvertAll(entityModels.ToArray(),x=>(x,direction)));
		}
		
		public void MoveTogether<T>(IEnumerable<(T Entity, Orientation Direction)> values) where T : EntityModel, IMove
		{
			CompositeCommand command = new CompositeCommand(Array.ConvertAll(values.ToArray(), x => CreateMoveCommand(x.Entity, x.Direction)));
			commandsController.Do(command);
		}

		public EntityView GetView(EntityModel model) => modelToView[model];
	}
}