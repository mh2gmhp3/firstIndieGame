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

        public FallData FallData { get; set; } = new FallData();

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

        public Vector3 GetMovementForwardNormal()
        {
            var vector = TargetPosition - UnitMovementSetting.RootTransform.position;
            vector = new Vector3(vector.x, 0f, vector.z); //不考慮高度
            return vector.normalized;
        }

        public Vector3 GetRotateForwardNormal()
        {
            return GetMovementForwardNormal();
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
            return FallData.FallElapsedTime(); ;
        }

        public bool HaveMoveOperate()
        {
            return GetMovementForwardNormal().sqrMagnitude > 0;
        }
    }
}
