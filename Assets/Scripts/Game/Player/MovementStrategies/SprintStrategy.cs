using UnityEngine;
using KeceK.Game.ScriptableObjects;

namespace KeceK.Game.Player.MovementStrategies
{
    /// <summary>
    /// Sprinting movement strategy
    /// </summary>
    public class SprintStrategy : BaseMovementStrategy
    {
        public override float MaxSpeed => _settings.MaxSpeed * _settings.SprintSpeedMultiplier;
        public override float Acceleration => _settings.SprintAccelerationMultiplier;
        
        public SprintStrategy(MovementSettingsSO settings, Transform transform) 
            : base(settings, transform)
        {
        }
    }
}