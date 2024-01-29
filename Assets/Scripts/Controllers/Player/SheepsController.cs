using System;
using System.Threading;
using Controllers.AI;
using Controllers.CommandAnimations;
using Controllers.Commands;
using Controllers.Entities;
using Cysharp.Threading.Tasks;
using Models;
using UnityEngine;

namespace Controllers.Player
{
	internal class SheepsController : MonoBehaviour, IPlayer
	{
		private EntitiesController entitiesController;

		private UniTaskCompletionSource turnCompleteSource;
		private CommandsController commandsController;
		private AnimationsController animationsController;
		private InputController inputController;
		private Action backToMenuCallback;
		private Action skipLevelCallback;
		private Action restartLevelCallback;

		public void Initialize(EntitiesController entitiesController,
		                       CommandsController commandsController,
		                       AnimationsController animationsController,
		                       InputController inputController,
		                       Action showMenuCallback,
		                       Action skipLevelCallback,
		                       Action restartLevelCallback)
		{
			this.restartLevelCallback = restartLevelCallback;
			this.skipLevelCallback = skipLevelCallback;
			this.backToMenuCallback = showMenuCallback;
			this.inputController = inputController;
			this.animationsController = animationsController;
			this.commandsController = commandsController;
			this.entitiesController = entitiesController;

			RegisterInputs();
		}

		public void Terminate()
		{
			UnregisterInputs();
		}

		private void RegisterInputs()
		{
			this.inputController.OnMoveUpEvent += OnMoveUp;
			this.inputController.OnMoveDownEvent += OnMoveDown;
			this.inputController.OnMoveLeftEvent += OnMoveLeft;
			this.inputController.OnMoveRightEvent += OnMoveRight;
			
			this.inputController.OnLookUpEvent += OnLookUp;
			this.inputController.OnLookDownEvent += OnLookDown;
			this.inputController.OnLookLeftEvent += OnLookLeft;
			this.inputController.OnLookRightEvent += OnLookRight;
			
			this.inputController.OnUndoEvent += UndoTurn;
			this.inputController.OnDoEvent += RedoTurn;
			
			this.inputController.OnWaitEvent += MakeEverySheepWait;
			this.inputController.OnMenuEvent += BackToMenu;
			
			this.inputController.OnSkipLevelEvent += SkipLevel;
			
			this.inputController.OnRestartLevelEvent += RestartLevel;
		}

		private void UnregisterInputs()
		{
			inputController.OnMoveUpEvent -= OnMoveUp;
			inputController.OnMoveDownEvent -= OnMoveDown;
			inputController.OnMoveLeftEvent -= OnMoveLeft;
			inputController.OnMoveRightEvent -= OnMoveRight;
			
			inputController.OnLookUpEvent -= OnLookUp;
			inputController.OnLookDownEvent -= OnLookDown;
			inputController.OnLookLeftEvent -= OnLookLeft;
			inputController.OnLookRightEvent -= OnLookRight;
			
			this.inputController.OnUndoEvent -= UndoTurn;
			this.inputController.OnDoEvent -= RedoTurn;
			
			this.inputController.OnWaitEvent -= MakeEverySheepWait;
			this.inputController.OnMenuEvent -= BackToMenu;;
			
			this.inputController.OnSkipLevelEvent -= SkipLevel;

			this.inputController.OnRestartLevelEvent -= RestartLevel;
		}

		private void RestartLevel() => restartLevelCallback?.Invoke();

		private void SkipLevel() => skipLevelCallback?.Invoke();

		private void BackToMenu() => backToMenuCallback?.Invoke();

		private void OnLookUp() => OnLook(Orientation.Up);
		private void OnLookDown() => OnLook(Orientation.Down);
		private void OnLookLeft() => OnLook(Orientation.Left);
		private void OnLookRight() => OnLook(Orientation.Right);

		private void OnLook(Orientation orientation)
		{
			if(turnCompleteSource.TrySetResult())
			{
				entitiesController.LookTogether(entitiesController.SheepEntityModels, orientation);
				UnregisterInputs();
			}		
		}

		private void OnMoveUp() => MoveEverySheep(Orientation.Up);  
		private void OnMoveDown() => MoveEverySheep(Orientation.Down);  
		private void OnMoveLeft() => MoveEverySheep(Orientation.Left);  
		private void OnMoveRight() => MoveEverySheep(Orientation.Right);

		private void MoveEverySheep(Orientation orientation)
		{
			if (turnCompleteSource.TrySetResult())
			{
				entitiesController.MoveTogether(entitiesController.SheepEntityModels, orientation);
				UnregisterInputs();
			}
		}

		private void MakeEverySheepWait()
		{
			if (turnCompleteSource.TrySetResult())
			{
				entitiesController.Wait(entitiesController.SheepEntityModels);
				UnregisterInputs();
			}
		}
		
		public async UniTask TakeTurnAsync(CancellationToken token)
		{
			commandsController.Do(new SheepTurnStart());
			
			RegisterInputs();
            
			gameObject.SetActive(true);
			turnCompleteSource = new UniTaskCompletionSource();
			await turnCompleteSource.Task;
            gameObject.SetActive(false);
			
            UnregisterInputs();

            commandsController.Do(new SheepTurnEnd());
		}

		public async void UndoTurn()
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

		public async void RedoTurn()
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
	}
}