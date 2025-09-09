using UnityEngine;
using KeceK.Game.Player.Interfaces;
using KeceK.Game.Player.MovementStrategies;
using KeceK.Game.ScriptableObjects;
using NUnit.Framework;

namespace KeceK.Game.Tests
{
    /// <summary>
    /// Unit tests for the FPS Movement Controller
    /// </summary>
    public class FPSMovementControllerTests
    {
        private MovementSettingsSO _testSettings;
        private Transform _testTransform;
        
        [SetUp]
        public void Setup()
        {
            // Create test objects
            GameObject testObject = new GameObject("Test Player");
            _testTransform = testObject.transform;
            
            _testSettings = ScriptableObject.CreateInstance<MovementSettingsSO>();
            _testSettings.MaxSpeed = 6f;
            _testSettings.SprintSpeedMultiplier = 1.6f;
            _testSettings.CrouchSpeedMultiplier = 0.5f;
        }
        
        [Test]
        public void WalkStrategy_HasCorrectMaxSpeed()
        {
            // Arrange
            var walkStrategy = new WalkStrategy(_testSettings, _testTransform);
            
            // Act & Assert
            Assert.AreEqual(_testSettings.MaxSpeed, walkStrategy.MaxSpeed);
        }
        
        [Test]
        public void SprintStrategy_HasCorrectMaxSpeed()
        {
            // Arrange
            var sprintStrategy = new SprintStrategy(_testSettings, _testTransform);
            
            // Act & Assert
            Assert.AreEqual(_testSettings.MaxSpeed * _testSettings.SprintSpeedMultiplier, sprintStrategy.MaxSpeed);
        }
        
        [Test]
        public void CrouchStrategy_HasCorrectMaxSpeed()
        {
            // Arrange
            var crouchStrategy = new CrouchStrategy(_testSettings, _testTransform);
            
            // Act & Assert
            Assert.AreEqual(_testSettings.MaxSpeed * _testSettings.CrouchSpeedMultiplier, crouchStrategy.MaxSpeed);
        }
        
        [Test]
        public void MovementStrategy_CalculatesMovement_WithValidInput()
        {
            // Arrange
            var walkStrategy = new WalkStrategy(_testSettings, _testTransform);
            Vector2 moveInput = Vector2.right;
            Vector3 currentVelocity = Vector3.zero;
            float deltaTime = 0.02f;
            
            // Act
            Vector3 result = walkStrategy.CalculateMovement(moveInput, currentVelocity, deltaTime);
            
            // Assert
            Assert.IsTrue(result.magnitude > 0, "Movement calculation should return non-zero result for valid input");
        }
        
        [Test]
        public void MovementStrategy_ReturnsDeceleration_WithZeroInput()
        {
            // Arrange
            var walkStrategy = new WalkStrategy(_testSettings, _testTransform);
            Vector2 moveInput = Vector2.zero;
            Vector3 currentVelocity = new Vector3(5f, 0f, 0f);
            float deltaTime = 0.02f;
            
            // Act
            Vector3 result = walkStrategy.CalculateMovement(moveInput, currentVelocity, deltaTime);
            
            // Assert
            Assert.IsTrue(result.x < 0, "Should apply deceleration when no input is provided");
        }
        
        [TearDown]
        public void TearDown()
        {
            // Clean up test objects
            if (_testTransform != null)
            {
                Object.DestroyImmediate(_testTransform.gameObject);
            }
            
            if (_testSettings != null)
            {
                Object.DestroyImmediate(_testSettings);
            }
        }
    }
}