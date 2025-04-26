using GameMainModule.Attack;
using Logging;
using UnitModule.Movement;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameMainModule
{
    /// <summary>
    /// 三維空間的角色控制器
    /// </summary>
    [Serializable]
    public class GameThreeDimensionalCharacterController
    {
        [SerializeField]
        private ThreeDimensionalMovement _movement = new ThreeDimensionalMovement();
        [SerializeField]
        private CharacterAttackController _attackController = new CharacterAttackController();

        public void DoUpdate()
        {
            _attackController.DoUpdate();
            _movement.DoUpdate();
        }

        public void DoFixedUpdate()
        {
            _movement.DoFixedUpdate();
        }

        public void DoOnGUI()
        {
            _movement.DoOnGUI();
        }

        /// <summary>
        /// 設定啟用 預設為關閉
        /// </summary>
        /// <param name="enable"></param>
        public void SetEnable(bool enable)
        {
            _movement.SetEnable(enable);
        }

        #region Movement

        /// <summary>
        /// 設定角色Root
        /// </summary>
        /// <param name="transform"></param>
        public void SetCharacterRoot(GameObject root)
        {
            _movement.SetMovementTargetRoot(root);
        }

        public void SetMovementAnimationController(IMovementAnimationController movementAnimationController)
        {
            _movement.SetMovementAnimationController(movementAnimationController);
        }

        /// <summary>
        /// 設定移動輸入軸
        /// </summary>
        /// <param name="axis"></param>
        public void SetMoveAxis(Vector3 axis)
        {
            _movement.SetMoveAxis(axis);
        }

        /// <summary>
        /// 設定移動選轉方向
        /// </summary>
        /// <param name="quaternion"></param>
        public void SetMoveQuaternion(Quaternion quaternion)
        {
            _movement.SetMoveQuaternion(quaternion);
        }

        public void Jump()
        {
            _movement.Jump();
        }

        #endregion

        #region Attack

        public void MainAttack()
        {
            _attackController.TriggerMainAttack();
        }

        public void SubAttack()
        {
            _attackController.TriggerSubAttack();
        }

        public void SetCombinationList(List<AttackCombination> combinationList)
        {
            _attackController.SetCombinationList(combinationList);
        }

        public void SetNowCombination(int index)
        {
            _attackController.SetNowCombination(index);
        }

        #endregion
    }
}
