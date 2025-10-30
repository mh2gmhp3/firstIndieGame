using Logging;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Utility
{
    public abstract class State<TStateId>
    {
        public virtual bool CanEnter() { return true; }
        public virtual bool CanExit() { return true; }

        public virtual void DoEnter(TStateId previousState) { }
        public virtual void DoUpdate() { }
        public virtual void DoFixedUpdate() { }
        public virtual void DoExit(TStateId nextState) { }
    }

    [Serializable]
    public class StateMachine<TStateId, TState> where TState : State<TStateId>
    {
        public class Transition<TStableId>
        {
            public TStableId From;
            public TStableId To;
            public Func<bool> Condition;

            public Transition(TStableId from, TStableId to, Func<bool> condition)
            {
                From = from;
                To = to;
                Condition = condition;
            }
        }

        [SerializeField]
        private TStateId _currentStateId;
        private TState _currentState;
        private Dictionary<TStateId, TState> _idToStateDic = new Dictionary<TStateId, TState>();
        private List<Transition<TStateId>> _transitionList = new List<Transition<TStateId>>();

        private Action<TStateId, TStateId> _onStateChangedEvent;

        public TStateId CurrentStateId => _currentStateId;
        public TState CurrentState => _currentState;

        public void SetStateChangeEvent(Action<TStateId, TStateId> action)
        {
            _onStateChangedEvent = action;
        }

        public void AddState(TStateId stateId, TState state)
        {
            if (_idToStateDic.ContainsKey(stateId))
                return;

            _idToStateDic.Add(stateId, state);
        }

        public void AddTransition(TStateId from, TStateId to, Func<bool> condition)
        {
            _transitionList.Add(new Transition<TStateId>(from, to, condition));
        }

        public void AddTransition(TStateId[] froms, TStateId to, Func<bool> condition)
        {
            for (int i = 0; i < froms.Length; i++)
            {
                AddTransition(froms[i], to, condition);
            }
        }

        public bool SetState(TStateId stateId, bool force = false)
        {
            if (stateId.Equals(_currentStateId) && !force)
            {
                return false;
            }

            if (!_idToStateDic.TryGetValue(stateId, out var state))
            {
                Log.LogError($"StateMachine.SetState : State not found, Can't Set State : {stateId}");
                return false;
            }


            var oriStateId = _currentStateId;
            if (_currentState != null)
            {
                if (!_currentState.CanExit())
                    return false;

                _currentState.DoExit(stateId);
            }

            if (!state.CanEnter())
                return false;

            _currentStateId = stateId;
            _currentState = state;
            _currentState.DoEnter(oriStateId);

            if (_onStateChangedEvent != null)
                _onStateChangedEvent.Invoke(oriStateId, _currentStateId);

            return true;
        }

        public void Update()
        {
            for (int i = 0; i < _transitionList.Count; i++)
            {
                var transition = _transitionList[i];
                if (!EqualityComparer<TStateId>.Default.Equals(transition.From, _currentStateId))
                    continue;

                if (!transition.Condition())
                    continue;

                SetState(transition.To);
            }
            if (_currentState == null)
                return;
            _currentState.DoUpdate();
        }

        public void FixedUpdate()
        {
            if (_currentState == null)
                return;
            _currentState.DoFixedUpdate();
        }
    }
}
