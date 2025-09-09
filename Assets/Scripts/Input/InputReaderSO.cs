using UnityEngine;
using UnityEngine.InputSystem;

namespace KeceK.Input
{
    [CreateAssetMenu(fileName = "InputReaderSO", menuName = "Scriptable Objects/InputReaderSO")]
    public class InputReaderSO : ScriptableObject, PlayerInputActions.IPlayerActions
    {
        public Vector2 MoveInput => _inputActions.Player.Move.ReadValue<Vector2>();
        
        private PlayerInputActions _inputActions;
        private void OnEnable()
        {
            if (_inputActions == null)
            {
                _inputActions = new PlayerInputActions();
                _inputActions.Player.SetCallbacks(this);
            }
            _inputActions.Enable();
        }
        
        public void EnableInput() => _inputActions.Enable();
        public void DisableInput() => _inputActions.Disable();

        public void OnMove(InputAction.CallbackContext context)
        {
            //not implemented yet
        }

        public void OnLook(InputAction.CallbackContext context)
        {
            //not implemented yet
        }

        public void OnAttack(InputAction.CallbackContext context)
        {
            //not implemented yet
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            //not implemented yet
        }

        public void OnCrouch(InputAction.CallbackContext context)
        {
            //not implemented yet
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            //not implemented yet
        }

        public void OnPrevious(InputAction.CallbackContext context)
        {
            //not implemented yet
        }

        public void OnNext(InputAction.CallbackContext context)
        {
            //not implemented yet
        }

        public void OnSprint(InputAction.CallbackContext context)
        {
            //not implemented yet
        }
    }
}
