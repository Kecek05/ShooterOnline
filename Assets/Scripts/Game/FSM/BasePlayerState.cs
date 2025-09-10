using UnityEngine;

namespace KeceK.Game
{
    public abstract class BasePlayerState : IState
    {
        protected readonly PlayerManager _playerManager;
        protected readonly Animator _animator;
        
        protected static readonly int IdleHash = Animator.StringToHash("Idle");
        protected static readonly int WalkHash = Animator.StringToHash("Walk");
        protected static readonly int JumpHash = Animator.StringToHash("Jump");

        protected const float CrossFadeDuration = 0.1f;
        
        protected BasePlayerState(PlayerManager playerManager, Animator animator)
        {
            _playerManager = playerManager;
            _animator = animator;
        }
        
        public virtual void OnEnterState() { }
        public virtual void OnExitState() { Debug.Log("OnExitState"); }
        public virtual void UpdateState() { }
        public virtual void FixedUpdateState() { }
        
        public virtual void LateUpdateState() { }
    }
}
