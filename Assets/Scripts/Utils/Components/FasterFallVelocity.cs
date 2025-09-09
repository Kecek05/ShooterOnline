using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace KeceK.Utils.Components
{
    /// <summary>
    /// Component added to an object to make it fall faster. Requires an Rigidbody in the same object.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class FasterFallVelocity : MonoBehaviour
    {
        [SerializeField] [Required] [Title("References")]
        private Rigidbody _rigidbody;
        [Space(5f)]
        
        [Title("Settings")]
        [SerializeField] [OnValueChanged(nameof(ResetHaveStoppingDistance))]
        private bool _isEnabled = true;
        [SerializeField] [ShowIf(nameof(_isEnabled))]
        private bool _haveStoppingDistance;
        [Space(2f)]
        [SerializeField] [Tooltip("Force that will be applied to the RB when starts falling every tick")] [ShowIf(nameof(_isEnabled))]
        private float _fallForce = 10f;
        [SerializeField] [Tooltip("The max force that the RB can get added by the fall force")] [ShowIf(nameof(_isEnabled))]
        private float _maxFallRigidbodyVelocityY = 10f;
        [SerializeField] [Tooltip("Minimum force to start adding negative force to Y")] [ShowIf(nameof(_isEnabled))]
        private float velocityYThreshold = 0.1f;
        [Space(1f)]

        [SerializeField] [FoldoutGroup("Stopping Distance Settings")] [Tooltip("Transforms to start the raycast")] 
        [ShowIf(nameof(_isEnabled))] [ShowIf(nameof(_haveStoppingDistance))]
        private List<Transform> _stoppingDistanceStartTransforms = new();
        
        [SerializeField] [FoldoutGroup("Stopping Distance Settings")] [Tooltip("Layers to stop adding negative Y velocity when getting close")] 
        [ShowIf(nameof(_isEnabled))] [ShowIf(nameof(_haveStoppingDistance))]
        private LayerMask _stoppingDistanceLayerMask;
        
        [SerializeField] [FoldoutGroup("Stopping Distance Settings")] [Tooltip("Layers to stop adding negative Y velocity when getting close")] 
        [ShowIf(nameof(_isEnabled))] [ShowIf(nameof(_haveStoppingDistance))]
        private float _rayLength = 2.5f;
        
        //Cache
        private RaycastHit2D[] _raycastHits; 
        private bool _shouldStopAddingForce = false;
        
        private void OnValidate()
        {
            if (_rigidbody == null)
            {
                _rigidbody = GetComponent<Rigidbody>();
            }
        }

        private void FixedUpdate()
        {
            if (!_isEnabled) return;
            
            if(ShouldStopAddingForce()) return;
                
            if (_rigidbody.linearVelocity.y < velocityYThreshold)
            {
                Vector3 newForce = new Vector3(_rigidbody.linearVelocity.x, -_fallForce, _rigidbody.linearVelocity.z);
                _rigidbody.AddForce(newForce, mode: ForceMode.Impulse);
            }
        }
        
        private bool ShouldStopAddingForce()
        {
            if (!_haveStoppingDistance) return false;

            _shouldStopAddingForce = IsAtMaxFallForce() || TooCloseToGround();
            return _shouldStopAddingForce;
        }

        private bool IsAtMaxFallForce()
        {
            return Mathf.Abs(_rigidbody.linearVelocity.y) >= _maxFallRigidbodyVelocityY;
        }
        
        private bool TooCloseToGround()
        {
            if (!_haveStoppingDistance) return false;
            
            foreach (Transform stoppingTransform in _stoppingDistanceStartTransforms)
            {
                _raycastHits = Physics2D.RaycastAll(
                    stoppingTransform.position, 
                    Vector2.down, _rayLength, 
                    _stoppingDistanceLayerMask);

                if (!RaycastHitOnlyItself(_raycastHits))
                {
                    if(_raycastHits.Length > 0)
                        return true;
                }
            }
            return false;
        }

        private bool RaycastHitOnlyItself(RaycastHit2D[] rayCastHit2Ds)
        {
            if (rayCastHit2Ds.Length == 0) return false;
            
            bool hitOnlyItself = true;
            
            foreach (RaycastHit2D hit2D in rayCastHit2Ds)
            {
                if (hit2D.collider.gameObject != gameObject)
                {
                    hitOnlyItself = false;
                }
            }
            return hitOnlyItself;
        }
        
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if(!_haveStoppingDistance) return;
            
            foreach (Transform stoppingTransform in _stoppingDistanceStartTransforms)
            {
                
                RaycastHit2D[] raycast = Physics2D.RaycastAll(
                    stoppingTransform.position, 
                    Vector2.down, _rayLength, 
                    _stoppingDistanceLayerMask);
                
                if (RaycastHitOnlyItself(raycast))
                {
                    Debug.DrawRay(
                        stoppingTransform.position,
                        Vector2.down * _rayLength,
                        Color.blue
                    );
                    continue;
                }
                
                if (raycast.Length > 0)
                {
                    Debug.DrawRay(
                        stoppingTransform.position,
                        Vector2.down * _rayLength,
                        Color.red
                    );
                }
                else
                {
                    Debug.DrawRay(
                        stoppingTransform.position,
                        Vector2.down * _rayLength,
                        Color.blue
                    );
                }
            }
        }
#endif
        private void ResetHaveStoppingDistance()
        {
            _haveStoppingDistance = false;
        }
        
        public void SetIsEnabled(bool isEnabled)
        {
            _isEnabled = isEnabled;
        }
    }
}
