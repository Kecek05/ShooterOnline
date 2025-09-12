using UnityEngine;

namespace KeceK.Game
{
    public class WalkPlayerState : BasePlayerState
    {
        public WalkPlayerState(PlayerManager playerManager, Animator animator) : base(playerManager, animator)
        {
        }

        public override void OnEnterState()
        {
            Debug.Log("WalkPlayerState OnEnterState");
            _animator.CrossFade(WalkHash, CrossFadeDuration);
        }

        public override void UpdateState()
        {
            _playerManager.CameraMovement.HandleCameraMovement();
        }
        
        public override void FixedUpdateState()
        {
            _playerManager.PlayerMovement.HandleMovement();
        }
    }
}
