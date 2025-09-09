using UnityEngine;

namespace KeceK.Game.Player.Interfaces
{
    /// <summary>
    /// Interface for ground detection systems following Single Responsibility Principle
    /// </summary>
    public interface IGroundDetector
    {
        /// <summary>
        /// Check if the player is currently grounded
        /// </summary>
        bool IsGrounded { get; }
        
        /// <summary>
        /// Get the ground normal vector
        /// </summary>
        Vector3 GroundNormal { get; }
        
        /// <summary>
        /// Get the distance to ground
        /// </summary>
        float DistanceToGround { get; }
        
        /// <summary>
        /// Update ground detection (should be called every frame)
        /// </summary>
        void UpdateGroundDetection();
    }
}