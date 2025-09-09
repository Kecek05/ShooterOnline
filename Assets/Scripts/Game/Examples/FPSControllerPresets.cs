using UnityEngine;
using KeceK.Game.ScriptableObjects;

namespace KeceK.Game.Examples
{
    /// <summary>
    /// Example configuration presets for FPS Movement Controller
    /// </summary>
    public static class FPSControllerPresets
    {
        public static MovementSettingsSO CreateRealisticMovementSettings()
        {
            var settings = ScriptableObject.CreateInstance<MovementSettingsSO>();
            
            // Realistic movement speeds (meters per second)
            settings.MaxSpeed = 4.5f; // Normal walking speed
            settings.SprintSpeedMultiplier = 2.2f; // Sprint ~10 m/s
            settings.CrouchSpeedMultiplier = 0.4f; // Slow crouch
            
            // Jump settings
            settings.JumpForce = 7f;
            settings.JumpCooldown = 0.3f;
            settings.AllowAirControl = true;
            settings.AirControlMultiplier = 0.2f;
            
            // Ground detection
            settings.GroundCheckDistance = 0.2f;
            settings.GroundCheckRadius = 0.4f;
            settings.GroundLayerMask = 1; // Default layer
            
            // Physics
            settings.GravityMultiplier = 2f;
            settings.FallMultiplier = 2.5f;
            
            // Crouch
            settings.CrouchHeight = 1f;
            settings.CrouchTransitionSpeed = 8f;
            
            return settings;
        }
        
        public static MovementSettingsSO CreateArcadeMovementSettings()
        {
            var settings = ScriptableObject.CreateInstance<MovementSettingsSO>();
            
            // Fast, arcade-style movement
            settings.MaxSpeed = 8f;
            settings.SprintSpeedMultiplier = 1.8f;
            settings.CrouchSpeedMultiplier = 0.6f;
            
            // High jump for arcade feel
            settings.JumpForce = 10f;
            settings.JumpCooldown = 0.1f;
            settings.AllowAirControl = true;
            settings.AirControlMultiplier = 0.8f;
            
            // Ground detection
            settings.GroundCheckDistance = 0.15f;
            settings.GroundCheckRadius = 0.3f;
            settings.GroundLayerMask = 1;
            
            // Lower gravity for arcade feel
            settings.GravityMultiplier = 1.5f;
            settings.FallMultiplier = 2f;
            
            // Quick crouch
            settings.CrouchHeight = 0.8f;
            settings.CrouchTransitionSpeed = 12f;
            
            return settings;
        }
        
        public static CameraSettingsSO CreateStandardCameraSettings()
        {
            var settings = ScriptableObject.CreateInstance<CameraSettingsSO>();
            
            settings.MouseSensitivity = 2f;
            settings.MouseSensitivityX = 2f;
            settings.MouseSensitivityY = 2f;
            
            settings.MinVerticalAngle = -80f;
            settings.MaxVerticalAngle = 80f;
            
            settings.UseSmoothRotation = false;
            settings.RotationSmoothness = 10f;
            
            settings.InvertMouseY = false;
            settings.CursorLocked = true;
            
            return settings;
        }
        
        public static CameraSettingsSO CreateSmoothCameraSettings()
        {
            var settings = CreateStandardCameraSettings();
            
            settings.UseSmoothRotation = true;
            settings.RotationSmoothness = 15f;
            settings.MouseSensitivity = 1.5f;
            settings.MouseSensitivityX = 1.5f;
            settings.MouseSensitivityY = 1.5f;
            
            return settings;
        }
    }
}