using Logging;
using System;
using UnityEngine;
using Utility;
using static UnitModule.Movement.ThreeDimensionalMovementUtility;

namespace UnitModule.Movement
{
    [Serializable]
    public partial class ThreeDimensionalMovement : IMovement
    {
        /// <summary>
        /// 啟用
        /// </summary>
        [SerializeField]
        private bool _enable = false;

        [SerializeField]
        private UnitMovementSetting _unitMovementSetting;
        [SerializeField]
        private MovementSetting _movementSetting;

        [SerializeField]
        private MovementData _movementData;

        /// <summary>
        /// 移動行為觀察者
        /// </summary>
        [SerializeField]
        private ObserverController<IMovementObserver> _observerController = new ObserverController<IMovementObserver>();

        public ThreeDimensionalMovement()
        {

        }

        #region IMovement

        #region Unity Life Cycle

        public void DoUpdate()
        {
            if (!_enable)
                return;

            if (_stateMachine.CurStateIns == null)
                return;

            ThreeDimensionalMovementUtility.UpdateState(_movementData);
            _stateMachine.CurStateIns.DoUpdate();
        }

        public void DoFixedUpdate()
        {
            if (!_enable)
                return;

            if (_stateMachine.CurStateIns == null)
                return;

            ThreeDimensionalMovementUtility.ResetRigibidy(_movementData);
            _stateMachine.CurStateIns.DoFixedUpdate();
        }

        #endregion

        #region Unity Editor

        public void DoOnGUI()
        {
#if UNITY_EDITOR
            if (!_enable)
                return;

            if (_stateMachine.CurStateIns == null)
                return;

            _stateMachine.CurStateIns.DoGUI();
#endif
        }

        #endregion

        #region Basic Set

        public void SetEnable(bool enable)
        {
            _enable = enable;
        }

        public void SetMovementSetting(UnitMovementSetting unitMovementSetting, MovementSetting movementSetting)
        {
            if (unitMovementSetting == null && unitMovementSetting.IsValid() || movementSetting == null)
            {
                Log.LogError("ThreeDimensionalMovement.SetMovementSetting : unitMovementSetting or movementSetting is null or invalid");
                return;
            }
            _unitMovementSetting = unitMovementSetting;
            _movementSetting = movementSetting;
            _movementData = new MovementData(unitMovementSetting, movementSetting);
            InitState(_movementData);
        }

        #endregion

        #region Input

        public void SetMoveAxis(Vector3 axis)
        {
            if (_movementData == null)
                return;
            _movementData.MoveAxis = axis;
        }

        public void SetMoveQuaternion(Quaternion quaternion)
        {
            if (_movementData == null)
                return;
            //只取Y軸
            _movementData.MoveQuaternion = new Quaternion(0, quaternion.y, 0, quaternion.w);
        }

        public void InputCommand(int command)
        {
            if (_stateMachine.CurStateIns == null)
                return;

            _stateMachine.CurStateIns.DoInputCommand(command);
        }

        #endregion

        #region Observer

        public void AddObserver(IMovementObserver observer)
        {
            _observerController.AddObserver(observer);
        }

        public void RemoveObserver(IMovementObserver observer)
        {
            _observerController.RemoveObserver(observer);
        }

        #endregion

        #endregion
    }
}