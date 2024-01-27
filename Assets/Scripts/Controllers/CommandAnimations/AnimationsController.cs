using System.Collections.Generic;
using Controllers.Commands;
using Models;
using UnityEngine;
using Views;

namespace Controllers.CommandAnimations
{
	public class AnimationsController : MonoBehaviour
	{
		[SerializeField] private AnimationQueueSystem animationQueueSystem;
		private CommandAnimationController[] commandAnimationControllers;
		public bool IsPlaying => animationQueueSystem.IsPlaying;

		public void Initialize(CommandsController commandsController,Dictionary<EntityModel, EntityView> modelToView, TilemapController tilemapController)
		{
			commandAnimationControllers = GetComponentsInChildren<CommandAnimationController>();
			foreach (CommandAnimationController commandAnimationController in commandAnimationControllers)
			{
				commandAnimationController.Initialize(commandsController, modelToView, animationQueueSystem, tilemapController);
			}
		}

		public void Terminate()
		{
			foreach (CommandAnimationController commandAnimationController in commandAnimationControllers)
			{
				commandAnimationController.Terminate();
			}
		}

	}
}