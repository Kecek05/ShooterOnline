using System;
using KeceK.Input;
using KeceK.Utils.Components;
using NUnit.Framework;
using Sirenix.OdinInspector;
using UnityEngine;

namespace KeceK.Game
{
    public class PlayerManager : MonoBehaviour
    {
        [Title("References")]
        [SerializeField] [Required] CharacterMovement _characterMovement;
        [SerializeField] [Required] CameraMovement _cameraMovement;
        [SerializeField] [Required] GroundCheck _groundCheck;
        [SerializeField] [Required] private Animator _animator;
        [SerializeField] [Required] private InputReaderSO _inputReaderSO;
        private StateMachine _playerStateMachine;
        
        public CharacterMovement CharacterMovement => _characterMovement;
        public CameraMovement CameraMovement => _cameraMovement;

        private void Awake()
        {
            _playerStateMachine = new StateMachine();
            DefineStateAndTransitions();
        }

        private void Start()
        {
            //TODO : Move to other Manager
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            _playerStateMachine.UpdateState();
        }
        
        private void FixedUpdate()
        {
            _playerStateMachine.FixedUpdateState();
        }
        
        private void LateUpdate()
        {
            _playerStateMachine.LateUpdateState();
        }

        private void DefineStateAndTransitions()
        {
            var idleState = new IdlePlayerState(this, _animator);
            var walkState = new WalkPlayerState(this, _animator);
            var jumpState = new JumpPlayerState(this, _animator);
            
            At(idleState, walkState, new FuncPredicate(() => _inputReaderSO.MoveInput != Vector2.zero));
            At(walkState, idleState, new FuncPredicate(() => _inputReaderSO.MoveInput == Vector2.zero));
            
            _playerStateMachine.SetState(idleState);
        }
        
        
        private void At(IState fromState, IState toState, IPredicate condition) => _playerStateMachine.AddTransition(fromState, toState, condition);
        private void Any(IState toState, IPredicate condition) => _playerStateMachine.AddAnyTransition(toState, condition);
    }
}
