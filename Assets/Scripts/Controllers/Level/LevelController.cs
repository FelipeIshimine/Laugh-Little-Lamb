using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Controllers.Entities;
using Controllers.Player;
using Cysharp.Threading.Tasks;
using Models;
using Sirenix.OdinInspector;
using UnityEngine;
using Views;

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
		[SerializeField] private AnimationSystem animationSystem;

		[SerializeField] private TilemapModel tilemapModel;

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
			
			entitiesController.Initialize(commandsController, tilemapModel, tilemapController,animationSystem);
		
			sheepsController.Initialize(entitiesController);
			enemyAiController.Initialize(entitiesController,tilemapModel, entitiesController.EnemyEntityModels, entitiesController.SheepEntityModels,tilemapController);
			
			cameraController.Initialize(
				entitiesController.GetSheepViews().ToArray(),
				entitiesController.GetEnemyViews().ToArray(),
				entitiesController.GetDoorViews().ToArray());
		}

		public void Terminate()
		{
			commandsController.Terminate();
			cameraController.Terminate();
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
					while (animationSystem.IsPlaying)
					{
						Debug.Log("Waiting for AnimationSystem");
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
