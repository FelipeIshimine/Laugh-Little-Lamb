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
using UnityEngine;
using UnityEngine.Tilemaps;
using Views.Canvases;

namespace Controllers.Level
{
	public partial class LevelController : MonoBehaviour
	{
		private UniTaskCompletionSource<Result> resultCompletionSource;
		[SerializeField] private TilemapController tilemapController;
		[SerializeField] private CommandsController commandsController;
		
		[SerializeField] private SheepsController sheepsController;
		[SerializeField] private EnemyAiController enemyAiController;
		
		[SerializeField] private EntitiesController entitiesController;
		[SerializeField] private CameraController cameraController;
		[SerializeField] private AnimationsController animationsController;
		[SerializeField] private InputController inputController;
		
		[SerializeField] private GameOverCanvasView gameOverController;

		[SerializeField] private TilemapModel tilemapModel;

		
		private async UniTaskVoid Start()
		{
			if(GameFlow.Instance == null)
			{
				await Run(null,null);
			}
		}

		private void Initialize(Tilemap terrainTilemap, Tilemap entitiesTilemap)
		{
			if (terrainTilemap != null && entitiesTilemap != null)
			{
				tilemapController.SetTileMaps(terrainTilemap,entitiesTilemap);
			}

			inputController.Initialize();
			
			commandsController.Initialize();
		
			tilemapModel =	tilemapController.ProcessTileMaps();
			
			animationsController.Initialize(commandsController, entitiesController.ModelToView, tilemapController);
			entitiesController.Initialize(commandsController, tilemapModel, tilemapController);
		
			sheepsController.Initialize(entitiesController, commandsController, animationsController, inputController, ShowMenu, SkipLevel, RestartLevel);
			enemyAiController.Initialize(entitiesController,tilemapModel, entitiesController.EnemyEntityModels, entitiesController.SheepEntityModels,tilemapController, commandsController);
			
			
			cameraController.Initialize(
				entitiesController.GetSheepViews().ToArray(),
				entitiesController.GetEnemyViews().ToArray(),
				entitiesController.GetDoorViews().ToArray());
		}

		public void Terminate()
		{
			entitiesController.Terminate();
			inputController.Terminate();
			commandsController.Terminate();
			cameraController.Terminate();
			
			animationsController.Terminate();
		
			sheepsController.Terminate();
		}

		private void RestartLevel() => resultCompletionSource.TrySetResult(new RestartResult());

		private void SkipLevel() => resultCompletionSource?.TrySetResult(new WinResult());

		private void ShowMenu() => resultCompletionSource?.TrySetResult(new QuitResult());

		public async UniTask<Result> Run(Tilemap terrainTilemap, Tilemap entitiesTilemap)
		{
			Debug.Log("Level Run");
			Initialize(terrainTilemap, entitiesTilemap);
			resultCompletionSource = new UniTaskCompletionSource<Result>();
			
			GameLoop(destroyCancellationToken).Forget();

			var result = await resultCompletionSource.Task;

			Terminate();

			return result;
		}


		private async UniTaskVoid GameLoop(CancellationToken token)
		{
			var players = new IPlayer[]
			{
				sheepsController,
				enemyAiController
			};

			bool canContinue = true;
			while (canContinue)
			{
				if (gameObject == null)
				{
					break;
				}

				foreach (IPlayer player in players)
				{
					//Debug.Log($"{player.GetType()} Turn Started");
					await player.TakeTurnAsync(token);
					while (animationsController.IsPlaying)
					{
						await UniTask.NextFrame();
					}
					//Debug.Log($"{player.GetType()} Turn Ended");

					if (PlayerWon())
					{
						resultCompletionSource?.TrySetResult(new WinResult());
						canContinue = false;
						break;
					}

					if (PlayerLost())
					{
						await GameOverLoop(token);
						canContinue = false;
						break;
					}
				}
			}
		}

		private async UniTask GameOverLoop(CancellationToken token)
		{
			Debug.Log("GameOverLoop Start");
			CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(token);

			inputController.OnRestartLevelEvent += RestartLevel;
			var result = await UniTask.WhenAny(
				gameOverController.Run(cts.Token),
				resultCompletionSource.Task);
			inputController.OnRestartLevelEvent -= RestartLevel;
			
			cts.Cancel();
			
			if (result.winArgumentIndex == 0)
			{
				switch (result.result1)
				{
					case GameOverCanvasView.RestartResult:
						resultCompletionSource?.TrySetResult(new RestartResult());
						break;
					case GameOverCanvasView.QuitResult:
						resultCompletionSource?.TrySetResult(new QuitResult());
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
			Debug.Log("GameOverLoop End");
		}

		private bool PlayerLost() => tilemapModel.DeadSheeps.Count > 0;

		private bool PlayerWon() => tilemapModel.DeadSheeps.Count == 0 && tilemapModel.SavedSheeps.Count > 0 && tilemapModel.ActiveSheeps.Count == 0;

	}

	public partial class LevelController
	{
		[Serializable] public abstract class Result { }
		[Serializable] public class QuitResult : Result { }
		[Serializable] public class WinResult : Result { }
		[Serializable] public class RestartResult : Result { }
	}
}

