using UnityEngine;
using KeceK.Game.Player.Interfaces;
using KeceK.Game.Player.MovementStrategies;
using KeceK.Game.ScriptableObjects;
using KeceK.Input;
using Sirenix.OdinInspector;
using System.Collections;

namespace KeceK.Game.Player
{
    /// <summary>
    /// Main FPS Controller that orchestrates all movement and camera systems
    /// Following Single Responsibility and Dependency Inversion Principles
    /// </summary>
    [RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
    public class FPSController : MonoBehaviour, IMovementController
    {
        [Title("References")]
        [SerializeField] [Required] private InputReaderSO _inputReader;
        [SerializeField] [Required] private MovementSettingsSO _movementSettings;
        [SerializeField] [Required] private CameraSettingsSO _cameraSettings;
        [SerializeField] [Required] private FirstPersonCameraController _cameraController;
        [SerializeField] [Required] private GroundDetector _groundDetector;
        
        [Title("Components")]
        [SerializeField] [Required] private Rigidbody _rigidbody;
        [SerializeField] [Required] private CapsuleCollider _capsuleCollider;
        
        // Movement strategy instances
        private IMovementStrategy _walkStrategy;
        private IMovementStrategy _sprintStrategy;
        private IMovementStrategy _crouchStrategy;
        private IMovementStrategy _currentStrategy;
        
        // State variables
        private bool _isSprinting;
        private bool _isCrouching;
        private bool _canJump;
        private float _lastJumpTime;
        private float _originalHeight;
        private Vector2 _currentMoveInput;
        
        // Properties
        public Vector3 CurrentVelocity => _rigidbody.linearVelocity;
        public bool IsGrounded => _groundDetector.IsGrounded;
        
        private void Awake()
        {
            InitializeComponents();
            InitializeStrategies();
            SetupCursor();
        }
        
        private void OnEnable()
        {
            SubscribeToInputEvents();
            _inputReader.EnableInput();
        }
        
        private void OnDisable()
        {
            UnsubscribeFromInputEvents();
            _inputReader.DisableInput();
        }
        
        private void Update()
        {
            _groundDetector.UpdateGroundDetection();
            UpdateJumpCooldown();
        }
        
        private void FixedUpdate()
        {
            HandleMovement(_currentMoveInput);
            ApplyGravity();
        }
        
        private void InitializeComponents()
        {
            if (_rigidbody == null)
                _rigidbody = GetComponent<Rigidbody>();
                
            if (_capsuleCollider == null)
                _capsuleCollider = GetComponent<CapsuleCollider>();
                
            _originalHeight = _capsuleCollider.height;
            
            // Configure rigidbody for FPS movement
            _rigidbody.freezeRotation = true;
            _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        }
        
        private void InitializeStrategies()
        {
            _walkStrategy = new WalkStrategy(_movementSettings, transform);
            _sprintStrategy = new SprintStrategy(_movementSettings, transform);
            _crouchStrategy = new CrouchStrategy(_movementSettings, transform);
            _currentStrategy = _walkStrategy;
        }
        
        private void SetupCursor()
        {
            if (_cameraSettings.CursorLocked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
        
        private void SubscribeToInputEvents()
        {
            _inputReader.OnMoveEvent += OnMoveInput;
            _inputReader.OnLookEvent += OnLookInput;
            _inputReader.OnJumpEvent += OnJumpInput;
            _inputReader.OnSprintEvent += OnSprintInput;
            _inputReader.OnCrouchEvent += OnCrouchInput;
        }
        
        private void UnsubscribeFromInputEvents()
        {
            _inputReader.OnMoveEvent -= OnMoveInput;
            _inputReader.OnLookEvent -= OnLookInput;
            _inputReader.OnJumpEvent -= OnJumpInput;
            _inputReader.OnSprintEvent -= OnSprintInput;
            _inputReader.OnCrouchEvent -= OnCrouchInput;
        }
        
        private void OnMoveInput(Vector2 moveInput)
        {
            _currentMoveInput = moveInput;
        }
        
        private void OnLookInput(Vector2 lookInput)
        {
            _cameraController.HandleLook(lookInput);
        }
        
        private void OnJumpInput()
        {
            Jump();
        }
        
        private void OnSprintInput(bool isSprinting)
        {
            SetMovementState(isSprinting, _isCrouching);
        }
        
        private void OnCrouchInput(bool isCrouching)
        {
            SetMovementState(_isSprinting, isCrouching);
        }
        
        public void HandleMovement(Vector2 moveInput)
        {
            Vector3 movementForce = _currentStrategy.CalculateMovement(
                moveInput, _rigidbody.linearVelocity, Time.fixedDeltaTime);
                
            // Apply air control if not grounded
            if (!IsGrounded && _movementSettings.AllowAirControl)
            {
                movementForce *= _movementSettings.AirControlMultiplier;
            }
            
            _rigidbody.AddForce(movementForce, ForceMode.VelocityChange);
        }
        
        public void Jump()
        {
            if (!_canJump || !IsGrounded)
                return;
                
            _rigidbody.AddForce(Vector3.up * _movementSettings.JumpForce, ForceMode.VelocityChange);
            _lastJumpTime = Time.time;
            _canJump = false;
        }
        
        public void SetMovementState(bool isSprinting, bool isCrouching)
        {
            // Can't sprint while crouching
            if (isCrouching)
                isSprinting = false;
                
            _isSprinting = isSprinting;
            _isCrouching = isCrouching;
            
            UpdateMovementStrategy();
            HandleCrouchHeight();
        }
        
        private void UpdateMovementStrategy()
        {
            if (_isCrouching)
                _currentStrategy = _crouchStrategy;
            else if (_isSprinting)
                _currentStrategy = _sprintStrategy;
            else
                _currentStrategy = _walkStrategy;
        }
        
        private void HandleCrouchHeight()
        {
            float targetHeight = _isCrouching ? _movementSettings.CrouchHeight : _originalHeight;
            StartCoroutine(SmoothHeightTransition(targetHeight));
        }
        
        private IEnumerator SmoothHeightTransition(float targetHeight)
        {
            float startHeight = _capsuleCollider.height;
            float elapsedTime = 0f;
            float transitionDuration = 1f / _movementSettings.CrouchTransitionSpeed;
            
            while (elapsedTime < transitionDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / transitionDuration;
                _capsuleCollider.height = Mathf.Lerp(startHeight, targetHeight, t);
                yield return null;
            }
            
            _capsuleCollider.height = targetHeight;
        }
        
        private void UpdateJumpCooldown()
        {
            if (!_canJump && Time.time - _lastJumpTime >= _movementSettings.JumpCooldown)
            {
                _canJump = true;
            }
        }
        
        private void ApplyGravity()
        {
            if (!IsGrounded)
            {
                Vector3 gravity = Physics.gravity * _movementSettings.GravityMultiplier;
                
                // Apply stronger gravity when falling
                if (_rigidbody.linearVelocity.y < 0)
                {
                    gravity *= _movementSettings.FallMultiplier;
                }
                
                _rigidbody.AddForce(gravity, ForceMode.Acceleration);
            }
        }
        
        private void OnValidate()
        {
            if (_rigidbody == null)
                _rigidbody = GetComponent<Rigidbody>();
                
            if (_capsuleCollider == null)
                _capsuleCollider = GetComponent<CapsuleCollider>();
        }
    }
}