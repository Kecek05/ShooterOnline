using UnityEngine;
using Sirenix.OdinInspector;

namespace KeceK.Game.ScriptableObjects
{
    [CreateAssetMenu(fileName = "MovementSettingsSO", menuName = "Scriptable Objects/MovementSettingsSO")]
    public class MovementSettingsSO : ScriptableObject
    {
        [Title("Base Movement")]
        public float MaxSpeed = 6f;
        public AnimationCurve AccelerationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        public AnimationCurve DecelerationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        [Title("Sprint Settings")]
        public float SprintSpeedMultiplier = 1.6f;
        public float SprintAccelerationMultiplier = 1.2f;
        
        [Title("Crouch Settings")]
        public float CrouchSpeedMultiplier = 0.5f;
        public float CrouchHeight = 1f;
        public float CrouchTransitionSpeed = 5f;
        
        [Title("Jump Settings")]
        public float JumpForce = 8f;
        public float JumpCooldown = 0.2f;
        public bool AllowAirControl = true;
        
        [ShowIf("AllowAirControl")]
        public float AirControlMultiplier = 0.3f;
        
        [Title("Ground Detection")]
        public float GroundCheckDistance = 0.1f;
        public float GroundCheckRadius = 0.4f;
        public LayerMask GroundLayerMask = 1;
        
        [Title("Physics")]
        public float GravityMultiplier = 2f;
        public float FallMultiplier = 2.5f;
    }
}
