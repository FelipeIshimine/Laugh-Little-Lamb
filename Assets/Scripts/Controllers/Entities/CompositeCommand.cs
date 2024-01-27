using Controllers.Commands;

namespace Controllers.Entities
{
	public class CompositeCommand : Command<CompositeCommand>
	{
		public readonly ICommand[] Commands;

		public CompositeCommand(ICommand[] commands)
		{
			Commands = commands;
		}

		protected override void DoAction()
		{
			for (int i = 0; i < Commands.Length; i++)
			{
				Commands[i].Do();
			}
		}

		protected override void UndoAction()
		{
			for (int i = Commands.Length - 1; i >= 0; i--)
			{
				Commands[i].Undo();
			}
		}
	}
}