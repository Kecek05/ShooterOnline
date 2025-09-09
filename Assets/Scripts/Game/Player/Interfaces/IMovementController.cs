using UnityEngine;

namespace KeceK.Game.Player.Interfaces
{
    /// <summary>
    /// Interface for movement control systems following Single Responsibility Principle
    /// </summary>
    public interface IMovementController
    {
        /// <summary>
        /// Process movement input and apply movement forces
        /// </summary>
        /// <param name="moveInput">Normalized movement input from input system</param>
        void HandleMovement(Vector2 moveInput);
        
        /// <summary>
        /// Handle jumping action
        /// </summary>
        void Jump();
        
        /// <summary>
        /// Set movement state (walking, sprinting, crouching)
        /// </summary>
        /// <param name="isSprinting">Is the player sprinting</param>
        /// <param name="isCrouching">Is the player crouching</param>
        void SetMovementState(bool isSprinting, bool isCrouching);
        
        /// <summary>
        /// Get current movement velocity
        /// </summary>
        Vector3 CurrentVelocity { get; }
        
        /// <summary>
        /// Check if player is grounded
        /// </summary>
        bool IsGrounded { get; }
    }
}