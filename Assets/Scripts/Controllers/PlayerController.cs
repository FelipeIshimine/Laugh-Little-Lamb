using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Models;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Controllers
{
	internal class PlayerController : MonoBehaviour
	{
		[SerializeField]private UnityEngine.InputSystem.InputAction upInput;
		[SerializeField]private UnityEngine.InputSystem.InputAction downInput;
		[SerializeField]private UnityEngine.InputSystem.InputAction leftInput;
		[SerializeField]private UnityEngine.InputSystem.InputAction rightInput;
		private EntitiesController entitiesController;

		private TaskCompletionSource<Orientation> orientationTcs;

		public void Initialize(EntitiesController entitiesController)
		{
			this.entitiesController = entitiesController;
		}

		private void Awake()
		{
			upInput.performed += OnUpInputOnperformed;
			downInput.performed += OnDownInputOnperformed;
			leftInput.performed += OnLeftInputOnperformed;
			rightInput.performed += OnRightInputOnperformed;
            
			upInput.Enable();
            downInput.Enable();
			leftInput.Enable();
            rightInput.Enable();
		}

		private void OnDestroy()
		{
			upInput.performed -= OnUpInputOnperformed;
			downInput.performed -= OnDownInputOnperformed;
			leftInput.performed -= OnLeftInputOnperformed;
			rightInput.performed -= OnRightInputOnperformed;            
			upInput.Disable();
			downInput.Disable();
			leftInput.Disable();
			rightInput.Disable();
		}

		private void SetResult(Orientation orientation)
		{
			Debug.Log(orientation);
			orientationTcs?.SetResult(orientation);
			orientationTcs = null;
		}

		private void OnRightInputOnperformed(InputAction.CallbackContext _) => SetResult(Orientation.Right);

		private void OnLeftInputOnperformed(InputAction.CallbackContext _) => SetResult(Orientation.Left);

		private void OnDownInputOnperformed(InputAction.CallbackContext _) => SetResult(Orientation.Down);

		private void OnUpInputOnperformed(InputAction.CallbackContext _) => SetResult(Orientation.Up);

		public async UniTask TakeTurnAsync()
		{
			Debug.Log("Player Turn Start");
			orientationTcs = new TaskCompletionSource<Orientation>();
			var result = await orientationTcs.Task;
			entitiesController.MovePlayers(result);
			Debug.Log("Player Turn End");
		}
	}
}