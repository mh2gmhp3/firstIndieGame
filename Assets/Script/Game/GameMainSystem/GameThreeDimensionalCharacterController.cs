using GameMainModule.Attack;
using System;
using System.Collections.Generic;
using UnitModule.Movement;
using UnityEngine;

namespace GameMainModule
{
    /// <summary>
    /// 三維空間的角色控制器
    /// </summary>
    [Serializable]
    public class GameThreeDimensionalCharacterController : IAttackCombinationObserver, IMovementObserver
    {
        //TODO 先開始加入部分狀態Flag 避免直接bool到時混亂
        public enum State
        {
            None     = 0,
            Movement = 1 << 0,
            Comboing = 1 << 1,
        }

        [SerializeField]
        private ThreeDimensionalMovement _movement;
        [SerializeField]
        private CharacterAttackController _attackController;

        private GameUnitAnimationController _animationController;

        private State _currentState = State.None;

        public GameThreeDimensionalCharacterController()
        {
            _movement = new ThreeDimensionalMovement();
            _movement.AddObserver(this);
            _attackController = new CharacterAttackController();
            _attackController.AddObserver(this);
            _animationController = new GameUnitAnimationController();
        }

        public void InitController(UnitMovementSetting unityMovementSetting, MovementSetting movementSetting)
        {
            _movement.SetMovementSetting(unityMovementSetting, movementSetting);
            _movement.SetEnable(true);
            //動畫控制
            _animationController.SetAnimatior(unityMovementSetting.Animator);
            _movement.AddObserver(_animationController);
            _attackController.AddObserver(_animationController);

            _currentState = State.Movement;
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
            _movement.InputCommand((int)MovementInputCommand.Jump);
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

        #region State

        private void ChangeState(State state)
        {
            if (_currentState == state)
            {
                return;
            }

            _currentState = state;
            if (state == State.Comboing)
            {
                _movement.SetState(MovementState.Block);
            }
            else if (state == State.Movement)
            {
                _movement.CancelBlockState();
            }
        }

        #endregion

        #region IAttackCombinationObserver
        public void OnStartAttackBehavior(string behaviorName)
        {
            //do nothing
        }

        public void OnStartComboing()
        {
            ChangeState(State.Comboing);
        }

        public void OnEndComboing()
        {
            ChangeState(State.Movement);
        }

        #endregion

        #region IMovementObserver

        public void OnStateChanged(int oriState, int newState)
        {
            _attackController.KeepComboing(!_movement.IsGround);
        }

        #endregion
    }
}
