using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnitModule.Movement.ThreeDimensionalMovementUtility;

namespace UnitModule.Movement
{
    public class TargetMovementData : IMovementData
    {
        #region IMovementData

        public IUnitMovementSetting UnitMovementSetting { get; private set; }

        public MovementSetting MovementSetting { get; private set; }

        public bool DisableResetVelocity { get; set; }

        public bool IsGround { get; set; }
        public RaycastHit GroundHit { get; set; }
        public bool IsSlope { get; set; }
        public RaycastHit SlopeHit { get; set; }

        #endregion

        public Vector3 TargetPosition;
        public float Speed;

        public void Init(IUnitMovementSetting unitMovementSetting, MovementSetting movementSetting)
        {
            UnitMovementSetting = unitMovementSetting;
            MovementSetting = movementSetting;
        }

        public void Clear()
        {
            UnitMovementSetting = null;
            MovementSetting = null;
        }

        public Vector3 GetForwardNormal()
        {
            var vector = TargetPosition - UnitMovementSetting.RootTransform.position;
            vector = new Vector3(vector.x, 0f, vector.z); //不考慮高度
            return vector.normalized;
        }

        public float GetSpeed()
        {
            return Speed;
        }

        public float GetJumpElapsedTime()
        {
            return 0;
        }

        public float GetFallElapsedTime()
        {
            return 0;
        }

        public bool HaveMoveOperate()
        {
            return GetForwardNormal().sqrMagnitude > 0;
        }
    }
}
