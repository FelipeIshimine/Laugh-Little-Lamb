using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        [SerializeField, FoldoutGroup("Animation")] private float animMoveSpeed = 3; 
        [SerializeField, FoldoutGroup("Animation")] private AnimationCurve animMoveCurve = AnimationCurve.EaseInOut(0,0,1,1); 
        
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
				var view = modelToView[model];
				if (position == -1)
				{
					Debug.Log($"{tileEntity} Removed from Tilemap");
					view.gameObject.SetActive(false);
				}
				else
				{
					view.gameObject.SetActive(true);
					
					animationSystem.Play(MoveAnimation(view,GetWorldPosition(position)), view);
					//view.transform.position = GetWorldPosition(position);
				}
			}
			
		}

		private IEnumerator MoveAnimation(EntityView view, Vector3 endPosition)
		{
			var startPosition = view.transform.position;
			float t = 0;
			float duration = Vector2.Distance(startPosition, endPosition) / animMoveSpeed; 
			do
			{
				t += Time.deltaTime / duration;
				view.transform.position = Vector3.LerpUnclamped(startPosition,endPosition,animMoveCurve.Evaluate(t));
				yield return null;
			} while (t<1);
		}

		private Vector3 GetWorldPosition(int positionIndex) => tilemapController.GetWorldPosition(positionIndex);
		private Vector3 GetWorldPosition(Vector2Int coordinate) => tilemapController.GetWorldPosition(coordinate);
		
		public ICommand CreateCommand(EntityModel entityModel, Orientation direction)
		{
			Debug.Log($"MOVE:{entityModel} {direction}");
			var coordinate = tilemapModel.IndexToCoordinate(entityModel.PositionIndex);

			if (direction == Orientation.None)
			{
				return new WaitCommand();
			}
			
			var targetCoordinate = coordinate + direction.ToVector2Int();
			var targetPosition = tilemapModel.CoordinateToIndex(targetCoordinate);
			
			if (tilemapModel.Contains(targetPosition) && CanWalk(entityModel,targetPosition))
			{
				var targetEntity = tilemapModel.GetEntity(targetPosition);
				
				if (entityModel is SheepEntityModel playerEntityModel)
				{
					if (targetEntity is DoorEntityModel door)
					{
						return new ExitCommand(playerEntityModel, door, tilemapModel, this);
					}

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

		private bool CanWalk(EntityModel entityModel, int targetPosition)
		{
			var targetEntity = tilemapModel.GetEntity(targetPosition);
			if (entityModel is SheepEntityModel sheep && targetEntity is DoorEntityModel doorEntityModel)
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
		public void MoveTogether<T>(IEnumerable<T> entityModels, Orientation direction) where T : EntityModel, IMove
		{
			MoveTogether<T>(Array.ConvertAll(entityModels.ToArray(),x=>(x,direction)));
		}
		
		public void MoveTogether<T>(IEnumerable<(T Entity, Orientation Direction)> values) where T : EntityModel, IMove
		{
			CompositeCommand command = new CompositeCommand(Array.ConvertAll(values.ToArray(), x => CreateCommand(x.Entity, x.Direction)));
			commandsController.Do(command);
		}

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
	}
}