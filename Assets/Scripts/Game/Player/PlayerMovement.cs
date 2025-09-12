using System;
using System.Collections;
using DG.Tweening;
using KeceK.Game.ScriptableObjects;
using KeceK.Input;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KeceK.Game
{
    public class PlayerMovement : MonoBehaviour
    {
        [Title("References")]
        [SerializeField] [Required] private InputReaderSO _inputReaderSO;
        [SerializeField] [Required] private Rigidbody _rigidbody;
        [SerializeField] [Required] private Transform _orientation;
        
        [Title("Settings")]
        [SerializeField] [Required] private MovementSettingsSO _movementSettingsSO;

        
        private Vector3 _moveDirection;
        private float _evaluatedAccelerationSpeed;
        private float _evaluatedDecelerationSpeed;
        private float _movingSpeed;
        
        
        private Coroutine _moveCoroutine;
        private Coroutine _stopMovingCoroutine;

        private bool _canMove = true;
        
        //DEBUG
        public float EvaluatedAccelerationSpeed => _evaluatedAccelerationSpeed;
        
        public float EvaluatedDecelerationSpeed => _evaluatedDecelerationSpeed;
        public Vector3 MoveDirection => _moveDirection;
        
        public float MovingSpeed => _movingSpeed;
            
        private void OnEnable()
        {
            _inputReaderSO.EnableInput();
            _inputReaderSO.OnMoveInputContext += InputReaderSO_OnOnMoveInputContext;
        }

        private void OnDisable()
        {
            _inputReaderSO.DisableInput();
            _inputReaderSO.OnMoveInputContext -= InputReaderSO_OnOnMoveInputContext;
        }
        
        private void InputReaderSO_OnOnMoveInputContext(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                ResetAllMovingCoroutines();

                _moveCoroutine = StartCoroutine(MoveCoroutine());
                
            } else if (context.canceled)
            {
                ResetAllMovingCoroutines();
                
                _stopMovingCoroutine = StartCoroutine(StopMovingCoroutine());
                
            }
        }
        
        private IEnumerator StopMovingCoroutine()
        {
            float currentDecelerationTime = 0f;
            Debug.Log("Starting Stop moving coroutine");
            while (_canMove)
            {
                currentDecelerationTime += Time.deltaTime;
                _moveDirection = _orientation.forward * _inputReaderSO.MoveInput.y + _orientation.right * _inputReaderSO.MoveInput.x;
                
                _evaluatedDecelerationSpeed = _movementSettingsSO.WalkDecelerationCurve.Evaluate(currentDecelerationTime);
                _evaluatedDecelerationSpeed = Mathf.Clamp(_evaluatedDecelerationSpeed, 0f, 1f);
                
                _movingSpeed = _evaluatedDecelerationSpeed * _movementSettingsSO.WalkMoveSpeed;
                
                // if(_rigidbody.linearVelocity.magnitude < _movementSettingsSO.WalkMaxSpeed) s
                _rigidbody.AddForce(-(_moveDirection.normalized * _movingSpeed * Time.deltaTime), ForceMode.VelocityChange);
                
                yield return null;
            }
        }

        private IEnumerator MoveCoroutine()
        {
            float currentAccelerationTime = 0f;
            Debug.Log("Starting moving coroutine");
            while (_canMove)
            {
                currentAccelerationTime += Time.deltaTime;
                _moveDirection = _orientation.forward * _inputReaderSO.MoveInput.y + _orientation.right * _inputReaderSO.MoveInput.x;
                
                _evaluatedAccelerationSpeed = _movementSettingsSO.WalkAccelerationCurve.Evaluate(currentAccelerationTime);
                _evaluatedAccelerationSpeed = Mathf.Clamp(_evaluatedAccelerationSpeed, 0f, 1f);
                
                _movingSpeed = _evaluatedAccelerationSpeed * _movementSettingsSO.WalkMoveSpeed;
                
                // if(_rigidbody.linearVelocity.magnitude < _movementSettingsSO.WalkMaxSpeed) 
                
                _rigidbody.AddForce(_moveDirection.normalized * _movingSpeed * Time.deltaTime, ForceMode.VelocityChange);

                if (_rigidbody.linearVelocity.magnitude >= _movementSettingsSO.WalkMaxSpeed)
                {
                    Vector3 limitedVelocity = _rigidbody.linearVelocity.normalized * _movementSettingsSO.WalkMaxSpeed;
                    _rigidbody.linearVelocity = limitedVelocity;
                }
                
                yield return null;
            }
        }

        private void ResetAllMovingCoroutines()
        {
            if(_stopMovingCoroutine != null)
                StopCoroutine(_stopMovingCoroutine);
            
            if(_moveCoroutine != null)
                StopCoroutine(_moveCoroutine);
        }

        // private void Update()
        // {
        //     if (_inputReaderSO.MoveInput != Vector2.zero)
        //     {
        //         //Moving 
        //         _isMoving = true;
        //     }
        //     else
        //     {
        //         _isMoving = false;
        //     }
        // }
        //
        // private void FixedUpdate()
        // {
        //     if (_isMoving)
        //     {
        //         _moveDirection = _orientation.forward * _inputReaderSO.MoveInput.y + _orientation.right * _inputReaderSO.MoveInput.x;
        //         
        //         _rigidbody.AddForce(_moveDirection.normalized * _movementSettingsSO.MaxSpeed * Time.deltaTime, ForceMode.Force);
        //     }
        // }


        //TODO Refactor Movement Code
        public void HandleMovement()
        {
            // if (_moveContext.canceled)
            // {
            //     //TODO : Add a deceleration curve to smoothly stop the player
            //     _rigidbody.linearVelocity = Vector3.zero;
            //     Debug.Log("Stop Movement Context");
            //     return;
            // }
            //
            // if (_moveContext.performed)
            // {
            //     _moveDirection = _orientation.forward * _inputReaderSO.MoveInput.y + _orientation.right * _inputReaderSO.MoveInput.x;
            //     
            //     _evaluatedSpeed = _movementSettingsSO.AccelerationCurve.Evaluate((float)_moveContext.duration);
            //     
            //     _currentSpeed = _evaluatedSpeed * _movementSettingsSO.MaxSpeed;
            //     
            //     Debug.Log($"Evaluated Speed : {_evaluatedSpeed} | Current Speed : {_currentSpeed} | Duration : {_moveContext.duration} | Move Input : {_inputReaderSO.MoveInput} | Move Direction : {_moveDirection}");
            //     _currentSpeed = Mathf.Clamp(_currentSpeed, 0f, _movementSettingsSO.MaxSpeed);
            //     
            //     _rigidbody.AddForce(_moveDirection.normalized * _currentSpeed, ForceMode.VelocityChange);
            // }



            // if (_inputReaderSO.MoveInput == Vector2.zero)
            // {
            //
            // }
            //
            // _moveDirection = _orientation.forward * _inputReaderSO.MoveInput.y + _orientation.right * _inputReaderSO.MoveInput.x;
            //
            // _rigidbody.AddForce(_moveDirection.normalized * _movementSettingsSO.MaxSpeed * Time.deltaTime, ForceMode.VelocityChange);
            
            // Vector2 moveInput = _inputReaderSO.MoveInput;
            //
            // Vector3 inputDirection = new Vector3(moveInput.x, 0, moveInput.y).normalized;
            // Vector3 moveDirection = transform.TransformDirection(inputDirection);
            //
            // Vector3 targetVelocity = moveDirection * _movementSettingsSO.MaxSpeed;
            // Vector3 velocityChange = targetVelocity - _rigidbody.linearVelocity;
            // velocityChange.y = 0; // We don't want to change the y velocity (gravity/jumping)
            //
            // // Apply acceleration or deceleration based on whether the player is trying to move
            // float accelerationFactor = moveInput.magnitude > 0 ? 
            //     _movementSettingsSO.AccelerationCurve.Evaluate(velocityChange.magnitude / _movementSettingsSO.MaxSpeed) : 
            //     _movementSettingsSO.DecelerationCurve.Evaluate(velocityChange.magnitude / _movementSettingsSO.MaxSpeed);
            //
            // velocityChange *= accelerationFactor;
            //
            // _rigidbody.AddForce(velocityChange * Time.deltaTime, ForceMode.VelocityChange);
        }
    }
}
