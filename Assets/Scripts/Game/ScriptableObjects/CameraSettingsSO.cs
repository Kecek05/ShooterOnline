using Sirenix.OdinInspector;
using UnityEngine;

namespace KeceK.Game.ScriptableObjects
{
    [CreateAssetMenu(fileName = "CameraSettingsSO", menuName = "Scriptable Objects/CameraSettingsSO")]
    public class CameraSettingsSO : ScriptableObject
    {
        [Title("Sensitivity")]
        public float AxisXMultiplier = 1f;
        public float AxisYMultiplier = 1f;
        
        [Title("Angle Limits")]
        [MinMaxSlider(-90, 90, true)]
        public Vector2 MinMaxYAngle = new Vector2(-75f, 75f);
    }
}
