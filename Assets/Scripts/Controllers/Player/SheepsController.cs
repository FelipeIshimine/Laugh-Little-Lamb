using System.Threading;
using System.Threading.Tasks;
using Controllers.Commands;
using Controllers.Entities;
using Cysharp.Threading.Tasks;
using Models;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Controllers.Player
{
	internal class SheepsController : MonoBehaviour, IPlayer
	{
		[SerializeField]private UnityEngine.InputSystem.InputAction upInput;
		[SerializeField]private UnityEngine.InputSystem.InputAction downInput;
		[SerializeField]private UnityEngine.InputSystem.InputAction leftInput;
		[SerializeField]private UnityEngine.InputSystem.InputAction rightInput;
		private EntitiesController entitiesController;

		private TaskCompletionSource<Orientation> orientationTcs;
		private CommandsController commandsController;
		public void Initialize(EntitiesController entitiesController, CommandsController commandsController)
		{
			this.commandsController = commandsController;
			this.entitiesController = entitiesController;
		}

		private void Awake()
		{
			upInput.performed += OnUpInputOnperformed;
			downInput.performed += OnDownInputOnperformed;
			leftInput.performed += OnLeftInputOnperformed;
			rightInput.performed += OnRightInputOnperformed;
		}

		private void OnDestroy()
		{
			upInput.performed -= OnUpInputOnperformed;
			downInput.performed -= OnDownInputOnperformed;
			leftInput.performed -= OnLeftInputOnperformed;
			rightInput.performed -= OnRightInputOnperformed;            
		}

		private void OnDisable()
		{
			downInput.Disable();
			leftInput.Disable();
			rightInput.Disable();
			upInput.Disable();
		}

		private void OnEnable()
		{
			upInput.Enable();
            downInput.Enable();
			leftInput.Enable();
            rightInput.Enable();
		}

		private void SetResult(Orientation orientation)
		{
			Debug.Log(orientation);
			orientationTcs?.SetResult(orientation);
		}

		private void OnRightInputOnperformed(InputAction.CallbackContext inputAction)
		{
			if (inputAction.performed)
			{
				SetResult(Orientation.Right);
			}
		}

		private void OnLeftInputOnperformed(InputAction.CallbackContext inputAction)
		{
			if (inputAction.performed) SetResult(Orientation.Left);
		}

		private void OnDownInputOnperformed(InputAction.CallbackContext inputAction)
		{
			if (inputAction.performed)	SetResult(Orientation.Down);
		}

		private void OnUpInputOnperformed(InputAction.CallbackContext inputAction)
		{
			if (inputAction.performed) SetResult(Orientation.Up);
		}

		public async UniTask TakeTurnAsync(CancellationToken token)
		{
			commandsController.Do(new SheepTurnStart());
			
            gameObject.SetActive(true);
			Debug.Log("Player Turn Start");
			orientationTcs = new TaskCompletionSource<Orientation>();
			var result = await orientationTcs.Task;
			entitiesController.MoveTogether(entitiesController.SheepEntityModels,result);
			Debug.Log("Player Turn End");
            gameObject.SetActive(false);
            
            commandsController.Do(new SheepTurnEnd());
		}

	}
}