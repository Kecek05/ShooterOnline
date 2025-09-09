using System;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace KeceK.Utils.Components
{
    [Serializable]
    public class MoverDataPoints
    {
        [FoldoutGroup("$TargetPoint")][Required] [Tooltip("Point to Target to move to")]
        public Transform TargetPoint;
        [FoldoutGroup("$TargetPoint")] [Tooltip("Ease of the Point move. This will be used when going to this point.")]
        public Ease ThisPointEase = Ease.Linear;
        [FoldoutGroup("$TargetPoint")] [Tooltip("Duration of the Point move in seconds. This will be used when going to this point.")] [Unit(Units.Second)]
        public float ThisPointDurationOfTheMove = 0.3f;
    }
    
    public class Mover : MonoBehaviour
    {
        public event Action OnMoverFinishedMoving;
        
        [SerializeField] [Tooltip("If true, will show the CurrentPointNameDebug in the inspector")]
        private bool _isDebug = false;
        [ReadOnly] [ShowIf(nameof(_isDebug))]
        public string CurrentPointNameDebug;
        
        [SerializeField] [FoldoutGroup("References")] [Required] [Tooltip("Transform to move")]
        private Transform _target;
        [SerializeField] [FoldoutGroup("References")] [Required] [Tooltip("List of points to Target to move to.")] [ShowIf(nameof(_simpleMover))]
        private List<Transform> _pointsToMoveTo;
        
        [SerializeField] [FoldoutGroup("References")] [Required] [Tooltip("List of points to Target to move to")] [HideIf(nameof(_simpleMover))]
        private List<MoverDataPoints> _moverDataPointsToMoveTo;
        
        
        [SerializeField] [FoldoutGroup("Settings")] [Tooltip("If true, points will have a general settings, if false, each point will have its own settings")]
        private bool _simpleMover = true;
        [SerializeField] [FoldoutGroup("Settings")] [Tooltip("If true, the Target will teleport to the starting point before moving. If false, it will start moving from its current position.")]
        private bool _teleportToStartingPoint = true;
        [SerializeField] [Tooltip("Speed of the mover")] [FoldoutGroup("Settings")] [Unit(Units.Second)] [ShowIf(nameof(_simpleMover))]
        private float _durationOfTheMove = 0.3f;
        [SerializeField] [FoldoutGroup("Settings")] [ShowIf(nameof(_simpleMover))]
        private Ease _ease = Ease.Linear;
        
        [SerializeField] [FoldoutGroup("Settings")] [HideIf(nameof(_isCiclic))] [OnValueChanged(nameof(ValidateIsStopMoverIfReachedLastPoint))]
        [Tooltip("If true, the mover will stop when it reaches the last point. If false, it will continue its behavior based on the _isCiclic setting.")]
        private bool _stopMoverIfReachedLastPoint = false;
        [SerializeField] [FoldoutGroup("Settings")] [HideIf(nameof(_stopMoverIfReachedLastPoint))] [OnValueChanged(nameof(ValidateIsCiclic))]
        [Tooltip("If true, when the mover reaches the last point, it will return to the first point and continue moving in a loop. If false, it will tries to go back in the list of points")]
        private bool _isCiclic = true;
        
        private Tween _moveTween;
        private int _currentPointIndex = 0;
        private bool _canMove = true;
        private bool _goingFowardInTheList = true;
        

        private void ValidateIsStopMoverIfReachedLastPoint()
        {
            if (_stopMoverIfReachedLastPoint)
                _isCiclic = false;
        }
        
        private void ValidateIsCiclic()
        {
            if (_isCiclic)
                _stopMoverIfReachedLastPoint = false;
        }

        /// <summary>
        /// Called if the <see cref="_simpleMover"/> is true. Will use general settings for all points.
        /// </summary>
        /// <param name="onFinishedMoving"></param>
        private void SimpleMover(Action onFinishedMoving = null)
        {

            if (_isCiclic)
                _goingFowardInTheList = true;
            
            int indexToMoveTo = -1;

            if (_goingFowardInTheList)
                indexToMoveTo = _currentPointIndex + 1;
            else 
                indexToMoveTo = _currentPointIndex - 1;
            
            if(indexToMoveTo > _pointsToMoveTo.Count - 1)
            {
                //Reached the end of the points
                if (_isCiclic)
                {
                    indexToMoveTo = 0;
                }
                else
                {
                    if (_stopMoverIfReachedLastPoint)
                    {
                        _canMove = false;
                        return;
                    }
                    else
                    {
                        //Go back in the list
                        _goingFowardInTheList = false;
                        indexToMoveTo = _currentPointIndex - 1;
                    }
                }
            }
            else if (indexToMoveTo < 0)
            {
                //That means that isnt ciclic
                indexToMoveTo = _currentPointIndex + 1;
                
                _goingFowardInTheList = true;
            }
            
            Vector3 currentPosition = _pointsToMoveTo[_currentPointIndex].position;
            Vector3 positionToGo = _pointsToMoveTo[indexToMoveTo].position;
            
            if(_isDebug)
                CurrentPointNameDebug = _pointsToMoveTo[indexToMoveTo].name; 
            
            if(_teleportToStartingPoint)
                _target.position = currentPosition;
            
            _moveTween = _target.DOMove(positionToGo, _durationOfTheMove).SetEase(_ease).OnComplete(() =>
            {
                onFinishedMoving?.Invoke();
                OnMoverFinishedMoving?.Invoke();
            });
            
            _currentPointIndex = indexToMoveTo;
        }
        
        /// <summary>
        /// Called if the <see cref="_simpleMover"/> is false. Will use specific settings for points.
        /// </summary>
        /// <param name="onFinishedMoving"></param>
        private void ComplexMover(Action onFinishedMoving = null)
        {
            if (_isCiclic)
                _goingFowardInTheList = true;
            
            int indexToMoveTo = -1;

            if (_goingFowardInTheList)
                indexToMoveTo = _currentPointIndex + 1;
            else 
                indexToMoveTo = _currentPointIndex - 1;
            
            if(indexToMoveTo > _moverDataPointsToMoveTo.Count - 1)
            {
                //Reached the end of the points
                if (_isCiclic)
                {
                    indexToMoveTo = 0;
                }
                else
                {
                    if (_stopMoverIfReachedLastPoint)
                    {
                        _canMove = false;
                        return;
                    }
                    else
                    {
                        //Go back in the list
                        _goingFowardInTheList = false;
                        indexToMoveTo = _currentPointIndex - 1;
                    }
                }
            }
            else if (indexToMoveTo < 0)
            {
                //That means that isnt ciclic
                indexToMoveTo = _currentPointIndex + 1;
                
                _goingFowardInTheList = true;
            }
            
            Vector3 currentPosition = _moverDataPointsToMoveTo[_currentPointIndex].TargetPoint.position;
            
            if(_isDebug)
                CurrentPointNameDebug = _moverDataPointsToMoveTo[indexToMoveTo].TargetPoint.name;
            
            if(_teleportToStartingPoint)
                _target.position = currentPosition;
            
            _moveTween = _target.DOMove(_moverDataPointsToMoveTo[indexToMoveTo].TargetPoint.position, _moverDataPointsToMoveTo[indexToMoveTo].ThisPointDurationOfTheMove).SetEase(_moverDataPointsToMoveTo[indexToMoveTo].ThisPointEase).OnComplete(() =>
            {
                onFinishedMoving?.Invoke();
                OnMoverFinishedMoving?.Invoke();
            });
            
            _currentPointIndex = indexToMoveTo;
        }
        
        [Button]
        public void Move(Action onFinishedMoving = null)
        {
            _moveTween?.Kill();
            if (_pointsToMoveTo == null || _pointsToMoveTo.Count == 0)
            {
                Debug.LogWarning("No points to move to!");
                onFinishedMoving?.Invoke();
                OnMoverFinishedMoving?.Invoke();
                return;
            }
            
            if(_stopMoverIfReachedLastPoint && !_canMove) return;
            
            if (_simpleMover)
                SimpleMover(onFinishedMoving);
            else
                ComplexMover(onFinishedMoving);
        }
    }
}
