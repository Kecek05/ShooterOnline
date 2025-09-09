using UnityEngine;

namespace KeceK.Game.Player.Interfaces
{
    /// <summary>
    /// Interface for camera control systems following Single Responsibility Principle
    /// </summary>
    public interface ICameraController
    {
        /// <summary>
        /// Process mouse look input and apply camera rotation
        /// </summary>
        /// <param name="lookInput">Mouse delta input from input system</param>
        void HandleLook(Vector2 lookInput);
        
        /// <summary>
        /// Set camera sensitivity
        /// </summary>
        /// <param name="sensitivity">Mouse sensitivity multiplier</param>
        void SetSensitivity(float sensitivity);
        
        /// <summary>
        /// Enable or disable camera control
        /// </summary>
        /// <param name="enabled">Should camera control be enabled</param>
        void SetEnabled(bool enabled);
        
        /// <summary>
        /// Get current camera rotation
        /// </summary>
        Vector2 CurrentRotation { get; }
    }
}