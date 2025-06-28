using GameMainModule.Attack;
using Logging;
using UnitModule.Movement;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnitModule;

namespace GameMainModule
{
    /// <summary>
    /// 三維空間的角色控制器
    /// </summary>
    [Serializable]
    public class GameThreeDimensionalCharacterController
    {
        //TODO 先開始加入部分狀態Flag 避免直接bool到時混亂
        public enum State
        {
            Move,
            Comboing,
        }

        [SerializeField]
        private ThreeDimensionalMovement _movement;
        [SerializeField]
        private CharacterAttackController _attackController;

        private GameAnimationController _animationController;

        private State _currentState = State.Move;

        public GameThreeDimensionalCharacterController()
        {
            _movement = new ThreeDimensionalMovement();
            _attackController = new CharacterAttackController();
            _attackController.RegisterComboingAction(OnStartAttackComboing, OnEndAttackComboing);
        }

        public void InitController(UnitMovementSetting setting)
        {
            _movement.SetMovementSetting(setting);
            _movement.SetEnable(true);
            //動畫控制
            _animationController = new GameAnimationController();
            _animationController.SetAnimatior(setting.Animator);
            _movement.SetMovementAnimationController(_animationController);
            _attackController.SetAnimationController(_animationController);
        }

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

        private void OnStartAttackComboing()
        {
            ChangeState(State.Comboing);
        }

        private void OnEndAttackComboing()
        {
            //TODO 不一定是直接替換回Move可能藥用回退或是檢查當前狀態的方式
            ChangeState(State.Move);
        }

        #endregion

        #region State

        private void ChangeState(State state)
        {
            if (_currentState == state)
            {
                return;
            }

            _currentState = state;
            //TODO 先簡單處理對於移動的控制
            switch (_currentState)
            {
                case State.Move:
                    _movement.SetSpeedRatio(1f);
                    _animationController.BlockMovement = false;
                    break;
                case State.Comboing:
                    _movement.SetSpeedRatio(0.1f);
                    _animationController.BlockMovement = true;
                    break;
                default:
                    break;
            }
        }

        #endregion
    }
}
