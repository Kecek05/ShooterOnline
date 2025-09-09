using Sirenix.OdinInspector;
using UnityEngine;

namespace KeceK.Utils.Components
{
    public class ShaderAnimatorTrigger : MonoBehaviour
    {
        [Title("References")] [SerializeField] [Required]
        private ShaderAnimator _shaderAnimator;

        [Button] [HideInEditorMode]
        public void StartAnimation(ShaderProperty property)
        {
            if (_shaderAnimator == null)
            {
                Debug.LogError("ShaderAnimator is not assigned.", this);
                return;
            }
            _shaderAnimator.StartAnimation(_shaderAnimator.GetAnimationDataByProperty(property));
        }
        
        [Button] [HideInEditorMode]
        public void StopAnimation(ShaderProperty property)
        {
            if (_shaderAnimator == null)
            {
                Debug.LogError("ShaderAnimator is not assigned.", this);
                return;
            }
            _shaderAnimator.StopAnimation(_shaderAnimator.GetAnimationDataByProperty(property));
        }
    }
}
