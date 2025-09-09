using UnityEngine;
using KeceK.Game.ScriptableObjects;

namespace KeceK.Game.Player.MovementStrategies
{
    /// <summary>
    /// Walking movement strategy
    /// </summary>
    public class WalkStrategy : BaseMovementStrategy
    {
        public override float MaxSpeed => _settings.MaxSpeed;
        public override float Acceleration => 1f;
        
        public WalkStrategy(MovementSettingsSO settings, Transform transform) 
            : base(settings, transform)
        {
        }
    }
}