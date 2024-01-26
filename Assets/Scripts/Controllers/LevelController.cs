using System;
using System.Threading.Tasks;
using Model;
using Models;
using UnityEngine;
using View;

namespace Controllers
{
	public partial class LevelController : MonoBehaviour
	{
		private Action<Result> resultCallback;
		
		[SerializeField] private TilemapController tilemapController;
		[SerializeField] private InputController inputController;
		[SerializeField] private CommandsController commandsController;

		public void Initialize(Action<Result> callback)
		{
			resultCallback = callback;
			
			commandsController.Initialize();
			
			tilemapController.Initialize();
		}

		public void Terminate()
		{
			commandsController.Terminate();
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
