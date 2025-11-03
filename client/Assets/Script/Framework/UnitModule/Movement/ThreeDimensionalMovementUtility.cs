using Extension;
using System;
using UnityEngine;

namespace UnitModule.Movement
{
    public static class ThreeDimensionalMovementUtility
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
        public class MovementData
        {
            #region Const

            public const float SlowSpeedRate = 0.3f;
            public const float MidSpeedRate = 0.5f;
            public const float FastSpeedRate = 1.0f;

            #endregion

            public IUnitMovementSetting UnitMovementSetting;
            public MovementSetting MovementSetting;

            public JumpData JumpData;
            public FallData FallData;
            public LandData LandData;
            public DashData DashData;

            public bool DisableResetVelocity = false;

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

            public bool IsGround;
            public RaycastHit GroundHit;

            public bool IsSlope;
            public RaycastHit SlopeHit;

            public MovementData(IUnitMovementSetting unitMovementSetting, MovementSetting movementSetting)
            {
                UnitMovementSetting = unitMovementSetting;
                MovementSetting = movementSetting;
                JumpData = new JumpData();
                FallData = new FallData();
                LandData = new LandData();
                DashData = new DashData();
            }

            public bool IsValid()
            {
                if (MovementSetting == null)
                    return false;

                if (UnitMovementSetting == null)
                    return false;

                return UnitMovementSetting.IsValid();
            }

            public bool HaveMoveInput()
            {
                return MoveAxis.sqrMagnitude > 0;
            }

            public Vector3 GetForward(bool withSlope)
            {
                Vector3 forward = (MoveQuaternion * MoveAxis).normalized;
                if (withSlope)
                    return GetOnSlopeVector(forward);

                return forward;
            }

            /// <summary>
            /// 使用方向獲取與斜波的向量 如果站在斜坡上 沒有將直接回傳原向量
            /// </summary>
            /// <param name="vector"></param>
            /// <returns></returns>
            public Vector3 GetOnSlopeVector(Vector3 vector)
            {
                if (IsSlope)
                    return Vector3.ProjectOnPlane(vector, SlopeHit.normal).normalized;
                return vector;
            }

            public Vector3 GetDashForward()
            {
                if (HaveMoveInput())
                    return GetForward(true);
                else
                    return GetOnSlopeVector(UnitMovementSetting.RotateTransform.forward);    // 沒輸入拿當前面向方向
            }

            public void ResetTrigger()
            {
                JumpData.JumpTrigger = false;
                RunTriggered = false;
                DashData.DashTrigger = false;
            }
        }

        public static bool IsGround(Vector3 startPoint, float radius, float distance, out RaycastHit hit)
        {
            return Physics.SphereCast(
                startPoint,
                radius,
                Vector3.down,
                out hit,
                distance);
        }

        public static bool IsSlope(Vector3 startPoint, float radius, float distance, float angle, out RaycastHit hit)
        {
            //TODO 要再評估看看是否要用Sphere Sphere站在部分物件邊緣會有異常判斷
            if (!Physics.Raycast(
                startPoint,
                Vector3.down,
                out hit,
                distance))
            {
                return false;
            }

            float curAngle = Vector3.Angle(Vector3.up, hit.normal);
            return curAngle != 0 && curAngle < angle;
        }

        public static void UpdateState(MovementData movementData)
        {
            if (movementData == null || movementData.UnitMovementSetting == null)
                return;

            var unitMovementSetting = movementData.UnitMovementSetting;
            var movementSetting = movementData.MovementSetting;
            var groundRaycastStartPoint = unitMovementSetting.GetGroundRaycastWorldPoint();

            movementData.IsGround = IsGround(
                groundRaycastStartPoint,
                unitMovementSetting.GroundRaycastRadius,
                unitMovementSetting.GroundRaycastDistance,
                out movementData.GroundHit);
            movementData.IsSlope = IsSlope(
                groundRaycastStartPoint,
                unitMovementSetting.SlopeRaycastRadius,
                unitMovementSetting.SlopeRaycastDistance,
                movementSetting.SlopeAngle,
                out movementData.SlopeHit);
        }

        public static void ResetRigibody(MovementData movementData)
        {
            if (movementData == null || !movementData.IsValid() || movementData.DisableResetVelocity)
                return;

            movementData.UnitMovementSetting.Rigidbody.velocity = Vector3.zero;
        }

