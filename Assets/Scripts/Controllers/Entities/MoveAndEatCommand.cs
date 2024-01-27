using Controllers.Commands;
using Models;

namespace Controllers.Entities
{
	public class MoveAndEatCommand : Command<MoveAndEatCommand>
	{
		public EntitiesController EntitiesController { get; }
		public readonly EnemyEntityModel Enemy;
		public readonly SheepEntityModel Sheep;
		public readonly TilemapModel TilemapModel;
		private readonly MoveCommand moveCommand;
		public MoveAndEatCommand(EnemyEntityModel enemy, SheepEntityModel sheep, TilemapModel tilemapModel,EntitiesController entitiesController)
		{
			EntitiesController = entitiesController;
			TilemapModel = tilemapModel;
			Enemy = enemy;
			Sheep = sheep;
			moveCommand = new MoveCommand(enemy, tilemapModel, sheep.PositionIndex);
		}

		protected override void DoAction()
		{
			TilemapModel.RemoveEntity(Sheep);
			EntitiesController.RemoveSheep(Sheep);
			moveCommand.Do();
		}
		protected override void UndoAction()
		{
			moveCommand.Undo();
			TilemapModel.AddEntity(Sheep, moveCommand.EndPosition);
			EntitiesController.AddSheep(Sheep);
		}
	}
}