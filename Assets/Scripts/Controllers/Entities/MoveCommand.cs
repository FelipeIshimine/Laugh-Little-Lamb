using Models;

namespace Controllers.Entities
{
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
			startPosition = target.PositionIndex;
		}

		public void Do()
		{
			model.SwapEntities(startPosition,endPosition);
		}

		public void Undo()
		{
			model.SwapEntities(startPosition,endPosition);
		}

		public override string ToString() => $"{target.GetType().Name} {startPosition}=>{endPosition}";
	}
}