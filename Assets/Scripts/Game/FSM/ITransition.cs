using UnityEngine;

namespace KeceK.Game
{
    public interface ITransition
    {
        IState TargetState { get; }
        IPredicate Condition { get; }
    }
}
