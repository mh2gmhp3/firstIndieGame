using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnitModule.Movement
{
    public enum MovementState
    {
        Idle,

        Walk,
        Run,

        Jump,

        Fall,

        Land,

        //可透過外部行為暫停移動狀態
        Block
    }

    public interface IMovement
    {
        #region Unity Life Cycle

        void DoUpdate();
        void DoFixedUpdate();

        #endregion

        #region Unity Editor

        void DoOnGUI();

        #endregion

        #region Basic Set

        void SetEnable(bool enable);
        void SetMovementSetting(UnitMovementSetting unitMovementSetting, MovementSetting movementSetting);

        #endregion

        #region Input

        void SetMoveAxis(Vector3 axis);
        void SetMoveQuaternion(Quaternion quaternion);
        void InputCommand(int command);

        #endregion

        #region State

        void SetState(MovementState state, bool force = false);

        #endregion

        #region Observer

        void AddObserver(IMovementObserver movementAnimationController);
        void RemoveObserver(IMovementObserver observer);

        #endregion
    }
}
