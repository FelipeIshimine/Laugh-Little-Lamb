using System;
using System.Collections.Generic;
using Controllers.Commands;

namespace Controllers.Commands
{
	public abstract class Command<T> : ICommand where T : Command<T> 
	{
		public static readonly CommandBroadcaster<T> OnDo = new CommandBroadcaster<T>();
		public static readonly CommandBroadcaster<T> OnUndo = new CommandBroadcaster<T>();

		public void Do()
		{
			DoAction();
			OnDo.Raise((T)this);
		}

		public void Undo()
		{
			UndoAction();
			OnUndo.Raise((T)this);
		}

		protected abstract void DoAction();
		protected abstract void UndoAction();
	}


	public interface ICommand
	{
		void Do();
		void Undo();
	}
	
	
}