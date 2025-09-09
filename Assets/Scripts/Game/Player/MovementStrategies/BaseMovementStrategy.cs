using UnityEngine;
using KeceK.Game.Player.Interfaces;
using KeceK.Game.ScriptableObjects;

namespace KeceK.Game.Player.MovementStrategies
{
    /// <summary>
    /// Base abstract class for movement strategies following Strategy Pattern
    /// </summary>
    public abstract class BaseMovementStrategy : IMovementStrategy
    {
        protected MovementSettingsSO _settings;
        protected Transform _transform;
        
        public abstract float MaxSpeed { get; }
        public abstract float Acceleration { get; }
        
        protected BaseMovementStrategy(MovementSettingsSO settings, Transform transform)
        {
            _settings = settings;
            _transform = transform;
        }
        
        public virtual Vector3 CalculateMovement(Vector2 moveInput, Vector3 currentVelocity, float deltaTime)
        {
            if (moveInput.magnitude < 0.1f)
                return CalculateDeceleration(currentVelocity, deltaTime);
                
            return CalculateAcceleration(moveInput, currentVelocity, deltaTime);
        }
        
        protected virtual Vector3 CalculateAcceleration(Vector2 moveInput, Vector3 currentVelocity, float deltaTime)
        {
            // Convert input to world space relative to player transform
            Vector3 moveDirection = _transform.TransformDirection(new Vector3(moveInput.x, 0, moveInput.y)).normalized;
            Vector3 targetVelocity = moveDirection * MaxSpeed;
            Vector3 velocityChange = targetVelocity - new Vector3(currentVelocity.x, 0, currentVelocity.z);
            
            float accelerationFactor = _settings.AccelerationCurve.Evaluate(
                velocityChange.magnitude / MaxSpeed) * Acceleration;
                
            return velocityChange.normalized * accelerationFactor * deltaTime;
        }
        
        protected virtual Vector3 CalculateDeceleration(Vector3 currentVelocity, float deltaTime)
        {
            Vector3 horizontalVelocity = new Vector3(currentVelocity.x, 0, currentVelocity.z);
            float decelerationFactor = _settings.DecelerationCurve.Evaluate(
                horizontalVelocity.magnitude / MaxSpeed);
                
            return -horizontalVelocity.normalized * decelerationFactor * MaxSpeed * deltaTime;
        }
    }
}