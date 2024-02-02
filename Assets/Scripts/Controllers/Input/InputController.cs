using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputController : MonoBehaviour, InputMap.IGameplayActions
{
	public event Action OnLookUpEvent;
	public event Action OnLookDownEvent;
	public event Action OnLookLeftEvent;
	public event Action OnLookRightEvent;
	public event Action OnMoveRightEvent;
	public event Action OnMoveDownEvent;
	public event Action OnMoveUpEvent;
	public event Action OnMoveLeftEvent;
	public event Action OnWaitEvent;
	public event Action OnMenuEvent;
	public event Action OnUndoEvent;
	public event Action OnDoEvent;
	public event Action OnSkipLevelEvent;
	public event Action OnRestartLevelEvent;
	
	private InputMap inputMap;

	public void Initialize()
	{
		inputMap = new InputMap();
		inputMap.Gameplay.AddCallbacks(this);
		if (gameObject.activeInHierarchy)
		{
			inputMap.Enable();
		}
	}

	public void Terminate()
	{
		inputMap.Gameplay.RemoveCallbacks(this);
	}

	private void OnEnable()
	{
		inputMap?.Enable();
	}

	private void OnDisable()
	{
		inputMap?.Disable();
	}

	public void OnLookUp(InputAction.CallbackContext context) { if(context.performed) OnLookUpEvent?.Invoke();}
	public void OnLookDown(InputAction.CallbackContext context) { if(context.performed) OnLookDownEvent?.Invoke();}
	public void OnLookLeft(InputAction.CallbackContext context) { if(context.performed) OnLookLeftEvent?.Invoke();}
	public void OnLookRight(InputAction.CallbackContext context) { if(context.performed) OnLookRightEvent?.Invoke();}
	public void OnMoveRight(InputAction.CallbackContext context) { if(context.performed) OnMoveRightEvent?.Invoke();}
	public void OnMoveDown(InputAction.CallbackContext context) { if(context.performed) OnMoveDownEvent?.Invoke();}
	public void OnMoveUp(InputAction.CallbackContext context) { if(context.performed) OnMoveUpEvent?.Invoke();}
	public void OnMoveLeft(InputAction.CallbackContext context) { if(context.performed) OnMoveLeftEvent?.Invoke();}
	public void OnWait(InputAction.CallbackContext context) { if(context.performed) OnWaitEvent?.Invoke();}
	public void OnMenu(InputAction.CallbackContext context) { if(context.performed) OnMenuEvent?.Invoke();}
	public void OnUndo(InputAction.CallbackContext context) { if(context.performed) OnUndoEvent?.Invoke();}
	public void OnDo(InputAction.CallbackContext context) { if(context.performed) OnDoEvent?.Invoke();}
	public void OnSkipLevel(InputAction.CallbackContext context)  { if(context.performed) OnSkipLevelEvent?.Invoke();}
	public void OnRestart(InputAction.CallbackContext context)  { if(context.performed) OnRestartLevelEvent?.Invoke();}
}