        public static void Movement(MovementData movementData)
        {
            if (movementData == null || !movementData.IsValid())
                return;

            var unitMovementSetting = movementData.UnitMovementSetting;
            var rigidbody = unitMovementSetting.Rigidbody;
            var movement = movementData.GetForward(true) * movementData.Speed * movementData.SpeedRate;
            rigidbody.velocity += movement;

            RotateToForward(movementData, movementData.GetForward(false));
            RotateCharacterAvatarWithSlope(movementData);
        }

        public static void RotateToForward(MovementData movementData, Vector3 lookForward, bool immediate = false)
        {
            if (movementData == null || !movementData.IsValid())
                return;

            var movementSetting = movementData.MovementSetting;
            var unitMovementSetting = movementData.UnitMovementSetting;
            var rotateTrans = unitMovementSetting.RotateTransform;

            if (immediate)
            {
                var rotation = Quaternion.LookRotation(lookForward);
                rotateTrans.rotation = rotation;
                return;
            }

            if (movementData.MoveAxis.sqrMagnitude > 0)
            {
                var rotation = Quaternion.LookRotation(lookForward);
                var angle = Quaternion.Angle(rotation, rotateTrans.rotation);
                if (angle != 0)
                {
                    rotateTrans.rotation = Quaternion.Slerp(
                        rotateTrans.rotation,
                        rotation,
                        movementSetting.RotateDurationTimePreAngle * Time.deltaTime);
                }
                else
                {
                    rotateTrans.rotation = rotation;
                }
            }
        }

        public static void RotateCharacterAvatarWithSlope(MovementData movementData)
        {
            if (movementData == null || !movementData.IsValid())
                return;

            var unitMovementSetting = movementData.UnitMovementSetting;
            var rotateTrans = unitMovementSetting.RotateTransform;
            var avatarTrans = unitMovementSetting.AvatarTransform;

            if (movementData.IsSlope)
            {

                Vector3 projectedForward = Vector3.ProjectOnPlane(rotateTrans.forward, movementData.SlopeHit.normal).normalized;
                Quaternion targetRotation = Quaternion.LookRotation(projectedForward, movementData.SlopeHit.normal);
                avatarTrans.rotation = Quaternion.Slerp(avatarTrans.rotation, targetRotation, 10 * Time.deltaTime);
            }
            else
            {
                //清空
                avatarTrans.localRotation = Quaternion.identity;
            }
        }

        public static bool Jumping(MovementData movementData)
        {
            if (movementData == null || !movementData.IsValid())
                return false;

            var movementSetting = movementData.MovementSetting;
            var jumpElapsedTime = movementData.JumpData.JumpElapsedTime();
            var jumpVelocity = movementSetting.JumpVelocityCurve.Evaluate(jumpElapsedTime);
            movementData.UnitMovementSetting.Rigidbody.velocity += Vector3.up * jumpVelocity;
            return jumpElapsedTime < movementSetting.JumpVelocityCurve.LastKey().time;
        }

        public static void Dash(MovementData movementData)
        {
            if (movementData == null || !movementData.IsValid())
                return;

            var unitMovementSetting = movementData.UnitMovementSetting;
            var rigidbody = unitMovementSetting.Rigidbody;
            var forward = movementData.GetDashForward();
            RotateToForward(movementData, forward, true);
            //TODO use setting or character attribute?
            rigidbody.AddForce(forward * 20f, ForceMode.VelocityChange);
        }

        public static void FixGroundPoint(MovementData movementData)
        {
            if (movementData == null || !movementData.IsValid())
                return;

            if (!movementData.IsGround)
                return;

            var movementSetting = movementData.MovementSetting;
            var unitMovementSetting = movementData.UnitMovementSetting;
            float fixGroundY = movementData.GroundHit.point.y - unitMovementSetting.RootTransform.position.y;
            unitMovementSetting.Rigidbody.velocity += new Vector3(0, fixGroundY * movementSetting.FixGroundVelocity, 0);
        }

        public static void GravityFall(MovementData movementData)
        {
            if (movementData == null || !movementData.IsValid())
                return;

            var movementSetting = movementData.MovementSetting;
            var fallGravity = movementSetting.FallGravityCurve.Evaluate(movementData.FallData.FallElapsedTime());
            var gravity = Vector3.down * fallGravity;
            movementData.UnitMovementSetting.Rigidbody.velocity += gravity;
        }
    }
}
