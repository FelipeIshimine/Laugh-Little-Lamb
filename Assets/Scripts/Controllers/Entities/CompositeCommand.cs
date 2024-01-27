namespace Controllers.Entities
{
	public class CompositeCommand : ICommand
	{
		public readonly ICommand[] Commands;

		public CompositeCommand(ICommand[] commands)
		{
			Commands = commands;
		}

		public void Do()
		{
			for (int i = 0; i < Commands.Length; i++)
			{
				Commands[i].Do();
			}
		}

		public void Undo()
		{
			for (int i = Commands.Length - 1; i >= 0; i--)
			{
				Commands[i].Do();
			}
		}
	}
}