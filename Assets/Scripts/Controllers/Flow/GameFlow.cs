using System.Threading.Tasks;
using Controllers.Level;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Controllers
{
	public class GameFlow : MonoBehaviour
	{
		[SerializeField] private AssetReference levelScene;
		[SerializeField] private AssetReference mainMenu;
		private void Awake()
		{
			DontDestroyOnLoad(gameObject);
		}

		private void Start()
		{
			GoToMenu();
		}

		private async UniTask GoToMenu()
		{
			await Addressables.LoadSceneAsync(mainMenu).Task;

			//MainMenuController mainMenuController = FindFirstObjectByType<MainMenuController>();

		}

		private async UniTask GoToLevel()
		{
			await Addressables.LoadSceneAsync(levelScene).Task;
		
			var levelController = FindFirstObjectByType<LevelController>();

		}
	}
}
