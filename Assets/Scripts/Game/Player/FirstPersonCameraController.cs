using UnityEngine;
using Unity.Cinemachine;
using KeceK.Game.Player.Interfaces;
using KeceK.Game.ScriptableObjects;
using Sirenix.OdinInspector;

namespace KeceK.Game.Player
{
    /// <summary>
    /// First-person camera controller using Cinemachine 3 following Single Responsibility Principle
    /// </summary>
    public class FirstPersonCameraController : MonoBehaviour, ICameraController
    {
        [Title("References")]
        [SerializeField] [Required] private CameraSettingsSO _cameraSettings;
        [SerializeField] [Required] private CinemachineCamera _virtualCamera;
        [SerializeField] [Required] private Transform _cameraTargetYaw;
        [SerializeField] [Required] private Transform _cameraTargetPitch;
        
        public Vector2 CurrentRotation { get; private set; }
        
        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;
        private bool _isEnabled = true;
        
        private void Awake()
        {
            // Initialize rotation values
            _cinemachineTargetYaw = _cameraTargetYaw.rotation.eulerAngles.y;
            _cinemachineTargetPitch = _cameraTargetPitch.rotation.eulerAngles.x;
            CurrentRotation = new Vector2(_cinemachineTargetYaw, _cinemachineTargetPitch);
        }
        
        public void HandleLook(Vector2 lookInput)
        {
            if (!_isEnabled || lookInput.sqrMagnitude < 0.01f)
                return;
                
            // Apply sensitivity and invert Y if needed
            float deltaTimeMultiplier = Time.deltaTime * 1000f;
            lookInput.x *= _cameraSettings.MouseSensitivityX * deltaTimeMultiplier;
            lookInput.y *= _cameraSettings.MouseSensitivityY * deltaTimeMultiplier;
            
            if (_cameraSettings.InvertMouseY)
                lookInput.y = -lookInput.y;
            
            // Update rotation values
            _cinemachineTargetYaw += lookInput.x;
            _cinemachineTargetPitch += lookInput.y;
            
            // Clamp vertical rotation
            _cinemachineTargetPitch = Mathf.Clamp(_cinemachineTargetPitch, 
                _cameraSettings.MinVerticalAngle, _cameraSettings.MaxVerticalAngle);
            
            // Apply rotation to camera targets
            if (_cameraSettings.UseSmoothRotation)
            {
                _cameraTargetYaw.rotation = Quaternion.Slerp(_cameraTargetYaw.rotation,
                    Quaternion.Euler(0.0f, _cinemachineTargetYaw, 0.0f),
                    _cameraSettings.RotationSmoothness * Time.deltaTime);
                    
                _cameraTargetPitch.rotation = Quaternion.Slerp(_cameraTargetPitch.rotation,
                    Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f),
                    _cameraSettings.RotationSmoothness * Time.deltaTime);
            }
            else
            {
                _cameraTargetYaw.rotation = Quaternion.Euler(0.0f, _cinemachineTargetYaw, 0.0f);
                _cameraTargetPitch.rotation = Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);
            }
            
            CurrentRotation = new Vector2(_cinemachineTargetYaw, _cinemachineTargetPitch);
        }
        
        public void SetSensitivity(float sensitivity)
        {
            _cameraSettings.MouseSensitivity = sensitivity;
        }
        
        public void SetEnabled(bool enabled)
        {
            _isEnabled = enabled;
        }
        
        private void OnValidate()
        {
            if (_cameraTargetYaw == null)
                _cameraTargetYaw = transform;
        }
    }
}