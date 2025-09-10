
namespace KeceK.Game
{
    public interface IState
    {
        void OnEnterState();
        void OnExitState();
        void UpdateState();
        void FixedUpdateState();
        void LateUpdateState();
        
    }
}
