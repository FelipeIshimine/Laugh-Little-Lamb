using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Controllers.Commands
{
	public class CommandsController : MonoBehaviour
	{
		[ShowInInspector,ReadOnly, FoldoutGroup("Debug")] private readonly Stack<ICommand> historyStack = new Stack<ICommand>();
		[ShowInInspector,ReadOnly, FoldoutGroup("Debug")] private readonly Stack<ICommand> redoStack = new Stack<ICommand>();

		[SerializeField] private InputAction inputUndo;
		[SerializeField] private InputAction inputRedo;
		
		public void Initialize()
		{
			historyStack.Clear();
			redoStack.Clear();
			inputUndo.performed += InputUndo;
			inputRedo.performed += InputRedo;
			inputUndo.Enable();
			inputRedo.Enable();
		}

	
		public void Terminate()
		{
			inputUndo.performed -= InputUndo;
			inputRedo.performed -= InputRedo;
			inputUndo.Disable();
			inputRedo.Disable();
		}
		
		private void InputRedo(InputAction.CallbackContext obj) => Redo();

		private void InputUndo(InputAction.CallbackContext obj) => Undo();


		public void Do(ICommand command)
		{
			//Debug.Log($"DO:{command}");
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
				//Debug.Log($"Redo:{command}");
				historyStack.Push(command);
			}
		}

		[Button]
		public void Undo()
		{
			if (historyStack.Count > 0)
			{
				var command = historyStack.Pop();
				//Debug.Log($"Undo:{command}");
				redoStack.Push(command);
				command.Undo();
			}
		}

		[Button("UndoX2")]
		public void UndoTwice()
		{
			for (int i = 0; i < 2; i++) Undo();
		}
		
		[Button("RedoX2")]
		public void RedoTwice()
		{
			for (int i = 0; i < 2; i++) Redo();
		}
	}

	public abstract class CommandListener : IComparable<CommandListener> 
	{
		public readonly int Priority;
		public CommandListener(int priority)
		{
			Priority = priority;
		}

		public abstract void Raise(object value);

		public int CompareTo(CommandListener other)
		{
			if (ReferenceEquals(this, other)) return 0;
			if (ReferenceEquals(null, other)) return 1;
			return Priority.CompareTo(other.Priority);
		}
	}
	
	public class CommandListener<T> : CommandListener where T : Command<T>
	{
		private readonly Action<T> action;
		public CommandListener(Action<T> action, int priority = 0) : base(priority)
		{
			this.action = action;
		}
		public override void Raise(object value) => action.Invoke((T)value);
	}

}