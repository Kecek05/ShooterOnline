using KeceK.Input;
using UnityEngine;
using Sirenix.OdinInspector;

namespace KeceK.Game
{
    public class PlayerStateMachineBrain : MonoBehaviour
    {
        [Title("References")]
        [SerializeField] [Required] private PlayerManager _playerManager;
        [SerializeField] [Required] private InputReaderSO _inputReaderSO;
        [SerializeField] [Required] private Rigidbody _rigidbody;
        
        private StateMachine _playerStateMachine;
        
        private void Awake()
        {
            _playerStateMachine = new StateMachine();
            DefineStateAndTransitions();
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
            var idleState = new IdlePlayerState(_playerManager, _playerManager.Animator);
            var walkState = new WalkPlayerState(_playerManager, _playerManager.Animator);
            var jumpState = new JumpPlayerState(_playerManager, _playerManager.Animator);
            
            At(idleState, walkState, new FuncPredicate(() => _inputReaderSO.MoveInput != Vector2.zero));
            At(walkState, idleState, new FuncPredicate(() => _inputReaderSO.MoveInput == Vector2.zero && _rigidbody.linearVelocity == Vector3.zero));
            
            _playerStateMachine.SetState(idleState);
        }
        
        private void At(IState fromState, IState toState, IPredicate condition) => _playerStateMachine.AddTransition(fromState, toState, condition);
        private void Any(IState toState, IPredicate condition) => _playerStateMachine.AddAnyTransition(toState, condition);
    }
}
