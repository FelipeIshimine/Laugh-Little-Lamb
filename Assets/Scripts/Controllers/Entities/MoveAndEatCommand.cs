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
		public readonly MoveCommand MoveCmd;
		public readonly EatCommand EatCmd;
		public MoveAndEatCommand(EnemyEntityModel enemy, SheepEntityModel sheep, TilemapModel tilemapModel,EntitiesController entitiesController)
		{
			EntitiesController = entitiesController;
			TilemapModel = tilemapModel;
			Enemy = enemy;
			Sheep = sheep;
			MoveCmd = new MoveCommand(enemy, tilemapModel, sheep.PositionIndex);
			EatCmd = new EatCommand(enemy, sheep, tilemapModel, entitiesController);
		}

		protected override void DoAction()
		{
			MoveCmd.Do();
			TilemapModel.SwapEntities(Sheep.PositionIndex, Enemy.PositionIndex);
			EatCmd.Do();
		}
		protected override void UndoAction()
		{
			EatCmd.Undo();
			TilemapModel.SwapEntities(Sheep.PositionIndex, Enemy.PositionIndex);
			MoveCmd.Undo();
		}
	}
}