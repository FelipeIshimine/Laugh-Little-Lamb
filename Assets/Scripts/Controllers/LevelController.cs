using Model;
using Models;
using UnityEngine;
using View;

namespace Controllers
{
	public class LevelController : MonoBehaviour
	{
		private LevelView levelView;
		private BoardModel board;
        
		public LevelController(LevelView levelView)
		{
			this.levelView = levelView;
			board = new BoardModel();
			
			Debug.Log($"Board ID:{board.Index}");
		}
	}
}
