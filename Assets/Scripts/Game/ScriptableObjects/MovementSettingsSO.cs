using UnityEngine;

namespace KeceK.Game.ScriptableObjects
{
    [CreateAssetMenu(fileName = "MovementSettingsSO", menuName = "Scriptable Objects/MovementSettingsSO")]
    public class MovementSettingsSO : ScriptableObject
    {
        public float MaxSpeed = 10f;
        public float MaxRunSpeed = 15f;
        public AnimationCurve AccelerationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        public AnimationCurve DecelerationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        public float JumpForce = 5f;
    }
}
