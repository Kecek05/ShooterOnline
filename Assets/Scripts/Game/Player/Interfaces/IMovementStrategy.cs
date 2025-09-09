using UnityEngine;

namespace KeceK.Game.Player.Interfaces
{
    /// <summary>
    /// Interface for movement strategies following Strategy Pattern
    /// </summary>
    public interface IMovementStrategy
    {
        /// <summary>
        /// Calculate movement velocity based on input and current state
        /// </summary>
        /// <param name="moveInput">Normalized movement input</param>
        /// <param name="currentVelocity">Current rigidbody velocity</param>
        /// <param name="deltaTime">Time since last frame</param>
        /// <returns>Target velocity for this movement strategy</returns>
        Vector3 CalculateMovement(Vector2 moveInput, Vector3 currentVelocity, float deltaTime);
        
        /// <summary>
        /// Get the maximum speed for this movement strategy
        /// </summary>
        float MaxSpeed { get; }
        
        /// <summary>
        /// Get the acceleration factor for this movement strategy
        /// </summary>
        float Acceleration { get; }
    }
}