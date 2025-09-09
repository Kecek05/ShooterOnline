using UnityEngine;



namespace HierarchyFocusedDebugConsole {

    public class LogGenerator : MonoBehaviour {

        [Header("Settings")]
        [Tooltip("Time between log messages.")]
        [Min(0f)]
        [SerializeField] private float _timeBetweenLogs = 0.2f;
        [Tooltip("Number of repeat of log messages.")]
        [Min(1)]
        [SerializeField] private int _repeatCount = 3;

        private int _logTypeCount = 5;


    
        void Start() {
            
            GenerateLogs();

        }



        private void GenerateLogs() {
            for (int i = 0; i < _repeatCount; i++) {
                Invoke(nameof(TestLog), _timeBetweenLogs * i * _logTypeCount);
                Invoke(nameof(TestError), _timeBetweenLogs * i * _logTypeCount + _timeBetweenLogs);
                Invoke(nameof(TestWarning), _timeBetweenLogs * i * _logTypeCount + _timeBetweenLogs * 2f);
                Invoke(nameof(TestAssert), _timeBetweenLogs * i * _logTypeCount + _timeBetweenLogs * 3f);
                Invoke(nameof(TestFail), _timeBetweenLogs * i * _logTypeCount + _timeBetweenLogs * 4f);
            }
        }



        private void TestLog() {
            Debug.Log("Test log message for " + gameObject.name, gameObject);
        }



        private void TestError() {
            string message = "Very long error message for " + gameObject.name + " that exceeds the default width of the console window. This is to test how the console handles long messages and if it wraps them correctly or not. Let's see how it behaves with this long string. This is a test to ensure that the console can handle long messages without any issues. We want to make sure that the text is readable and doesn't overflow or cause any problems in the console window. This is important for debugging purposes and for maintaining a clean and organized console output.";
            Debug.LogError(message, gameObject);
        }



        private void TestWarning() {
            Debug.LogWarning("Test warning message for " + gameObject.name, gameObject);
        }



        private void TestAssert() {
            Debug.Assert(false, "Test assert message for " + gameObject.name, gameObject);
        }



        private void TestFail() {
            Rigidbody rigidbody = null;
            rigidbody.AddForce(Vector3.one); // This will cause a NullReferenceException
        }



    }

}
