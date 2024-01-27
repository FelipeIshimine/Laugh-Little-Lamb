using System.Collections.Generic;
using Models;
using Sirenix.OdinInspector;
using UnityEngine;
using Views;

namespace Controllers
{
	public class EntitiesController : MonoBehaviour
	{
		private CommandsController commandsController;
		
		[ShowInInspector] private TilemapModel tilemapModel;
		[ShowInInspector] private List<PlayerEntityModel> playerModels;
		[ShowInInspector] private List<EnemyEntityModel> enemyModels;
		[ShowInInspector] private List<DoorModel> doorModels;

		[SerializeField] private EnemyEntityView enemyViewPrefab;
		[SerializeField] private PlayerEntityView playerViewPrefab;
		[SerializeField] private DoorEntityView doorViewPrefab;

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
			
			foreach (var entity in model.GetAllEntities())
			{
				if (entity is EnemyEntityModel enemy)
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

					playerModel.LookDirection.OnUpdate += x => view.SetLookDirection((PlayerEntityView.EntityOrientation)x);
					 
					modelToView.Add(entity, view);
				}
				else if (entity is DoorModel door)
				{
					doorModels.Add(door);
					var view = Instantiate(
						doorViewPrefab,
						GetWorldPosition(entity.PositionIndex),
						Quaternion.identity);
					modelToView.Add(entity, view);
				}

				entity.OnPositionChange += EntityPositionChanged;
			}
		}

		private void EntityPositionChanged(EntityModel entity, int position)
		{
			modelToView[entity].transform.position = GetWorldPosition(position);
			
			
			
			//TODO add animationsystem
		}

		private Vector3 GetWorldPosition(int positionIndex) => tilemapController.GetWorldPosition(positionIndex);
		private Vector3 GetWorldPosition(Vector2Int coordinate) => tilemapController.GetWorldPosition(coordinate);
		
		public void Move(EntityModel entityModel, Orientation direction)
		{
			Debug.Log($"MOVE:{entityModel} {direction}");
			
			var coordinate = tilemapModel.IndexToCoordinate(entityModel.Index);
			var targetCoordinate = coordinate + direction.ToVector2Int();
			var targetPosition = tilemapModel.CoordinateToIndex(targetCoordinate);
			
			if (tilemapModel.IsEmpty(targetPosition))
			{
				if (entityModel is PlayerEntityModel playerEntityModel)
				{
					commandsController.Do(
						new MoveAndLookCommand(
							playerEntityModel,
							tilemapModel,
							direction,
							targetPosition));
				}
				else
				{
					commandsController.Do(
						new MoveCommand(
							entityModel,
							tilemapModel,
							targetPosition));
				}
			}
		}

		public void MovePlayers(Orientation result)
		{
			Debug.Log("MOVE PLAYERS");
			foreach (PlayerEntityModel model in playerModels)
			{
				Move(model,result);
			}
		}
	}

	public class WaitCommand : ICommand
	{
		public void Do() { }
		public void Undo() { }
	}
	
	public class MoveAndLookCommand : ICommand
	{
		public readonly Orientation StartOrientation;
		public readonly int StartPosition;
		public readonly Orientation EndOrientation;
		public readonly int EndPosition;

		private PlayerEntityModel target;
		private TilemapModel model;
		
		public MoveAndLookCommand(
			PlayerEntityModel target,
			TilemapModel model,
			Orientation endOrientation,
			int endPosition)
		{
			this.model = model;
			EndOrientation = endOrientation;
			EndPosition = endPosition;
			this.target = target;
			StartOrientation = target.LookDirection;
			StartPosition = target.PositionIndex;
		}

		public void Do()
		{
			target.LookDirection.Set(EndOrientation);
			model.SwapEntities(StartPosition,EndPosition);
		}

		public void Undo()
		{
			target.LookDirection.Set(StartOrientation);
			model.SwapEntities(StartPosition,EndPosition);
		}
	}
	
	public class MoveCommand : ICommand
	{
		private int startPosition;
		private int endPosition;

		private EntityModel target;
		private TilemapModel model;
		
		public MoveCommand(EntityModel target, TilemapModel model, int endPosition)
		{
			this.model = model;
			this.endPosition = endPosition;
			this.target = target;
		}

		public void Do()
		{
			model.SwapEntities(startPosition,endPosition);
		}

		public void Undo()
		{
			model.SwapEntities(startPosition,endPosition);
		}
	}
	
	
	public class LookAtCommand : ICommand
	{
		private readonly Orientation startOrientation;
		private readonly Orientation endOrientation;
		private PlayerEntityModel target;

		public LookAtCommand(PlayerEntityModel target, Orientation endOrientation)
		{
			startOrientation = target.LookDirection;
			this.endOrientation = endOrientation;
			this.target = target;
		}

		public void Do()
		{
			target.LookDirection.Set(endOrientation);
		}

		public void Undo()
		{
			target.LookDirection.Set(startOrientation);
		}
	}
}