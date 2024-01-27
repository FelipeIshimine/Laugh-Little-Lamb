using System;

namespace Controllers
{
	public interface ICommand
	{
		void Do();
		void Undo();
	}


	public class Command : ICommand
	{
		private readonly Action doAction;
		private readonly Action undoAction;

		public Command(Action doAction, Action undoAction)
		{
			this.doAction = doAction;
			this.undoAction = undoAction;
		}

		public void Do() => doAction.Invoke();
		public void Undo() => undoAction.Invoke();
	}
}