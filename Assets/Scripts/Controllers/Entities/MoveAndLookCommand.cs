using Models;

namespace Controllers.Entities
{
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
}