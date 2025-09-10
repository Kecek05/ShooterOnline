using UnityEngine;

namespace KeceK.Game
{
    public class JumpPlayerState : BasePlayerState
    {
        public JumpPlayerState(PlayerManager playerManager, Animator animator) : base(playerManager, animator)
        {
        }
        
        public override void OnEnterState()
        {
            _animator.CrossFade(JumpHash, CrossFadeDuration);
            // _playerManager.Jump();
        }

        public override void FixedUpdateState()
        {
            _playerManager.CharacterMovement.HandleMovement();
            // TODO _playerManager.CharacterMovement.HandleJump();
        }

        public override void LateUpdateState()
        {
            _playerManager.CameraMovement.HandleCameraMovement();
        }
    }
}
