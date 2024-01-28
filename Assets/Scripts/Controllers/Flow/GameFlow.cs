using System;
using System.Threading.Tasks;
using Controllers.Level;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace Controllers
{
	public class GameFlow : MonoBehaviour
	{
		[SerializeField] private WinController winController;
		public static GameFlow Instance { get; set; }
		[SerializeField] private AssetReferenceGameObject[] tilemapPrefabs;
		[SerializeField] private AssetReference levelScene;
		[SerializeField] private AssetReference mainMenu;

		private void Awake()
		{
			Instance = this;
			DontDestroyOnLoad(gameObject);
		}

		private async UniTaskVoid Start()
		{
		MainMenu:
			int levelIndex = 0;
			MainMenuController.Result menuResult = await RunMenu();

			Debug.Log($"MenuResult:{menuResult}");
			if (menuResult is MainMenuController.QuitResult)
			{
				goto Quit;
			}
			
			if (menuResult is MainMenuController.PlayResult)
			{
			Level:
				LevelController.Result levelResult = await RunLevel(levelIndex);
				
				if (levelResult is LevelController.QuitResult)
				{
					goto MainMenu;
				}

				if (levelResult is LevelController.WinResult)
				{
					levelIndex++;
					if (levelIndex == tilemapPrefabs.Length)
					{
						switch (await winController.Run())
						{
							case WinController.QuitResult:
								goto MainMenu;
							case WinController.RestartResult:
								levelIndex = 0;
								goto Level;
							default:
								throw new ArgumentOutOfRangeException();
						}		
					}
					goto Level;
				}

				if (levelResult is LevelController.RestartResult)
				{
					goto Level;
				}
			}
			
		Quit:
			Application.Quit();
		}

		private async UniTask<MainMenuController.Result> RunMenu()
		{
			var sceneHandler = Addressables.LoadSceneAsync(mainMenu, LoadSceneMode.Additive);
			await sceneHandler.Task;
			
			MainMenuController mainMenuController = FindFirstObjectByType<MainMenuController>();
			var result = await mainMenuController.Run();
			await Addressables.UnloadSceneAsync(sceneHandler).Task;

			return result;
		}

		private async UniTask<LevelController.Result> RunLevel(int levelIndex)
		{
			Debug.Log($"RunLevel:{levelIndex}");
			var levelSceneHandle = Addressables.LoadSceneAsync(levelScene, LoadSceneMode.Additive);

			var tilemapHandler = tilemapPrefabs[levelIndex].InstantiateAsync();

			//Debug.Log($"Loading Level");
			await levelSceneHandle.Task;
			//Debug.Log($"Loading Tilemap");
			await tilemapHandler.Task;

			var tilemapGo = tilemapHandler.Result;

			var terrainTilemap = tilemapGo.transform.GetChild(0).GetComponent<Tilemap>();
			var entitiesTilemap = tilemapGo.transform.GetChild(1).GetComponent<Tilemap>();
			
			var levelController = FindFirstObjectByType<LevelController>();
			
			//Debug.Log($"Running Level");
			var result = await levelController.Run(terrainTilemap,entitiesTilemap);

			await Addressables.UnloadSceneAsync(levelSceneHandle).Task;
			Addressables.ReleaseInstance(tilemapHandler);

			return result;
		}
	}
}
