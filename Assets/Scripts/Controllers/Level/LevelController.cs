using System;
using System.Threading.Tasks;
using Controllers.Entities;
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
		
		[SerializeField] private PlayerController playerController;
		[SerializeField] private EnemyAiController enemyAiController;
		
		[SerializeField] private EntitiesController entitiesController;
		[SerializeField] private AnimationSystem animationSystem;

		[SerializeField] private TilemapModel tilemapModel;

		private async UniTaskVoid Awake()
		{
			Initialize(null);
			await GameLoop();
		}

		[Button]
		public void Initialize(Action<Result> callback)
		{
			resultCallback = callback;
			
			commandsController.Initialize();
		
			tilemapModel =	tilemapController.ProcessTileMaps();
			
			entitiesController.Initialize(commandsController, tilemapModel, tilemapController,animationSystem);
		
			playerController.Initialize(entitiesController);
		}

		public void Terminate()
		{
			commandsController.Terminate();
		}

		public async UniTask GameLoop()
		{
			while (true)
			{
				if (gameObject == null)
				{
					break;
				}

				await playerController.TakeTurnAsync();

				while (animationSystem.IsPlaying)
				{
					Debug.Log("Waiting for AnimationSystem");
					await Task.Yield();
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
