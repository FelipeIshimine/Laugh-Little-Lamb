using Models;

namespace Controllers.Entities
{
	public class LookCommand : ICommand
	{
		private readonly Orientation startOrientation;
		private readonly Orientation endOrientation;
		private PlayerEntityModel target;

		public LookCommand(PlayerEntityModel target, Orientation endOrientation)
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