using System;
using KeceK.Game.ScriptableObjects;
using KeceK.Input;
using Sirenix.OdinInspector;
using UnityEngine;

namespace KeceK.Game
{
    public class CharacterMovement : MonoBehaviour
    {
        [Title("References")]
        [SerializeField] [Required] private InputReaderSO _inputReaderSO;
        [SerializeField] [Required] private Rigidbody _rigidbody;
        
        [Title("Settings")]
        [SerializeField] [Required] private MovementSettingsSO _movementSettingsSO;

        private void OnEnable()
        {
            _inputReaderSO.EnableInput();
        }
        
        private void OnDisable()
        {
            _inputReaderSO.DisableInput();
        }
        
        private void FixedUpdate()
        {
            HandleMovement();
        }
        
        private void HandleMovement()
        {
            Vector2 moveInput = _inputReaderSO.MoveInput;
            Vector3 inputDirection = new Vector3(moveInput.x, 0, moveInput.y).normalized;
            Vector3 moveDirection = transform.TransformDirection(inputDirection);

            Vector3 targetVelocity = moveDirection * _movementSettingsSO.MaxSpeed;
            Vector3 velocityChange = targetVelocity - _rigidbody.linearVelocity;
            velocityChange.y = 0; // We don't want to change the y velocity (gravity/jumping)

            // Apply acceleration or deceleration based on whether the player is trying to move
            float accelerationFactor = moveInput.magnitude > 0 ? 
                _movementSettingsSO.AccelerationCurve.Evaluate(velocityChange.magnitude / _movementSettingsSO.MaxSpeed) : 
                _movementSettingsSO.DecelerationCurve.Evaluate(velocityChange.magnitude / _movementSettingsSO.MaxSpeed);

            velocityChange *= accelerationFactor;

            _rigidbody.AddForce(velocityChange, ForceMode.VelocityChange);
        }
    }
}
