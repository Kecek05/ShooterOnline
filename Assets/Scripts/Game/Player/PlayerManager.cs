using KeceK.Input;
using KeceK.Utils.Components;
using Sirenix.OdinInspector;
using UnityEngine;

namespace KeceK.Game
{
    public class PlayerManager : MonoBehaviour
    {
        [Title("References")]
        [SerializeField] [Required] PlayerMovement playerMovement;
        [SerializeField] [Required] CameraMovement _cameraMovement;
        [SerializeField] [Required] GroundCheck _groundCheck;
        [SerializeField] [Required] private Animator _animator;
        [SerializeField] [Required] private InputReaderSO _inputReaderSO;
        
        
        public PlayerMovement PlayerMovement => playerMovement;
        public CameraMovement CameraMovement => _cameraMovement;
        public Animator Animator => _animator;
        
        public InputReaderSO InputReaderSO => _inputReaderSO;
        public GroundCheck GroundCheck => _groundCheck;

        private void Start()
        {
            //TODO : Move to other Manager
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
