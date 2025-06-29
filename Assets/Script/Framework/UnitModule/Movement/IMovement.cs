using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnitModule.Movement
{
    public interface IMovement
    {
        public void DoUpdate();
        public void DoFixedUpdate();
        public void DoOnGUI();

        public void SetEnable(bool enable);
        public void SetMovementSetting(UnitMovementSetting movementSetting);
        public void AddObserver(IMovementObserver movementAnimationController);

        public void SetMoveAxis(Vector3 axis);
        public void SetMoveQuaternion(Quaternion quaternion);
        public void Jump();
    }
}
