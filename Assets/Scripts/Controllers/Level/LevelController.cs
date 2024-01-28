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
		
		[SerializeField] private GameOverController gameOverController;

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
			inputController.gameObject.SetActive(false);
			
			commandsController.Initialize();
		
			tilemapModel =	tilemapController.ProcessTileMaps();
			
			animationsController.Initialize(commandsController, entitiesController.ModelToView, tilemapController);
			entitiesController.Initialize(commandsController, tilemapModel, tilemapController);
		
			sheepsController.Initialize(entitiesController, commandsController, animationsController, inputController, ShowMenu, SkipLevel);
			enemyAiController.Initialize(entitiesController,tilemapModel, entitiesController.EnemyEntityModels, entitiesController.SheepEntityModels,tilemapController, commandsController);
			
			cameraController.Initialize(
				entitiesController.GetSheepViews().ToArray(),
				entitiesController.GetEnemyViews().ToArray(),
				entitiesController.GetDoorViews().ToArray());
		}

		private void SkipLevel() => resultCompletionSource?.TrySetResult(new WinResult());

		private void ShowMenu() => resultCompletionSource?.TrySetResult(new QuitResult());

		public void Terminate()
		{
			inputController.Terminate();
			commandsController.Terminate();
			cameraController.Terminate();
			
			animationsController.Terminate();
			//entitiesController.Terminate();
		
			sheepsController.Terminate();
			//enemyAiController.Terminate();
		}

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
					await player.TakeTurnAsync(token);
					while (animationsController.IsPlaying)
					{
						await Task.Yield();
					}

					if (PlayerWon())
					{
						resultCompletionSource?.TrySetResult(new WinResult());
						canContinue = false;
						break;
					}

					if (PlayerLost())
					{
						switch (await gameOverController.Run())
						{
							case GameOverController.RestartResult:
								resultCompletionSource?.TrySetResult(new RestartResult());
								break;
							case GameOverController.QuitResult:
								resultCompletionSource?.TrySetResult(new QuitResult());
								break;
							default:
								throw new ArgumentOutOfRangeException();
						}
						canContinue = false;
						break;
					}
				}
			}
		}

		private bool PlayerLost() => tilemapModel.ActiveSheeps.Count == 0;

		private bool PlayerWon() => tilemapModel.SavedSheeps.Count > 0 && tilemapModel.ActiveSheeps.Count == 0;

	}

	public partial class LevelController
	{
		[Serializable] public abstract class Result { }
		[Serializable] public class QuitResult : Result { }
		[Serializable] public class WinResult : Result { }
		[Serializable] public class RestartResult : Result { }
	}
}

