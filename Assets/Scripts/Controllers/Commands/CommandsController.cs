using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Controllers
{
	public class CommandsController : MonoBehaviour
	{
		private readonly Stack<ICommand> historyStack = new Stack<ICommand>();
		private readonly Stack<ICommand> redoStack = new Stack<ICommand>();

		public void Initialize()
		{
			historyStack.Clear();
			redoStack.Clear();
		}

		public void Terminate()
		{
		}

		public void Do(ICommand command)
		{
			Debug.Log($"DO:{command}");
			if (redoStack.Count > 0)
			{
				redoStack.Clear();
			}
			historyStack.Push(command);
			command.Do();
		}

		[Button]
		public void Redo()
		{
			if (redoStack.Count > 0)
			{
				var command = redoStack.Pop();
				command.Do();
				Debug.Log($"Redo:{command}");
				historyStack.Push(command);
			}
		}

		[Button]
		public void Undo()
		{
			if (historyStack.Count > 0)
			{
				var command = historyStack.Pop();
				Debug.Log($"Undo:{command}");
				redoStack.Push(command);
				command.Do();
			}
		}
	}
}