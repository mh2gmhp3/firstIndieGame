using Extension;
using UnityEngine;

namespace UnitModule.Movement
{
    public static class ThreeDimensionalMovementUtility
    {
        public interface IMovementData
        {
            public IUnitMovementSetting UnitMovementSetting { get; }
            public MovementSetting MovementSetting { get; }

            public bool DisableResetVelocity { get; set; }

            public bool IsGround { get; set; }
            public RaycastHit GroundHit { get; set; }

            public bool IsSlope { get; set; }
            public RaycastHit SlopeHit { get; set; }

            public bool HaveMoveOperate();

            public Vector3 GetMovementForwardNormal();
            public Vector3 GetRotateForwardNormal();

            public float GetSpeed();
            public float GetDashForce();

            public float GetJumpElapsedTime();
            public float GetFallElapsedTime();
        }

        #region IMovementData Exention

        public static bool IsValid(this IMovementData movementData)
        {
            if (movementData.MovementSetting == null)
                return false;

            if (movementData.UnitMovementSetting == null)
                return false;

            return movementData.UnitMovementSetting.IsValid();
        }

        public static Vector3 GetMovementForwardNormal(this IMovementData movementData, bool withSlope)
        {
            Vector3 forward = movementData.GetMovementForwardNormal();
            if (withSlope)
                return movementData.GetOnSlopeNormal(forward);

            return forward;
        }

        /// <summary>
        /// 使用方向獲取與斜波的向量 如果站在斜坡上 沒有將直接回傳原向量
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static Vector3 GetOnSlopeNormal(this IMovementData movementData, Vector3 vector)
        {
            if (movementData.IsSlope)
                return Vector3.ProjectOnPlane(vector, movementData.SlopeHit.normal).normalized;
            return vector;
        }

        public static Vector3 GetDashForward(this IMovementData movementData)
        {
            if (movementData.HaveMoveOperate())
                return movementData.GetMovementForwardNormal(true);
            else
                return movementData.GetOnSlopeNormal(movementData.UnitMovementSetting.RotateTransform.forward);    // 沒輸入拿當前面向方向
        }

        public static float GetForwardAndRotateTransDirection(this IMovementData movementData)
        {
            return Vector3.Cross(movementData.UnitMovementSetting.RotateTransform.forward, movementData.GetMovementForwardNormal()).y;
        }

        public static void DrawDebugGizmos(this IMovementData movementData)
        {
            //Ground
            var rayStarPoint = movementData.UnitMovementSetting.GetGroundRaycastWorldPoint();
            var rayEndPoint = rayStarPoint +
                Vector3.down * movementData.UnitMovementSetting.GroundRaycastDistance;
            var groundRadius = movementData.UnitMovementSetting.GroundRaycastRadius;
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(rayStarPoint, movementData.UnitMovementSetting.GroundRaycastRadius);
            Gizmos.DrawLine(rayStarPoint + new Vector3(groundRadius, 0, 0), rayEndPoint + new Vector3(groundRadius, 0, 0));
            Gizmos.DrawLine(rayStarPoint + new Vector3(-groundRadius, 0, 0), rayEndPoint + new Vector3(-groundRadius, 0, 0));
            Gizmos.DrawLine(rayStarPoint + new Vector3(0, 0, groundRadius), rayEndPoint + new Vector3(0, 0, groundRadius));
            Gizmos.DrawLine(rayStarPoint + new Vector3(0, 0, -groundRadius), rayEndPoint + new Vector3(0, 0, -groundRadius));
            Gizmos.DrawWireSphere(rayEndPoint, movementData.UnitMovementSetting.GroundRaycastRadius);
            if (movementData.IsGround)
            {
                var rayHitPoint = movementData.GroundHit.point;
                Gizmos.color = Color.red;
                Gizmos.DrawLine(rayStarPoint, rayHitPoint);
            }
            if (movementData.IsSlope)
            {
                var rayHitPoint = movementData.SlopeHit.point;
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(rayStarPoint, rayHitPoint);
                Gizmos.DrawLine(rayHitPoint, rayHitPoint + movementData.SlopeHit.normal * 2f);
                Vector3 projectedForward = Vector3.ProjectOnPlane(
                    movementData.UnitMovementSetting.RotateTransform.forward,
                    movementData.SlopeHit.normal).normalized;
                Gizmos.DrawLine(rayHitPoint, rayHitPoint + projectedForward * 2f);
            }
        }

