using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Controllers
{
	public class MainMenuController : MonoBehaviour
	{
		[SerializeField] private Button quitBtn; 
		[SerializeField] private Button playBtn;
		
		private UniTaskCompletionSource<Result> resultCompletionSource;
		
		public async UniTask<Result> Run()
		{
			Debug.Log($"{this}.Run()");
			playBtn.onClick.AddListener(Play);
			quitBtn.onClick.AddListener(Quit);
			resultCompletionSource = new UniTaskCompletionSource<Result>();
			var result = await resultCompletionSource.Task;
			playBtn.onClick.RemoveListener(Play);
			quitBtn.onClick.RemoveListener(Quit);
			//Debug.Log($"{this}.Run()");
			return result;
		}

		private void Play() => resultCompletionSource?.TrySetResult(new PlayResult());

		private void Quit() => resultCompletionSource?.TrySetResult(new QuitResult());


		public abstract class Result { }
		public class PlayResult : Result { }
		public class QuitResult : Result { }
		
	}
}