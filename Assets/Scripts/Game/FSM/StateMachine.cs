using System;
using System.Collections.Generic;

namespace KeceK.Game
{
    public class StateMachine
    {
        
        private StateNode _currentStateNode;
        
        private Dictionary<Type, StateNode> stateNodes = new();
        private HashSet<ITransition> anyTransitions = new();
        
        class StateNode
        {
            public IState State;
            public HashSet<ITransition> Transitions { get; }

            public StateNode(IState state)
            {
                State = state;
                Transitions = new HashSet<ITransition>();
            }

            public void AddTransition(IState toState, IPredicate condition)
            {
                Transitions.Add(new Transition(toState, condition));
            }
        }

        public void UpdateState()
        {
            var transition = GetTransition();
            if (transition != null)
                ChangeState(transition.TargetState);
            
            _currentStateNode.State?.UpdateState();
        }
        
        public void LateUpdateState()
        {
            _currentStateNode.State?.LateUpdateState();
        }
        
        public void FixedUpdateState()
        {
            _currentStateNode.State?.FixedUpdateState();
        }

        public void SetState(IState state)
        {
            _currentStateNode = stateNodes[state.GetType()];
            _currentStateNode.State?.OnEnterState();
        }

        private void ChangeState(IState newState)
        {
            if(newState == _currentStateNode.State) return;
            
            var previousState = _currentStateNode.State;
            var nextState = stateNodes[newState.GetType()].State;
            
            previousState?.OnExitState();
            nextState?.OnEnterState();
            
            _currentStateNode = stateNodes[newState.GetType()];
        }

        private ITransition GetTransition()
        {
            foreach(ITransition transition in anyTransitions)
                if(transition.Condition.Evaluate())
                    return transition;

            foreach (ITransition transition in _currentStateNode.Transitions)
                if(transition.Condition.Evaluate())
                    return transition;
            
            return null;
        }
        
        public void AddTransition(IState fromState, IState toState, IPredicate condition)
        {
            GetOrAddStateNode(fromState).AddTransition(GetOrAddStateNode(toState).State, condition);
        }
        
        public void AddAnyTransition(IState toState, IPredicate condition)
        {
            anyTransitions.Add(new Transition(GetOrAddStateNode(toState).State, condition));
        }

        private StateNode GetOrAddStateNode(IState state)
        {
            var node = stateNodes.GetValueOrDefault(state.GetType());
            if (node == null)
            {
                node = new StateNode(state);
                stateNodes.Add(state.GetType(), node);
            }
            
            return node;
        }
    }
}
