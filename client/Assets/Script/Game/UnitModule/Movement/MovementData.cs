using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnitModule.Movement.ThreeDimensionalMovementUtility;

namespace UnitModule.Movement
{
    [Serializable]
    public class FallData
    {
        public float FallStartTime;

        public void StartFall()
        {
            FallStartTime = Time.time;
        }

        public float FallElapsedTime()
        {
            return Time.time - FallStartTime;
        }
    }

    [Serializable]
    public class JumpData
    {
        public int JumpMaxCount = 2;

        public int JumpCount = 0;

        public float JumpStartTime;
        public bool JumpEnd = false;

        public bool JumpTrigger = false;

        public void StartJump()
        {
            JumpCount++;
            JumpStartTime = Time.time;
            JumpEnd = false;
            JumpTrigger = false;
        }

        public float JumpElapsedTime()
        {
            return Time.time - JumpStartTime;
        }

        public bool CanJump()
        {
            return JumpCount < JumpMaxCount;
        }

        public void ResetJump()
        {
            JumpCount = 0;
            JumpEnd = false;
            JumpTrigger = false;
        }
    }

    [Serializable]
    public class LandData
    {
        public float LandStartTime;

        public void StartLand()
        {
            LandStartTime = Time.time;
        }

        public float LandElapsedTime()
        {
            return Time.time - LandStartTime;
        }
    }

    public class DashData
    {
        public float DashStartTime;

        public bool DashTrigger = false;

        public void StartDash()
        {
            DashStartTime = Time.time;
        }

        public float DashElapsedTime()
        {
            return Time.time - DashStartTime;
        }
    }

    [Serializable]
    public class MovementData : IMovementData
    {
        #region Const

        public const float SlowSpeedRate = 0.3f;
        public const float MidSpeedRate = 0.5f;
        public const float FastSpeedRate = 1.0f;

        #endregion

        #region IMovementData

        public IUnitMovementSetting UnitMovementSetting { get; }
        public MovementSetting MovementSetting { get; }

        public bool DisableResetVelocity { get; set; } = false;

        public bool IsGround { get; set; }
        public RaycastHit GroundHit { get; set; }

        public bool IsSlope { get; set; }
        public RaycastHit SlopeHit { get; set; }

        #endregion

        public JumpData JumpData;
        public FallData FallData;
        public LandData LandData;
        public DashData DashData;

        #region Dynamic Value 可能外部變動的值

        /// <summary>
        /// 移動輸入軸
        /// </summary>
        public Vector3 MoveAxis = Vector3.zero;
        /// <summary>
        /// 移動選轉方向
        /// </summary>
        public Quaternion MoveQuaternion = Quaternion.identity;

        public float Speed = 10f;

        /// <summary>
        /// 速度倍率 會經由部分狀態變動 Walk Run
        /// </summary>
        public float SpeedRate = 1.0f;

        public bool RunTriggered = false;

        #endregion

        public MovementData(IUnitMovementSetting unitMovementSetting, MovementSetting movementSetting)
        {
            UnitMovementSetting = unitMovementSetting;
            MovementSetting = movementSetting;
            JumpData = new JumpData();
            FallData = new FallData();
            LandData = new LandData();
            DashData = new DashData();
        }

        #region IMovementData

        public bool HaveMoveOperate()
        {
            return MoveAxis.sqrMagnitude > 0;
        }

        public Vector3 GetForwardNormal()
        {
            return (MoveQuaternion * MoveAxis).normalized;
        }
        public float GetSpeed()
        {
            return Speed * SpeedRate;
        }

        public float GetJumpElapsedTime()
        {
            return JumpData.JumpElapsedTime();
        }

        public float GetFallElapsedTime()
        {
            return FallData.FallElapsedTime();
        }

        #endregion

        public void ResetTrigger()
        {
            JumpData.JumpTrigger = false;
            RunTriggered = false;
            DashData.DashTrigger = false;
        }
    }
}