        #endregion

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

        public static void UpdateState(IMovementData movementData)
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
                out var groundHit);
            movementData.GroundHit = groundHit;
            movementData.IsSlope = IsSlope(
                groundRaycastStartPoint,
                unitMovementSetting.SlopeRaycastRadius,
                unitMovementSetting.SlopeRaycastDistance,
                movementSetting.SlopeAngle,
                out var slopeHit);
            movementData.SlopeHit = slopeHit;
        }

        public static void ResetRigibody(IMovementData movementData)
        {
            if (movementData == null || !movementData.IsValid() || movementData.DisableResetVelocity)
                return;

            movementData.UnitMovementSetting.Rigidbody.velocity = Vector3.zero;
        }

        public static void Movement(IMovementData movementData)
        {
            if (movementData == null || !movementData.IsValid())
                return;

            var unitMovementSetting = movementData.UnitMovementSetting;
            var rigidbody = unitMovementSetting.Rigidbody;
            var movement = movementData.GetMovementForwardNormal(true) * movementData.GetSpeed();
            rigidbody.velocity += movement;

            RotateToForward(movementData, movementData.GetRotateForwardNormal());
            RotateCharacterAvatarWithSlope(movementData);
        }

        public static void RotateToForward(IMovementData movementData, Vector3 lookForward, bool immediate = false)
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

            if (movementData.HaveMoveOperate())
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

        public static void RotateCharacterAvatarWithSlope(IMovementData movementData)
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
                avatarTrans.rotation = Quaternion.Slerp(avatarTrans.rotation, targetRotation, 5 * Time.deltaTime);
            }
            else
            {
                //清空
                avatarTrans.localRotation = Quaternion.Slerp(avatarTrans.localRotation, Quaternion.identity, 5 * Time.deltaTime);
            }
        }

        public static bool Jumping(IMovementData movementData)
        {
            if (movementData == null || !movementData.IsValid())
                return false;

            var movementSetting = movementData.MovementSetting;
            var jumpElapsedTime = movementData.GetJumpElapsedTime();
            var jumpVelocity = movementSetting.JumpVelocityCurve.Evaluate(jumpElapsedTime);
            movementData.UnitMovementSetting.Rigidbody.velocity += Vector3.up * jumpVelocity;
            return jumpElapsedTime < movementSetting.JumpVelocityCurve.LastKey().time;
        }

        public static void Dash(IMovementData movementData)
        {
            if (movementData == null || !movementData.IsValid())
                return;

            var unitMovementSetting = movementData.UnitMovementSetting;
            var rigidbody = unitMovementSetting.Rigidbody;
            var forward = movementData.GetDashForward();
            RotateToForward(movementData, forward, true);
            rigidbody.AddForce(forward * movementData.GetDashForce(), ForceMode.VelocityChange);
        }

        public static void FixGroundPoint(IMovementData movementData)
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

        public static void GravityFall(IMovementData movementData)
        {
            if (movementData == null || !movementData.IsValid())
                return;

            var movementSetting = movementData.MovementSetting;
            var fallElapsedTime = movementData.GetFallElapsedTime();
            var fallGravity = movementSetting.FallGravityCurve.Evaluate(fallElapsedTime);
            var gravity = Vector3.down * fallGravity;
            movementData.UnitMovementSetting.Rigidbody.velocity += gravity;
        }
    }
}
