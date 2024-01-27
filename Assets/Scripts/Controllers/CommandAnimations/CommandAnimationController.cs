﻿using System.Collections.Generic;
using Controllers.Commands;
using Models;
using UnityEngine;
using Views;

namespace Controllers.CommandAnimations
{
	public abstract class CommandAnimationController : MonoBehaviour
	{
		protected Dictionary<EntityModel, EntityView> ModelToView;
		protected CommandsController CommandsController;
		protected AnimationQueueSystem AnimationsController;
		protected TilemapController TilemapController;
		
		public void Initialize(CommandsController commandsController,
		                               Dictionary<EntityModel, EntityView> modelToView,
		                               AnimationQueueSystem animationQueueSystem,
		                               TilemapController tilemapController)
		{
			this.TilemapController = tilemapController;
			this.AnimationsController = animationQueueSystem;
			this.CommandsController = commandsController;
			this.ModelToView = modelToView;
			Initialize();
		}

		public abstract void Terminate();

		protected abstract void Initialize();
	}
	
}