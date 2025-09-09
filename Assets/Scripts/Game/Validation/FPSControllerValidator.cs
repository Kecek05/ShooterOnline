using UnityEngine;
using KeceK.Game.Player;
using KeceK.Game.ScriptableObjects;
using KeceK.Input;

namespace KeceK.Game.Validation
{
    /// <summary>
    /// Validation script to test FPS Movement Controller implementation
    /// </summary>
    public class FPSControllerValidator : MonoBehaviour
    {
        [Header("Required Assets")]
        [SerializeField] private MovementSettingsSO movementSettings;
        [SerializeField] private CameraSettingsSO cameraSettings;
        [SerializeField] private InputReaderSO inputReader;
        
        [Header("Validation Results")]
        [SerializeField] private bool allComponentsValid;
        [SerializeField] private string validationMessage;
        
        private void Start()
        {
            ValidateImplementation();
        }
        
        public void ValidateImplementation()
        {
            bool isValid = true;
            string message = "FPS Controller Validation:\n";
            
            // Check ScriptableObjects
            if (movementSettings == null)
            {
                isValid = false;
                message += "❌ MovementSettingsSO is missing\n";
            }
            else
            {
                message += "✓ MovementSettingsSO found\n";
            }
            
            if (cameraSettings == null)
            {
                isValid = false;
                message += "❌ CameraSettingsSO is missing\n";
            }
            else
            {
                message += "✓ CameraSettingsSO found\n";
            }
            
            if (inputReader == null)
            {
                isValid = false;
                message += "❌ InputReaderSO is missing\n";
            }
            else
            {
                message += "✓ InputReaderSO found\n";
            }
            
            // Check if FPSController component exists
            var fpsController = FindObjectOfType<FPSController>();
            if (fpsController == null)
            {
                isValid = false;
                message += "❌ FPSController component not found in scene\n";
            }
            else
            {
                message += "✓ FPSController component found\n";
                
                // Check FPSController configuration
                if (ValidateFPSController(fpsController))
                {
                    message += "✓ FPSController properly configured\n";
                }
                else
                {
                    isValid = false;
                    message += "❌ FPSController missing required references\n";
                }
            }
            
            allComponentsValid = isValid;
            validationMessage = message;
            
            if (isValid)
            {
                message += "\n🎉 FPS Movement Controller validation PASSED!";
                Debug.Log(message);
            }
            else
            {
                message += "\n⚠️ FPS Movement Controller validation FAILED!";
                Debug.LogWarning(message);
            }
        }
        
        private bool ValidateFPSController(FPSController controller)
        {
            // Use reflection to check if required fields are assigned
            var fields = controller.GetType().GetFields(
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Instance);
            
            foreach (var field in fields)
            {
                if (field.Name.Contains("_") && field.FieldType.IsSubclassOf(typeof(Object)))
                {
                    var value = field.GetValue(controller);
                    if (value == null)
                    {
                        Debug.LogWarning($"Field {field.Name} is not assigned in FPSController");
                        return false;
                    }
                }
            }
            
            return true;
        }
        
        [ContextMenu("Run Validation")]
        public void RunValidationFromContext()
        {
            ValidateImplementation();
        }
    }
}