using Logging;
using System;
using System.Collections.Generic;

namespace Utility
{
    public interface IStateIns<T> where T : Enum
    {
        void DoEnter(T previousState);
        void DoExit(T nextState);
    }

    public class SimpleStateMachine<TState, TStateIns> where TState : Enum where TStateIns : IStateIns<TState>
    {

        Dictionary<TState, TStateIns> _stateToIns = new Dictionary<TState, TStateIns>();
        private TState _curState;
        private TStateIns _curStateIns;
        private Action<TState, TState> _onStateChangedEvent;

        public TStateIns CurStateIns => _curStateIns;

        public void SetStateChangeEvent(Action<TState, TState> action)
        {
            _onStateChangedEvent = action;
        }

        public void AddState(TState state, TStateIns stateIns)
        {
            if (_stateToIns.ContainsKey(state))
                return;

            _stateToIns.Add(state, stateIns);
        }

        public void SetState(TState state, bool force = false)
        {
            if (state.Equals(_curState) && !force)
            {
                return;
            }

            if (!_stateToIns.TryGetValue(state, out var stateImp))
            {
                Log.LogError($"SimpleStateMachine.SetState : State Ins not found Can't Set State : {state}");
                return;
            }

            var oriState = _curState;
            if (_curStateIns != null)
                _curStateIns.DoExit(state);
            _curState = state;
            _curStateIns = stateImp;
            _curStateIns.DoEnter(oriState);

            if (_onStateChangedEvent != null)
                _onStateChangedEvent.Invoke(oriState, _curState);
        }
    }
}
