using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnitModule.Movement
{
    //TODO 先只處理IMovementAnimationController 這個應該會觸里程能夠吃所有操作動作的動畫控制 避免需要另外的管理器管理動畫相互關係的狀態
    public class GameAnimationController : IMovementAnimationController
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
        }

        private Dictionary<(State Out, State In), float> _changeStateFadeNormalizedTime = new Dictionary<(State Out, State In), float>()
        {
            {(State.Idle, State.Run) , 0.4f},
            {(State.Idle, State.Walk) , 0.4f},
            {(State.Walk, State.Run) , 1f},
            {(State.Jump_d, State.Fall_d) , 1f},
            {(State.Fall_d, State.Fall_e) , 1f},
            {(State.Fall_e, State.Idle) , 0.1f},
            {(State.Fall_e, State.Walk) , 1f},
            {(State.Fall_e, State.Run) , 1f},
        };

        private Animator _animator;

        #region Animator State Name TODO 暫時使用 之後可以改成每個單位的設定檔 來對應觸發的State 或是每個Animator有固定的State命名

        private const string IDLE = "Idle";
        private const string RUN = "Run";

        private State _state = State.None;

        #endregion

        public void SetAnimatior(Animator animator)
        {
            _animator = animator;
        }

        public void MoveInput(Vector3 axis, Quaternion quaternion)
        {
            if (_state == State.Jump_d ||
                _state == State.Jump_s ||
                _state == State.Fall_d)
            {
                return;
            }

            if (axis.sqrMagnitude >= 0.7)
            {
                ChangeState(State.Run);
                return;
            }

            if (axis.sqrMagnitude > 0)
            {
                ChangeState(State.Walk);
                return;
            }

            ChangeState(State.Idle);
        }

        public void OnFalling(float fallingElapsedTime, float fallingMaxTime)
        {
            ChangeState(State.Fall_d);
        }

        public void OnJumping(int jumpCount, float jumpElapsedTime, float jumpLimitTime)
        {
            ChangeState(State.Jump_d);
        }

        public void OnLand()
        {
            ChangeState(State.Fall_e);
        }

        private void ChangeState(State state)
        {
            if (_state == state)
            {
                return;
            }

            if (!_changeStateFadeNormalizedTime.TryGetValue((_state, state), out var fadeTime))
            {
                fadeTime = 0.1f;
            }

            _state = state;
            CrossFade(state.ToString(), fadeTime);
        }

        private void CrossFade(string stateName, float normalizedTransitionDuration)
        {
            var currentStateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            //if (currentStateInfo.IsName(stateName))
            //    return;

            _animator.CrossFade(stateName, normalizedTransitionDuration);
        }
    }
}
