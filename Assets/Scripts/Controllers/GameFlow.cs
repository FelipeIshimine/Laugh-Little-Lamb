using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Controllers
{
	public class GameFlow : MonoBehaviour
	{
		[SerializeField] private AssetReference levelScene;
		private void Awake()
		{
			DontDestroyOnLoad(gameObject);
		}

		private void Start()
		{
			GoToLevel();
		}

		private async void GoToLevel()
		{
			await Addressables.LoadSceneAsync(levelScene).Task;
		
			var levelController = FindFirstObjectByType<LevelController>();

		}
	}
}
