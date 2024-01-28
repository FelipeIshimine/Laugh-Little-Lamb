using System;
using System.Threading;
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
		private Action showMenuCallback;

		public void Initialize(EntitiesController entitiesController,
		                       CommandsController commandsController,
		                       AnimationsController animationsController,
		                       InputController inputController,
		                       Action showMenuCallback)
		{
			this.showMenuCallback = showMenuCallback;
			this.inputController = inputController;
			this.animationsController = animationsController;
			this.commandsController = commandsController;
			this.entitiesController = entitiesController;


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
			this.inputController.OnMenuEvent += ShowMenu;
		}

		public void Terminate()
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
			this.inputController.OnMenuEvent -= ShowMenu;;

			
		}

		private void ShowMenu() => showMenuCallback?.Invoke();

		private void OnLookUp() => OnLook(Orientation.Up);
		private void OnLookDown() => OnLook(Orientation.Down);
		private void OnLookLeft() => OnLook(Orientation.Left);
		private void OnLookRight() => OnLook(Orientation.Right);

		private void OnLook(Orientation orientation)
		{
			entitiesController.LookTogether(entitiesController.SheepEntityModels, orientation);
			inputController.gameObject.SetActive(false);
			turnCompleteSource.TrySetResult();
		}

		private void OnMoveUp() => MoveEverySheep(Orientation.Up);  
		private void OnMoveDown() => MoveEverySheep(Orientation.Down);  
		private void OnMoveLeft() => MoveEverySheep(Orientation.Left);  
		private void OnMoveRight() => MoveEverySheep(Orientation.Right);

		private void MoveEverySheep(Orientation orientation)
		{
			entitiesController.MoveTogether(entitiesController.SheepEntityModels, orientation);
			inputController.gameObject.SetActive(false);
			turnCompleteSource.TrySetResult();
		}

		private void MakeEverySheepWait()
		{
			entitiesController.Wait(entitiesController.SheepEntityModels);
			inputController.gameObject.SetActive(false);
			turnCompleteSource.TrySetResult();
		}
		
		public async UniTask TakeTurnAsync(CancellationToken token)
		{
			commandsController.Do(new SheepTurnStart());
			
			inputController.gameObject.SetActive(true);
			
            gameObject.SetActive(true);
			turnCompleteSource = new UniTaskCompletionSource();
			await turnCompleteSource.Task;
            gameObject.SetActive(false);
            
            inputController.gameObject.SetActive(false);

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