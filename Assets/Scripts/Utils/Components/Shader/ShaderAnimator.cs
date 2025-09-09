using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace KeceK.Utils.Components
{
    /// <summary>
    /// Use this component to animate any shader property over time or trigger.
    /// </summary>
    public class ShaderAnimator : MonoBehaviour
    {
        
        [SerializeField] [Title("Settings")]
        private List<ShaderAnimationData> _shaderAnimations = new();

        private void Awake()
        {
            _shaderAnimations.ForEach(shaderAnimation => shaderAnimation.Initialize());
        }

        private void OnEnable()
        {
            _shaderAnimations.ForEach(shaderAnimation =>
            {
                if (shaderAnimation.StartAnimationOnStart)
                {
                    StartCoroutine(DelayToStartAnimationLoop(shaderAnimation));
                }
            });
        }

        private void OnDisable()
        {
            _shaderAnimations.ForEach(StopAnimation);
        }

        private IEnumerator DelayToStartAnimationLoop(ShaderAnimationData animationData)
        {
            yield return new WaitForSeconds(animationData.GetPropertyStartDelay());
            StartAnimation(animationData);
        }

        /// <summary>
        /// Call this to start the animation. If the loop is already running, it will restart it.
        /// </summary>
        /// <param name="animationData"> wich animation should Start</param>
        public void StartAnimation(ShaderAnimationData animationData)
        {
            animationData.ResetAnimationToInitialState();
            CancelAnimationCoroutine(animationData);
            if (animationData.IsALoop)
                animationData.SetAnimationLoopCoroutine(StartCoroutine(animationData.AnimationLoop()));
            else
                animationData.DoAnimation();
            
        }
        
        /// <summary>
        /// Call this to stop the animation. This will also reset the property value to start value.
        /// </summary>
        /// <param name="animationData"> Wich animation should Start</param>
        public void StopAnimation(ShaderAnimationData animationData)
        {
            animationData.ResetAnimationToInitialState();
            CancelAnimationCoroutine(animationData);
        }
            
        /// <summary>
        /// Called by <see cref="StartShineLoop"/> and <see cref="StopAnimation"/> to cancel the current shine coroutine and reset the shine value.
        /// </summary>
        private void CancelAnimationCoroutine(ShaderAnimationData animationData)
        {
            if (animationData.AnimationLoopCoroutine != null)
                StopCoroutine(animationData.AnimationLoopCoroutine);
        }
        
        public ShaderAnimationData GetAnimationDataByProperty(ShaderProperty property)
        {
            return _shaderAnimations.Find(animation => animation.ShaderProperty == property);
        }
        
    }
}
