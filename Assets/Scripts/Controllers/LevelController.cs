using Model;
using Models;
using UnityEngine;
using View;

namespace Controllers
{
	public class LevelController : MonoBehaviour
	{
		[SerializeField] private LevelView levelView;
		[SerializeField] private TilemapModel tilemap;
        
		public LevelController(LevelView levelView)
		{
			this.levelView = levelView;
		}
	}
}
