using UnityEngine;
using UnityEditor;
using KeceK.Game.Player;
using KeceK.Game.ScriptableObjects;
using Unity.Cinemachine;

namespace KeceK.Game.Editor
{
    /// <summary>
    /// Editor utility to setup FPS Controller quickly
    /// </summary>
    public class FPSControllerSetup : EditorWindow
    {
        [MenuItem("Tools/Kecek/FPS Controller Setup")]
        public static void ShowWindow()
        {
            GetWindow<FPSControllerSetup>("FPS Controller Setup");
        }
        
        private void OnGUI()
        {
            GUILayout.Label("FPS Controller Setup", EditorStyles.boldLabel);
            GUILayout.Space(10);
            
            if (GUILayout.Button("Create FPS Player GameObject"))
            {
                CreateFPSPlayer();
            }
            
            GUILayout.Space(5);
            
            if (GUILayout.Button("Create Movement Settings SO"))
            {
                CreateMovementSettings();
            }
            
            GUILayout.Space(5);
            
            if (GUILayout.Button("Create Camera Settings SO"))
            {
                CreateCameraSettings();
            }
            
            GUILayout.Space(10);
            GUILayout.Label("Instructions:", EditorStyles.boldLabel);
            GUILayout.Label("1. Click 'Create FPS Player GameObject' to create the player");
            GUILayout.Label("2. Create ScriptableObject assets for settings");
            GUILayout.Label("3. Assign references in the FPSController component");
            GUILayout.Label("4. Set up ground layer mask in MovementSettings");
        }
        
        private void CreateFPSPlayer()
        {
            // Create main player object
            GameObject player = new GameObject("FPS Player");
            player.transform.position = Vector3.zero;
            
            // Add required components
            var rigidbody = player.AddComponent<Rigidbody>();
            rigidbody.freezeRotation = true;
            rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            
            var capsuleCollider = player.AddComponent<CapsuleCollider>();
            capsuleCollider.height = 2f;
            capsuleCollider.radius = 0.5f;
            capsuleCollider.center = new Vector3(0, 1f, 0);
            
            // Create camera target structure
            GameObject cameraTarget = new GameObject("Camera Target");
            cameraTarget.transform.SetParent(player.transform);
            cameraTarget.transform.localPosition = new Vector3(0, 1.6f, 0);
            
            GameObject cameraPitchTarget = new GameObject("Camera Pitch Target");
            cameraPitchTarget.transform.SetParent(cameraTarget.transform);
            cameraPitchTarget.transform.localPosition = Vector3.zero;
            
            // Create ground check point
            GameObject groundCheck = new GameObject("Ground Check");
            groundCheck.transform.SetParent(player.transform);
            groundCheck.transform.localPosition = new Vector3(0, 0.1f, 0);
            
            // Add FPS Controller components
            var groundDetector = player.AddComponent<GroundDetector>();
            var cameraController = player.AddComponent<FirstPersonCameraController>();
            var fpsController = player.AddComponent<FPSController>();
            
            // Create virtual camera
            GameObject virtualCameraGO = new GameObject("FPS Virtual Camera");
            virtualCameraGO.transform.SetParent(cameraPitchTarget.transform);
            virtualCameraGO.transform.localPosition = Vector3.zero;
            
            var virtualCamera = virtualCameraGO.AddComponent<CinemachineCamera>();
            
            // Configure virtual camera for first person
            virtualCamera.Target.TrackingTarget = cameraTarget.transform;
            virtualCamera.Target.LookAtTarget = null;
            
            // Set up basic lens settings
            virtualCamera.Lens.FieldOfView = 75f;
            virtualCamera.Lens.NearClipPlane = 0.01f;
            
            Debug.Log("FPS Player GameObject created successfully!");
            
            // Select the created object
            Selection.activeGameObject = player;
        }
        
        private void CreateMovementSettings()
        {
            var asset = CreateInstance<MovementSettingsSO>();
            string path = "Assets/ScriptableObjects/MovementSettings.asset";
            
            // Create directory if it doesn't exist
            System.IO.Directory.CreateDirectory("Assets/ScriptableObjects");
            
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            
            Debug.Log($"MovementSettingsSO created at {path}");
            Selection.activeObject = asset;
        }
        
        private void CreateCameraSettings()
        {
            var asset = CreateInstance<CameraSettingsSO>();
            string path = "Assets/ScriptableObjects/CameraSettings.asset";
            
            // Create directory if it doesn't exist
            System.IO.Directory.CreateDirectory("Assets/ScriptableObjects");
            
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            
            Debug.Log($"CameraSettingsSO created at {path}");
            Selection.activeObject = asset;
        }
    }
}