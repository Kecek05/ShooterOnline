using Sirenix.OdinInspector;
using UnityEngine;

namespace KeceK.Utils.Components
{
    public class GroundCheck : MonoBehaviour
    {
        [SerializeField] [Title("References")] [Required]
        private Transform _checkTransform;
        [SerializeField] [Title("Settings")] [Tooltip("Layers that is possible to jump")] 
        private LayerMask _jumpableLayers;
        [SerializeField] [Tooltip("Radius to check if the player is on ground")]
        private float _isGroundedRadius = 0.5f;
        
        public bool IsGrounded()
        {
            return Physics.OverlapSphere(
                _checkTransform.position,
                _isGroundedRadius,
                _jumpableLayers
            ) != null;
        }
        
#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            if (_checkTransform != null)
            {
                Gizmos.color = IsGrounded() ? Color.green : Color.red;
                Gizmos.DrawWireSphere(_checkTransform.position, _isGroundedRadius);
            }
        }
#endif
    }
}