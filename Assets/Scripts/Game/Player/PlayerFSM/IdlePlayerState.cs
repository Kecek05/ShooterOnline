using UnityEngine;

namespace KeceK.Game
{
    public class IdlePlayerState : BasePlayerState
    {
        public IdlePlayerState(PlayerManager playerManager, Animator animator) : base(playerManager, animator)
        {
        }
        
        public override void OnEnterState()
        {
            Debug.Log("IdlePlayerState OnEnterState");
            _animator.CrossFade(IdleHash, CrossFadeDuration);
        }
        
        public override void UpdateState()
        {
            _playerManager.CameraMovement.HandleCameraMovement();
        }
    }
}
