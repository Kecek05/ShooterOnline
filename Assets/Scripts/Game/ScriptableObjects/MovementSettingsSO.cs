using Sirenix.OdinInspector;
using UnityEngine;

namespace KeceK.Game.ScriptableObjects
{
    [CreateAssetMenu(fileName = "MovementSettingsSO", menuName = "Scriptable Objects/MovementSettingsSO")]
    public class MovementSettingsSO : ScriptableObject
    {
        [Title("Walk Settings")]
        public float WalkMaxSpeed = 10f;
        public float WalkMoveSpeed = 5f;
        [Tooltip("Always normalized between 0 and 1")]
        public AnimationCurve WalkAccelerationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [Tooltip("Always normalized between 0 and 1")]
        public AnimationCurve WalkDecelerationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        [Title("Run Settings")]
        public float RunMaxSpeed = 15f;
        public float RunMoveSpeed = 5f;
        
        [Title("Jump Settings")]
        public float JumpForce = 5f;
    }
}
