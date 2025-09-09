using UnityEngine;
using Sirenix.OdinInspector;

namespace KeceK.Game.ScriptableObjects
{
    [CreateAssetMenu(fileName = "CameraSettingsSO", menuName = "Scriptable Objects/CameraSettingsSO")]
    public class CameraSettingsSO : ScriptableObject
    {
        [Title("Mouse Sensitivity")]
        [Range(0.1f, 10f)]
        public float MouseSensitivity = 2f;
        
        [Range(0.1f, 5f)]
        public float MouseSensitivityX = 2f;
        
        [Range(0.1f, 5f)]
        public float MouseSensitivityY = 2f;
        
        [Title("Camera Constraints")]
        [Range(-90f, 0f)]
        public float MinVerticalAngle = -80f;
        
        [Range(0f, 90f)]
        public float MaxVerticalAngle = 80f;
        
        [Title("Camera Smoothing")]
        public bool UseSmoothRotation = false;
        
        [ShowIf("UseSmoothRotation")]
        [Range(1f, 20f)]
        public float RotationSmoothness = 10f;
        
        [Title("Mouse Settings")]
        public bool InvertMouseY = false;
        public bool CursorLocked = true;
    }
}