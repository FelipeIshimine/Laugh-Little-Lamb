using System.Threading.Tasks;
using Models;
using UnityEngine;

namespace Controllers
{
	public class EntityController : MonoBehaviour
	{
		private TilemapController tilemapController;
		public EntityModel EntityModel { get; private set; }
		//public IAsyncProvider<InputRequest> AsyncProvider { get; set; }

		private CommandsController commandsController;
		public void Initialize(TilemapController tilemapController, CommandsController commandsController, Vector2Int startCoordinate)
		{
			this.tilemapController = tilemapController;
			this.commandsController = commandsController;
			EntityModel = new EntityModel(startCoordinate, Orientation.Right);
			//AsyncProvider = asyncProvider;
		}
	}


	public class MoveCommand : ICommand
	{
		private readonly EntityController entityController;
		private readonly InputRequest inputRequest;

		public MoveCommand(EntityController entityController, InputRequest inputRequest)
		{
			this.inputRequest = inputRequest;
		}

		public void Do()
		{
		}

		public void Undo()
		{
		}
	}
	
	public class InputRequest
	{
		public Orientation MoveDirection;
		public Orientation LookDirection;
	}
}