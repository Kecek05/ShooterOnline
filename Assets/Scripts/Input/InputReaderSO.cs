using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KeceK.Input
{
    [CreateAssetMenu(fileName = "InputReaderSO", menuName = "Scriptable Objects/InputReaderSO")]
    public class InputReaderSO : ScriptableObject, PlayerInputActions.IPlayerActions
    {
        public Vector2 MoveInput => _inputActions.Player.Move.ReadValue<Vector2>();
        public Vector2 LookInput => _inputActions.Player.Look.ReadValue<Vector2>();
        
        public event Action<InputAction.CallbackContext> OnMoveInputContext;
        
        public event Action OnJumpPressed; 
        
        private PlayerInputActions _inputActions;
        private void OnEnable()
        {
            if (_inputActions == null)
            {
                _inputActions = new PlayerInputActions();
                _inputActions.Player.SetCallbacks(this);
            }
        }
        
        public void EnableInput() => _inputActions.Enable();
        public void DisableInput() => _inputActions.Disable();

        public void OnMove(InputAction.CallbackContext context)
        {
            OnMoveInputContext?.Invoke(context);
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
            OnJumpPressed?.Invoke();
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
