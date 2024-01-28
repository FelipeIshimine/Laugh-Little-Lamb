using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Controllers.Commands;
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
        public Dictionary<EntityModel, EntityView> ModelToView => modelToView;

        private readonly Dictionary<EntityModel, EntityView> modelToView = new Dictionary<EntityModel, EntityView>();

		private TilemapController tilemapController;

		[SerializeField] private int lightLength = 3;
		
		public void Initialize(CommandsController commands, TilemapModel model, TilemapController tilemapController)
		{
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

					view.name = $"Tree {treeModels.Count}";
					modelToView.Add(entity, view);
				}
				else if (entity is EnemyEntityModel enemy)
				{
					enemyModels.Add(enemy);
					var view = Instantiate(
						enemyViewPrefab,
						GetWorldPosition(entity.PositionIndex),
						Quaternion.identity);
					view.name = $"Enemy {enemyModels.Count}";
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
					view.name = $"Sheep {sheepModels.Count}";
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
					view.name = $"Door {doorModels.Count}";
					modelToView.Add(entity, view);
				}
			}

			List<TurnLightOnCommand> turnOnLightsCommands = new List<TurnLightOnCommand>();
			foreach (SheepEntityModel sheepEntityModel in sheepModels)
			{
				turnOnLightsCommands.Add(new TurnLightOnCommand(sheepEntityModel, tilemapModel, lightLength));
			}
			commandsController.Do(new CompositeCommand(turnOnLightsCommands));
		}

		private Vector3 GetWorldPosition(int positionIndex) => tilemapController.GetWorldPosition(positionIndex);
		private Vector3 GetWorldPosition(Vector2Int coordinate) => tilemapController.GetWorldPosition(coordinate);
		
		public ICommand CreateCommand(EntityModel entityModel, Orientation direction)
		{
			//Debug.Log($"CreateCommand:{entityModel} {direction}");
			var coordinate = tilemapModel.IndexToCoordinate(entityModel.PositionIndex);

			if (direction == Orientation.None)
			{
				return new WaitCommand(entityModel);
			}
			
			var targetCoordinate = coordinate + direction.ToVector2Int();
			var targetPosition = tilemapModel.CoordinateToIndex(targetCoordinate);
			
			if (tilemapModel.Contains(targetPosition) && CanWalk(entityModel,targetPosition))
			{
				var targetEntity = tilemapModel.GetEntity(targetPosition);
				
				if (entityModel is SheepEntityModel sheepEntityModel)
				{
					if (targetEntity is DoorEntityModel door)
					{
						return new LookMoveAndExitCommand(sheepEntityModel, door,tilemapModel,direction, this);
					}
					
					return new CompositeCommand(
						new TurnLightOffCommand(sheepEntityModel, tilemapModel),
						new MoveAndLookCommand(
						sheepEntityModel,
						tilemapModel,
						direction,
						targetPosition),
						new TurnLightOnCommand(sheepEntityModel, tilemapModel, lightLength)
						);
				}
				if (entityModel is EnemyEntityModel enemyEntityModel && targetEntity is SheepEntityModel sheep)
				{
					return new MoveAndEatCommand(enemyEntityModel, sheep, tilemapModel,this);
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
				return new WaitCommand(entityModel);
			}
		}

		private bool CanWalk(EntityModel entityModel, int targetPosition)
		{
			var targetEntity = tilemapModel.GetEntity(targetPosition);
			if (entityModel is SheepEntityModel sheep && targetEntity is DoorEntityModel)
			{
				return true;
			}
			if (entityModel is EnemyEntityModel enemy && targetEntity is SheepEntityModel)
			{
				return true;
			}
			return tilemapModel.IsEmpty(targetPosition);
		}

		public void MoveTogether<T>(T[] entityModels, Orientation[] direction) where T : EntityModel, IMove
		{
			ICommand[] commands = new ICommand[entityModels.Length];
			for (int i = 0; i < entityModels.Length; i++)
			{
				commands[i]= CreateCommand(entityModels[i], direction[i]);
				Debug.Log(commands[i]);
			}
			commandsController.Do(new CompositeCommand(commands.ToArray()));
		}
		
		public void LookTogether(IReadOnlyList<SheepEntityModel> sheep, Orientation orientation)
		{
			List<CompositeCommand> commands = new List<CompositeCommand>();
			foreach (var entityModel in sheep)
			{
				commands.Add(
					new CompositeCommand(
						new TurnLightOffCommand(entityModel, tilemapModel),
					new LookCommand(entityModel, orientation),
						new TurnLightOnCommand(entityModel, tilemapModel, lightLength)
					));
			}
			commandsController.Do(new CompositeCommand(commands));
		}
		
		public void MoveTogether<T>(IEnumerable<T> entityModels, Orientation direction) where T : EntityModel, IMove
		{
			MoveTogether<T>(Array.ConvertAll(entityModels.ToArray(),x=>(x,direction)));
		}
		
		public void MoveTogether<T>(IEnumerable<(T Entity, Orientation Direction)> values) where T : EntityModel, IMove
		{
			CompositeCommand command = new CompositeCommand(Array.ConvertAll(values.ToArray(), x => CreateCommand(x.Entity, x.Direction)));
			commandsController.Do(command);
		}
		
		
		
		public void Move(EntityModel model, Orientation moveDirection) => commandsController.Do(CreateCommand(model, moveDirection));

		public EntityView GetView(EntityModel model) => modelToView[model];

		public IEnumerable<EntityView> GetSheepViews() => GetViews(sheepModels);
		public IEnumerable<EntityView> GetEnemyViews() => GetViews(enemyModels);
		public IEnumerable<EntityView> GetDoorViews() => GetViews(doorModels);

		public IEnumerable<EntityView> GetViews(IEnumerable<EntityModel> models)
		{
			foreach (EntityModel entityModel in models)
			{
				yield return modelToView[entityModel];
			}
		}

		public void RemoveSheep(SheepEntityModel sheepModel) => sheepModels.Remove(sheepModel);
		public void AddSheep(SheepEntityModel sheepModel) => sheepModels.Add(sheepModel);

		public void KillSheep(SheepEntityModel sheep)
		{
			tilemapModel.KillSheep(sheep);
			RemoveSheep(sheep);
		}

		public void ReviveSheep(SheepEntityModel sheep, int sheepPosition)
		{
			tilemapModel.ReviveSheep(sheep, sheepPosition);
			AddSheep(sheep);
		}

		public void Wait(IReadOnlyList<EntityModel> entityModels)
		{
			List<WaitCommand> commands = new List<WaitCommand>();
			foreach (var entity in entityModels)
			{
				commands.Add(new WaitCommand(entity));
			}
			commandsController.Do(new CompositeCommand(commands));
		}

		
	}

}