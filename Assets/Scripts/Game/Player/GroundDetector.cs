using UnityEngine;
using KeceK.Game.Player.Interfaces;
using KeceK.Game.ScriptableObjects;

namespace KeceK.Game.Player
{
    /// <summary>
    /// Ground detection system following Single Responsibility Principle
    /// </summary>
    public class GroundDetector : MonoBehaviour, IGroundDetector
    {
        [SerializeField] private MovementSettingsSO _movementSettings;
        [SerializeField] private Transform _groundCheckPoint;
        
        public bool IsGrounded { get; private set; }
        public Vector3 GroundNormal { get; private set; }
        public float DistanceToGround { get; private set; }
        
        private void Awake()
        {
            if (_groundCheckPoint == null)
                _groundCheckPoint = transform;
        }
        
        public void UpdateGroundDetection()
        {
            RaycastHit hit;
            Vector3 rayOrigin = _groundCheckPoint.position;
            
            // Perform sphere cast for more reliable ground detection
            IsGrounded = Physics.SphereCast(
                rayOrigin,
                _movementSettings.GroundCheckRadius,
                Vector3.down,
                out hit,
                _movementSettings.GroundCheckDistance,
                _movementSettings.GroundLayerMask
            );
            
            if (IsGrounded)
            {
                GroundNormal = hit.normal;
                DistanceToGround = hit.distance;
            }
            else
            {
                GroundNormal = Vector3.up;
                DistanceToGround = float.MaxValue;
            }
        }
        
        private void OnDrawGizmosSelected()
        {
            if (_movementSettings == null || _groundCheckPoint == null)
                return;
                
            Gizmos.color = IsGrounded ? Color.green : Color.red;
            Vector3 origin = _groundCheckPoint.position;
            Gizmos.DrawWireSphere(origin, _movementSettings.GroundCheckRadius);
            Gizmos.DrawWireSphere(origin + Vector3.down * _movementSettings.GroundCheckDistance, 
                                _movementSettings.GroundCheckRadius);
        }
    }
}