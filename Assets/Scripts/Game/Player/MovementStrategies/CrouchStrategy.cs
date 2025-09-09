using UnityEngine;
using KeceK.Game.ScriptableObjects;

namespace KeceK.Game.Player.MovementStrategies
{
    /// <summary>
    /// Crouching movement strategy
    /// </summary>
    public class CrouchStrategy : BaseMovementStrategy
    {
        public override float MaxSpeed => _settings.MaxSpeed * _settings.CrouchSpeedMultiplier;
        public override float Acceleration => 0.8f; // Slightly slower acceleration when crouching
        
        public CrouchStrategy(MovementSettingsSO settings, Transform transform) 
            : base(settings, transform)
        {
        }
    }
}