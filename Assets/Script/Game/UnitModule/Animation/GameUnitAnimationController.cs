using CollisionModule;
using GameMainModule.Attack;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnitModule.Movement
{
    //TODO 先只處理IMovementAnimationController 這個應該會觸里程能夠吃所有操作動作的動畫控制 避免需要另外的管理器管理動畫相互關係的狀態
    public class GameUnitAnimationController : IMovementObserver, IAttackCombinationObserver
    {
        private enum State
        {
            None,
            Idle,
            Walk,
            Run,
            Jump_s,
            Jump_d,
            Fall_d,
            Fall_e,
            Attack_01,
            Attack_02,
            Attack_03,
        }

        private Dictionary<(State Out, State In), float> _changeStateFadeNormalizedTime = new Dictionary<(State Out, State In), float>()
        {
            {(State.Idle, State.Run) , 0.4f},
            {(State.Idle, State.Walk) , 0.4f},
            {(State.Walk, State.Run) , 1f},
            {(State.Run, State.Idle) , 0.2f},
            {(State.Jump_d, State.Fall_d) , 1f},
            {(State.Fall_d, State.Fall_e) , 1f},
            {(State.Fall_e, State.Idle) , 0.1f},
            {(State.Fall_e, State.Walk) , 1f},
            {(State.Fall_e, State.Run) , 1f},
        };

        private HashSet<State> _forceState = new HashSet<State>
        {
            State.Attack_01,
            State.Attack_02,
            State.Attack_03,
        };

        private Animator _animator;

        #region Animator State Name TODO 暫時使用 之後可以改成每個單位的設定檔 來對應觸發的State 或是每個Animator有固定的State命名

        private State _state = State.None;

        #endregion

        public void SetAnimatior(Animator animator)
        {
            _animator = animator;
        }

        #region IMovementObserver
        public void OnStateChanged(int oriState, int newState)
        {
            var movementState = (MovementState)newState;
            switch (movementState)
            {
                case MovementState.Idle:
                    ChangeState(State.Idle);
                    break;
                case MovementState.Walk:
                    ChangeState(State.Run);
                    break;
                case MovementState.Run:
                    ChangeState(State.Run);
                    break;
                case MovementState.Jump:
                    ChangeState(State.Jump_d);
                    break;
                case MovementState.Fall:
                    ChangeState(State.Fall_d);
                    break;
                case MovementState.Land:
                    ChangeState(State.Fall_e);
                    break;
            }
        }

        #endregion

        #region IAttackCombinationObserver

        public void OnStartAttackBehavior(string behaviorName)
        {
            if (!Enum.TryParse(behaviorName, out State stateObj))
            {
                return;
            }

            float dir = 0.5f;
            switch (stateObj)
            {
                case State.Attack_01:
                    dir = 0.3f;
                    break;
                case State.Attack_02:
                    dir = 0.6f;
                    break;
                case State.Attack_03:
                    dir = 0.9f;
                    break;
                default:
                    break;
            }

            CollisionAreaManager.CreateCollisionArea(
                new QuadCollisionAreaSetupData(
                    _animator.transform.position + new Vector3(0f, 2.5f, 0f),
                    Vector3.forward,
                    dir,
                    5f,
                    5f,
                    new TestCollisionAreaTriggerReceiver()));

            ChangeState(stateObj);
        }

        public void OnStartComboing()
        {

        }

        public void OnEndComboing()
        {

        }

        #endregion

        private void ChangeState(State state)
        {
            if (_state == state && !_forceState.Contains(state))
            {
                return;
            }

            if (!_changeStateFadeNormalizedTime.TryGetValue((_state, state), out var fadeTime))
            {
                fadeTime = 0.1f;
            }

            bool replay = _state == state;
            _state = state;
            CrossFade(state.ToString(), fadeTime, replay);
        }

        private void CrossFade(string stateName, float normalizedTransitionDuration, bool replay)
        {
            var currentStateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            //if (currentStateInfo.IsName(stateName))
            //    return;

            if (replay)
            {
                _animator.CrossFade(stateName, normalizedTransitionDuration, 0, 0f);
                return;
            }

            _animator.CrossFade(stateName, normalizedTransitionDuration, 0);
        }
    }
}
