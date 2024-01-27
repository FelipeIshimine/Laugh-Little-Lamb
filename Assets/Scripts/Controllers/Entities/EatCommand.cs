using Controllers.Commands;
using Models;

namespace Controllers.Entities
{
	public class EatCommand : Command<EatCommand>
	{
		public readonly EnemyEntityModel Enemy;
		public readonly SheepEntityModel Sheep;
		public readonly TilemapModel TilemapModel;
		public readonly EntitiesController EntitiesController;
		public readonly int SheepPosition;
		public EatCommand(EnemyEntityModel enemy, SheepEntityModel sheep, TilemapModel tilemapModel, EntitiesController entitiesController)
		{
			Enemy = enemy;
			Sheep = sheep;
			SheepPosition = sheep.PositionIndex;
			TilemapModel = tilemapModel;
			EntitiesController = entitiesController;
		}

		protected override void DoAction()
		{
			TilemapModel.RemoveEntity(Sheep);
			EntitiesController.RemoveSheep(Sheep);
		}

		protected override void UndoAction()
		{
			TilemapModel.AddEntity(Sheep, SheepPosition);
			EntitiesController.AddSheep(Sheep);
		}
	}
}