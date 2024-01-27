using Models;

namespace Controllers.Entities
{
	public class MoveCommand : ICommand
	{
		public readonly int StartPosition;
		public readonly int EndPosition;
		public readonly EntityModel Target;
		public readonly TilemapModel Model;
		
		public MoveCommand(EntityModel target, TilemapModel model, int endPosition)
		{
			this.Model = model;
			this.EndPosition = endPosition;
			this.Target = target;
			StartPosition = target.PositionIndex;
		}

		public void Do()
		{
			Model.SwapEntities(StartPosition,EndPosition);
		}

		public void Undo()
		{
			Model.SwapEntities(StartPosition,EndPosition);
		}

		public override string ToString() => $"{Target.GetType().Name} {StartPosition}=>{EndPosition}";
	}
}