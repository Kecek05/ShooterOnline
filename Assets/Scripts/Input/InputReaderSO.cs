using UnityEngine;
using UnityEngine.InputSystem;
using System;

namespace KeceK.Input
{
    [CreateAssetMenu(fileName = "InputReaderSO", menuName = "Scriptable Objects/InputReaderSO")]
    public class InputReaderSO : ScriptableObject, PlayerInputActions.IPlayerActions
    {
        public Vector2 MoveInput => _inputActions.Player.Move.ReadValue<Vector2>();
        public Vector2 LookInput => _inputActions.Player.Look.ReadValue<Vector2>();
        
        // Events following Observer Pattern
        public event Action<Vector2> OnMoveEvent;
        public event Action<Vector2> OnLookEvent;
        public event Action OnJumpEvent;
        public event Action<bool> OnSprintEvent;
        public event Action<bool> OnCrouchEvent;
        public event Action OnAttackEvent;
        public event Action OnInteractEvent;
        
        private PlayerInputActions _inputActions;
        
        private void OnEnable()
        {
            if (_inputActions == null)
            {
                _inputActions = new PlayerInputActions();
                _inputActions.Player.SetCallbacks(this);
            }
        }
        
        public void EnableInput()
        {
            _inputActions.Enable();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        
        public void DisableInput()
        {
            _inputActions.Disable();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            OnMoveEvent?.Invoke(context.ReadValue<Vector2>());
        }

        public void OnLook(InputAction.CallbackContext context)
        {
            OnLookEvent?.Invoke(context.ReadValue<Vector2>());
        }

        public void OnAttack(InputAction.CallbackContext context)
        {
            if (context.performed)
                OnAttackEvent?.Invoke();
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            if (context.performed)
                OnInteractEvent?.Invoke();
        }

        public void OnCrouch(InputAction.CallbackContext context)
        {
            OnCrouchEvent?.Invoke(context.performed);
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.performed)
                OnJumpEvent?.Invoke();
        }

        public void OnPrevious(InputAction.CallbackContext context)
        {
            // Weapon switching - implement later if needed
        }

        public void OnNext(InputAction.CallbackContext context)
        {
            // Weapon switching - implement later if needed
        }

        public void OnSprint(InputAction.CallbackContext context)
        {
            OnSprintEvent?.Invoke(context.performed);
        }
    }
}
