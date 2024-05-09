using UnityEngine;
using UnityEngine.InputSystem;
using static PlayerInputActions;

public class InputReader : ScriptableObject, IPlayerActions {

	public Vector2 Move => inputActions.Player.Move.ReadValue<Vector2>();

	PlayerInputActions inputActions;

	void OnEnable() {
		if (inputActions == null) {
			inputActions = new PlayerInputActions();
			inputActions.Player.SetCallbacks(this);
		}
	}

	public void Enable() {
		inputActions.Enable();
	}

    public void OnMove(InputAction.CallbackContext context)
    {
        // noop
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        // noop
    }
}
