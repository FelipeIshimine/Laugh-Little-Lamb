using Controllers.Commands;
using Models;

namespace Controllers.Entities
{
	public class ExitCommand : Command<ExitCommand>
	{
		public readonly int StartPosition;
		public readonly SheepEntityModel SheepModel;
		public readonly DoorEntityModel Door;
		public readonly TilemapModel Model;
		public readonly EntitiesController EntitiesController;
		public ExitCommand(SheepEntityModel sheep, DoorEntityModel door, TilemapModel model, EntitiesController entitiesController)
		{
			this.EntitiesController = entitiesController;
			Door = door;
			Model = model;
			StartPosition = sheep.PositionIndex;
			SheepModel = sheep;
		}

		protected override void DoAction()
		{
			Model.RemoveEntity(SheepModel);
			EntitiesController.RemoveSheep(SheepModel);
		}

		protected override void UndoAction()
		{
			EntitiesController.AddSheep(SheepModel);
			Model.AddEntity(SheepModel, StartPosition);
		}
	}
}