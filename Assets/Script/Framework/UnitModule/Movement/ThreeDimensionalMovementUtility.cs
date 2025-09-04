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

            public void StartJump()
            {
                JumpCount++;
                JumpStartTime = Time.time;
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

        [Serializable]
        public class MovementData
        {
            public UnitMovementSetting UnitMovementSetting;
            public MovementSetting MovementSetting;

            public JumpData JumpData;
            public FallData FallData;
            public LandData LandData;

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

            public bool Run = false;

            #endregion

            public bool IsGround;
            public RaycastHit GroundHit;

            public bool IsSlope;
            public RaycastHit SlopeHit;

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

            public MovementData(UnitMovementSetting unitMovementSetting, MovementSetting movementSetting)
            {
                UnitMovementSetting = unitMovementSetting;
                MovementSetting = movementSetting;
                JumpData = new JumpData();
                FallData = new FallData();
                LandData = new LandData();
            }
        }

        public static bool IsGround(Vector3 startPoint, float distance, out RaycastHit hit)
        {
            return Physics.Raycast(
                startPoint,
                Vector3.down,
                out hit,
                distance);
        }

        public static bool IsSlope(Vector3 startPoint, float distance, float angle, out RaycastHit hit)
        {
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
                unitMovementSetting.GroundRaycastDistance,
                out movementData.GroundHit);
            movementData.IsSlope = IsSlope(
                groundRaycastStartPoint,
                unitMovementSetting.SlopeRaycastDistance,
                movementSetting.SlopeAngle,
                out movementData.SlopeHit);
        }

        public static void ResetRigibidy(MovementData movementData)
        {
            if (movementData == null || !movementData.IsValid())
                return;

            movementData.UnitMovementSetting.Rigidbody.velocity = Vector3.zero;
        }

        public static void Movement(MovementData movementData)
        {
            if (movementData == null || !movementData.IsValid())
                return;

            var movementSetting = movementData.MovementSetting;
            var unityMovementSetting = movementData.UnitMovementSetting;
            var rigidbody = unityMovementSetting.Rigidbody;
            var rotateTrans = unityMovementSetting.RotateTransform;

            Vector3 moveForward = movementData.MoveQuaternion * movementData.MoveAxis;
            Vector3 lookForward = moveForward;
            if (movementData.IsSlope)
            {
                moveForward = Vector3.ProjectOnPlane(moveForward, movementData.SlopeHit.normal).normalized;
            }

            var movement = moveForward * movementData.Speed;
            rigidbody.velocity += movement;

            //Rotate
            if (movementData.MoveAxis.sqrMagnitude > 0)
            {
                var rotation = Quaternion.LookRotation(lookForward);
                var angle = Quaternion.Angle(rotation, rotateTrans.rotation);
                if (angle != 0)
                {
                    rotateTrans.rotation = Quaternion.Lerp(
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

        public static bool Jump(MovementData movementData)
        {
            if (movementData == null || !movementData.IsValid())
                return false;

            var movementSetting = movementData.MovementSetting;
            var jumpElapedTime = movementData.JumpData.JumpElapsedTime();
            var jumpVelocity = movementSetting.JumpVelocityCurve.Evaluate(jumpElapedTime);
            movementData.UnitMovementSetting.Rigidbody.velocity += Vector3.up * jumpVelocity;
            return jumpElapedTime < movementSetting.JumpVelocityCurve.keys[movementSetting.JumpVelocityCurve.length - 1].time;
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
