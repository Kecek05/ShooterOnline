using System;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace KeceK.Game
{
    public class DebugPlayerUI : MonoBehaviour
    {
        [Title("References")]
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private PlayerMovement _playerMovement;
        
        [Title("Texts")]
        [SerializeField] private TextMeshProUGUI _text1;
        [SerializeField] private TextMeshProUGUI _text2;
        [SerializeField] private TextMeshProUGUI _text3;

        private void Update()
        {
            if (_text1)
            {
                _text1.text = $"Moving Speed: {_playerMovement.MovingSpeed} | Rigidbody Speed: {_rigidbody.linearVelocity.magnitude}";
            }

            if (_text2)
            {
                _text2.text = $"Acceleration: {_playerMovement.EvaluatedAccelerationSpeed} | Deceleration: {_playerMovement.EvaluatedDecelerationSpeed}";
            }

            if (_text3)
            {
                _text3.text = $"Move Direction: {_playerMovement.MoveDirection}";
            }

            if (UnityEngine.Input.GetKeyDown(KeyCode.Y))
            {
                Application.targetFrameRate = 60;
            }
            
            if (UnityEngine.Input.GetKeyDown(KeyCode.U))
            {
                Application.targetFrameRate = 15;
            }
            
            if (UnityEngine.Input.GetKeyDown(KeyCode.I))
            {
                Application.targetFrameRate = 200;
            }
        }
    }
}
