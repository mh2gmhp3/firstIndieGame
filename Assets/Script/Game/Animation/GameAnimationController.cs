using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Movement
{
    //TODO 先只處理IMovementAnimationController 這個應該會觸里程能夠吃所有操作動作的動畫控制 避免需要另外的管理器管理動畫相互關係的狀態
    public class GameAnimationController : IMovementAnimationController
    {
        private Animator _animator;

        #region Animator State Name TODO 暫時使用 之後可以改成每個單位的設定檔 來對應觸發的State 或是每個Animator有固定的State命名

        private const string IDLE = "Idle";
        private const string RUN = "Run";

        #endregion

        public void SetAnimatior(Animator animator)
        {
            _animator = animator;
        }

        public void MoveInput(Vector3 axis, Quaternion quaternion)
        {
            _animator.GetCurrentAnimatorStateInfo(0);
            if (axis.sqrMagnitude != 0)
            {
                CrossFade(RUN, 2f);
                return;
            }

            CrossFade(IDLE, 2f);
        }

        public void OnFalling(float fallingElapsedTime, float fallingMaxTime)
        {

        }

        public void OnJumping(int jumpCount, float jumpElapsedTime, float jumpLimitTime)
        {

        }

        public void OnLand()
        {

        }

        private void CrossFade(string stateName, float normalizedTransitionDuration)
        {
            var currentStateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            if (currentStateInfo.IsName(stateName))
                return;

            _animator.CrossFade(stateName, normalizedTransitionDuration);
        }
    }
}
