using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace KeceK.Utils.Components
{
    [Serializable]
    public class ShaderAnimationData
    {
    
        /// <summary>
        /// OnAnimationFinished event is triggered when the animation is finished.
        /// Passes the ShaderAnimationData as an argument.
        /// </summary>
        public event Action<ShaderAnimationData> OnAnimationFinished;
        
        
        [InfoBox("All random values should be changed out of play mode")]
        [SerializeField] [FoldoutGroup("$_shaderProperty")] 
        [Tooltip("Name of the shader property to animate. This should match the name in the shader. This is UNIQUE for each component.")]
        private ShaderProperty _shaderProperty;
        [Space(10f)]
        
        [FoldoutGroup("$_shaderProperty")] [Title("References")]
        [SerializeField] [Required] [Tooltip("List of sprite renderers that will have this animation effect. All simultaneously.")]
        private List<SpriteRenderer> _spriteRenderers;
        [Space(10f)]

        [Title("Animation Settings")]
        [SerializeField] [FoldoutGroup("$_shaderProperty")] [OnValueChanged(nameof(IsALoopValidate))]
        [Tooltip("If true, the animation will loop indefinitely. If false, it will play once and stop.")]
        private bool _isALoop = false;
        
        [SerializeField] [FoldoutGroup("$_shaderProperty")]
        private Ease _shaderEase = Ease.Linear;
        [Space(10f)]
            
        [Title("Delay Between Loops Settings")]
        [SerializeField] [FoldoutGroup("$_shaderProperty")] [ShowIf(nameof(_isALoop))]
        private bool _isRandomDelayBetweenLoops;
        
        [SerializeField] [ShowIf(nameof(DelayBetweenLoopsValidate))] [FoldoutGroup("$_shaderProperty")]
        private float _delayBetweenLoops = 0f;
        
        [SerializeField] [MinMaxSlider(0, 10)] [ShowIf(nameof(_isRandomDelayBetweenLoops))] [FoldoutGroup("$_shaderProperty")]
        private Vector2 _randomDelayBetweenLoops = new Vector2(1f, 3f);
        [Space(5f)]
        
        [Title("Duration Settings")]
        [SerializeField] [FoldoutGroup("$_shaderProperty")]
        private bool _isRandomDuration;
        
        [SerializeField] [HideIf(nameof(_isRandomDuration))] [FoldoutGroup("$_shaderProperty")]
        private float _animationDuration = 1f;
        
        [SerializeField] [MinMaxSlider(0,30)] [ShowIf(nameof(_isRandomDuration))] [FoldoutGroup("$_shaderProperty")]
        private Vector2 _randomAnimationDuration = new Vector2(1f, 5f);
        [Space(5f)]
        
        [Title("Start Value Settings")]
        [SerializeField] [FoldoutGroup("$_shaderProperty")]
        private bool _isRandomStartValue;
        
        [SerializeField] [HideIf(nameof(_isRandomStartValue))] [FoldoutGroup("$_shaderProperty")]
        private float _startValue = 0f;
        
        [SerializeField] [MinMaxSlider(0, 1)] [ShowIf(nameof(_isRandomStartValue))] [FoldoutGroup("$_shaderProperty")]
        private Vector2 _randomStartValue = new Vector2(0f, 0.5f);
        [Space(5f)]
            
        [Title("End Value Settings")]
        [SerializeField] [FoldoutGroup("$_shaderProperty")]
        private bool _isRandomEndValue;
        
        [SerializeField] [HideIf(nameof(_isRandomEndValue))] [FoldoutGroup("$_shaderProperty")]
        private float _endValue = 1f;
        
        [SerializeField] [MinMaxSlider(0, 1)] [ShowIf(nameof(_isRandomEndValue))] [FoldoutGroup("$_shaderProperty")]
        private Vector2 _randomEndValue = new Vector2(0f, 0.7f);
        [Space(10f)]
            
        [Title("Start Animation On Start Settings")]
        [SerializeField] [FoldoutGroup("$_shaderProperty")] [OnValueChanged(nameof(StartAnimationOnStartValidate))]
        private bool _startAnimationOnStart = true;
        
        [SerializeField] [FoldoutGroup("$_shaderProperty")] [ShowIf(nameof(_startAnimationOnStart))]
        private bool _isRandomStartAnimationOnStart;
        
        [SerializeField] [ShowIf(nameof(_isRandomStartAnimationOnStart))] [FoldoutGroup("$_shaderProperty")]
        [MinMaxSlider(0, 10)]
        private Vector2 _randomStartDelay = new Vector2();
        
        [SerializeField] [ShowIf(nameof(ShouldShowStartDelayValidate))] [FoldoutGroup("$_shaderProperty")]
        private float _startDelay = 0f;
        
        
            
        private float _shaderPropertyValue = 0f;
        private bool _animationFinished = true;
        private Tween _animationTween;
        private Coroutine _animationLoopCoroutine;
        private List<Material> _materials = new();
        private WaitForSeconds _animationLoopWait => new WaitForSeconds(_isRandomStartAnimationOnStart ? MathK.GetRandomFloatByRange(_randomStartDelay) : _startDelay);

            
        //Randomized values
        private float _randomizedEndValue;
        private float _randomizedDuration;
        private float _randomizedStartValue;
        private float _randomizedStartDelay;

        private bool _alreadyRandomized = false;
            
        //Publics
        public bool StartAnimationOnStart => _startAnimationOnStart;
        public Coroutine AnimationLoopCoroutine => _animationLoopCoroutine;
        public Tween AnimationTween => _animationTween;
        
        public ShaderProperty ShaderProperty => _shaderProperty;
        
        public bool IsALoop => _isALoop;

        private bool ShouldShowStartDelayValidate()
        {
            return _startAnimationOnStart && !_isRandomStartAnimationOnStart;
        }
        
        private void StartAnimationOnStartValidate()
        {
            if (!_startAnimationOnStart)
                _isRandomStartAnimationOnStart = false;
        }

        private void IsALoopValidate()
        {
            if (!_isALoop)
                _isRandomDelayBetweenLoops = false;
        }

        private bool DelayBetweenLoopsValidate()
        {
            return _isALoop && !_isRandomDelayBetweenLoops;
        }

        public void Initialize()
        {
            InitializeMaterials();
            InitializeRandomValues();
        }
            
        private void InitializeRandomValues()
        {
            if (_alreadyRandomized) return;
                
            _randomizedEndValue = MathK.GetRandomFloatByRange(_randomEndValue);
            _randomizedDuration = MathK.GetRandomFloatByRange(_randomAnimationDuration);
            _randomizedStartValue = MathK.GetRandomFloatByRange(_randomStartValue);
            _randomizedStartDelay = MathK.GetRandomFloatByRange(_randomStartDelay);
                
            _alreadyRandomized = true;
        }
            
        private void InitializeMaterials()
        {
            _spriteRenderers.ForEach(spriteRenderer => _materials.Add(spriteRenderer.material));
        }
            
        /// <summary>
        /// Do the animation once.
        /// </summary>
        [Button] [HideInEditorMode]
        public void DoAnimation()
        {
            _animationFinished = false;
            _animationTween?.Kill(false);
            _shaderPropertyValue = GetPropertyStartValue();
            _animationTween = DOTween.To(
                    () => _shaderPropertyValue, 
                    x => _shaderPropertyValue = x, 
                    GetPropertyEndValue(), 
                    GetPropertyDuration())
                .SetEase(_shaderEase)
                .OnUpdate(() =>
                {
                    _materials.ForEach(material => material.SetFloat(UtilsK.GetShaderProperty(_shaderProperty), _shaderPropertyValue));
                }).OnComplete(() =>
                {
                    _animationFinished = true;
                    OnAnimationFinished?.Invoke(this);
                });
        }
            
        /// <summary>
        /// Reset all materials value and reset the ShaderPropertyValue to the initial value.
        /// </summary>
        private void ResetShaderValue()
        {
            _materials.ForEach(material => material.SetFloat(UtilsK.GetShaderProperty(_shaderProperty), GetPropertyStartValue()));
            _shaderPropertyValue = GetPropertyStartValue();
        }
            
        /// <summary>
        /// Do the shine loop while _shouldLoop is true.
        /// </summary>
        /// <returns></returns>
        public IEnumerator AnimationLoop()
        {
            while (_isALoop)
            {
                if (_animationFinished)
                {
                    yield return  _animationLoopWait;
                    ResetShaderValue();
                    DoAnimation();
                }
                else
                    yield return null;
                
            }
        }

        /// <summary>
        /// Call this to reset the animation to initial state. This will also reset the property value to start value.
        /// </summary>
        public void ResetAnimationToInitialState()
        {
            SetAnimationLoopCoroutine(null);
            AnimationTween?.Kill(false);
            ResetShaderValue();
            _animationFinished = true;
        }

        public float GetPropertyStartValue()
        {
            return _isRandomStartValue ? _randomizedStartValue : _startValue;
        }
            
        public float GetPropertyEndValue()
        {
            return _isRandomEndValue ? _randomizedEndValue : _endValue;
        }
            
        public float GetPropertyDuration()
        {
            return _isRandomDuration ? _randomizedDuration : _animationDuration;
        }
            
        public float GetPropertyStartDelay()
        {
            return _isRandomStartAnimationOnStart ? _randomizedStartDelay : _startDelay;
        }
            
        public void SetAnimationFinished(bool loopFinished)
        {
            _animationFinished = loopFinished;
        }
        
        public void SetAnimationLoopCoroutine(Coroutine coroutine)
        {
            _animationLoopCoroutine = coroutine;
        }
    }
}