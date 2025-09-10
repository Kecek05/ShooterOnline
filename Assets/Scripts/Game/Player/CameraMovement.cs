using System;
using System.Runtime.CompilerServices;
using KeceK.Game.ScriptableObjects;
using KeceK.Input;
using Sirenix.OdinInspector;
using Unity.Cinemachine;
using UnityEngine;

namespace KeceK.Game
{
    public class CameraMovement : MonoBehaviour
    {
        [Title("References")]
        [SerializeField] [Required] private CameraSettingsSO _cameraSettingsSO;

        [SerializeField] [Required] private InputReaderSO _inputReaderSO;
        [SerializeField] [Required] private CinemachineCamera _cinemachineCamera;

        private float _desiredYAngle;

        public void HandleCameraMovement()
        {
            Vector2 lookInput = _inputReaderSO.LookInput;
            if (lookInput.sqrMagnitude < 0.01f) return;
            
            float mouseXRotation = lookInput.x * _cameraSettingsSO.AxisXMultiplier;
            
            float deltaTime = Time.deltaTime;
            
            _desiredYAngle -= lookInput.y * _cameraSettingsSO.AxisYMultiplier;
            _desiredYAngle = Mathf.Clamp(_desiredYAngle, _cameraSettingsSO.MinMaxYAngle.x, _cameraSettingsSO.MinMaxYAngle.y);
            
            transform.Rotate(0, mouseXRotation, 0);
            _cinemachineCamera.transform.localRotation = Quaternion.Euler(_desiredYAngle, 0, 0);
            
        }
    }
}
