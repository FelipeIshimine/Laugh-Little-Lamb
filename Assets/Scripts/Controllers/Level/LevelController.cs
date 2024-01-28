using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Controllers.AI;
using Controllers.CommandAnimations;
using Controllers.Commands;
using Controllers.Entities;
using Controllers.Player;
using Cysharp.Threading.Tasks;
using Models;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Controllers.Level
{
	public partial class LevelController : MonoBehaviour
	{
		private Action<Result> resultCallback;
		
		[SerializeField] private TilemapController tilemapController;
		[SerializeField] private CommandsController commandsController;
		
		[SerializeField] private SheepsController sheepsController;
		[SerializeField] private EnemyAiController enemyAiController;
		
		[SerializeField] private EntitiesController entitiesController;
		[SerializeField] private CameraController cameraController;
		[SerializeField] private AnimationsController animationsController;

		[SerializeField] private TilemapModel tilemapModel;

		[SerializeField] private InputAction inputUndo;
		[SerializeField] private InputAction inputRedo;
		

		public IPlayer[] players;
		private async UniTaskVoid Awake()
		{
			Initialize(null);
			
			await GameLoop(destroyCancellationToken);
		}

		[Button]
		public void Initialize(Action<Result> callback)
		{
			resultCallback = callback;
			
			commandsController.Initialize();
		
			tilemapModel =	tilemapController.ProcessTileMaps();
			
			animationsController.Initialize(commandsController, entitiesController.ModelToView, tilemapController);
			entitiesController.Initialize(commandsController, tilemapModel, tilemapController);
		
			sheepsController.Initialize(entitiesController, commandsController);
			enemyAiController.Initialize(entitiesController,tilemapModel, entitiesController.EnemyEntityModels, entitiesController.SheepEntityModels,tilemapController, commandsController);
			
			cameraController.Initialize(
				entitiesController.GetSheepViews().ToArray(),
				entitiesController.GetEnemyViews().ToArray(),
				entitiesController.GetDoorViews().ToArray());

			inputUndo.performed += UndoTurn;
			inputRedo.performed += RedoTurn;
			inputUndo.Enable();
			inputRedo.Enable();
		}

		public void Terminate()
		{
			inputUndo.Disable();
			inputRedo.Disable();
			inputUndo.performed -= UndoTurn;
			inputRedo.performed -= RedoTurn;

			commandsController.Terminate();
			cameraController.Terminate();
		}

		private async void UndoTurn(InputAction.CallbackContext obj)
		{
			if (animationsController.IsPlaying)
			{
				return;
			}
			if (commandsController.HistoryStack.TryPeek(out var result) && result is SheepTurnStart)
			{
				commandsController.Undo();
			}
			commandsController.Undo<SheepTurnStart>();

			Time.timeScale = 3;
			while (animationsController.IsPlaying)
			{
				await UniTask.NextFrame();
			}
			Time.timeScale = 1;
		}

		private async void RedoTurn(InputAction.CallbackContext obj)
		{
			if (animationsController.IsPlaying)
			{
				return;
			}
			if (commandsController.RedoStack.TryPeek(out var result) && result is SheepTurnStart)
			{
				commandsController.Redo();
			}
			commandsController.Redo<SheepTurnStart>();
			
			Time.timeScale = 3;
			while (animationsController.IsPlaying)
			{
				await UniTask.NextFrame();
			}
			Time.timeScale = 1;
		}

		public async UniTask GameLoop(CancellationToken token)
		{
			players = new IPlayer[]
			{
				sheepsController,
				enemyAiController
			};
			
			while (true)
			{
				if (gameObject == null)
				{
					break;
				}

				foreach (IPlayer player in players)
				{
					await player.TakeTurnAsync(token);
					while (animationsController.IsPlaying)
					{
						//Debug.Log("Waiting for AnimationSystem");
						await Task.Yield();
					}
				}
			}
		}
	}

	public partial class LevelController
	{
		[System.Serializable] public abstract class Result { }
		[System.Serializable] public class Quit : Result { }
		[System.Serializable] public class Win : Result { }
		[System.Serializable] public class Lose : Result { }
	}
}

