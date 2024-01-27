using Models;

namespace Controllers.Entities
{
	public class MoveAndEatCommand : ICommand
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

		public void Do()
		{
			TilemapModel.RemoveEntity(Sheep);
			EntitiesController.RemoveSheep(Sheep);
			moveCommand.Do();
		}

		public void Undo()
		{
			moveCommand.Undo();
			TilemapModel.AddEntity(Sheep, moveCommand.EndPosition);
			EntitiesController.AddSheep(Sheep);
		}
	}
}