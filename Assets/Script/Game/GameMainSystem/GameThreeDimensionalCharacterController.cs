using Logging;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameMainSystem
{
    /// <summary>
    /// 三維空間的角色控制器
    /// </summary>
    public class GameThreeDimensionalCharacterController
    {
        /// <summary>
        /// 啟用
        /// </summary>
        private bool _enable = false;

        /// <summary>
        /// Root Transform
        /// </summary>
        private Transform _rootTrans = null;

        /// <summary>
        /// Root Rigidbody
        /// </summary>
        private Rigidbody _rootRigidbody = null;

        /// <summary>
        /// Character Transform
        /// </summary>
        private Transform _characterTrans = null;

        /// <summary>
        /// 移動輸入軸
        /// </summary>
        private Vector3 _moveAxis = Vector3.zero;
        /// <summary>
        /// 移動選轉方向
        /// </summary>
        private Quaternion _moveQuaternion = Quaternion.identity;

        /// <summary>
        /// 是否在地面
        /// </summary>
        private bool _isGround = false;

        private float _characterRotateDurationTimePreAngle = 0.2f;
        private float _characterRotateStartTime = 0;
        private float _characterRotateEndTime = 0f;

        private bool _isFalling = false;
        private float _fallingMaxTime = 0.3f;
        private float _fallingStartTime = 0;
        private float _fallingForce = 5;
        private float _fallingMaxForce = 20;

        private bool _inputtedJump = false;
        private bool _isJumping = false;
        private float _jumpLimitTime = 0.3f;
        private float _jumpStartTime = 0;
        private float _jumpForce = 10;
        private float _jumpWeak = 5f;
        private int _jumpMaxCount = 2;
        private int _jumpCount = 0;

        private bool _isSlope = false;
        private RaycastHit _slopHit;

        private Vector3 RootForward => _moveQuaternion * Vector3.forward;

        public void DoUpdate()
        {
            if (!_enable)
                return;

            if (_rootTrans == null)
                return;

            var rayStartPoint = _rootTrans.position + Vector3.up * 0.01f;

            _isGround = Physics.Raycast(
                rayStartPoint,
                Vector3.down,
                0.2f);

            if (Physics.Raycast(
                rayStartPoint,
                new Vector3(_characterTrans.forward.x, 0, _characterTrans.forward.z),
                out _slopHit,
                1))
            {
                float angle = Vector3.Angle(Vector3.up, _slopHit.normal);
                _isSlope = angle != 0 && angle < 60f;
            }
            else
            {
                _isSlope = false;
            }
        }

        public void DoFixedUpdate()
        {
            if (!_enable)
                return;

            if (_rootRigidbody == null)
                return;

            //movement
            float speed = 10;
            Vector3 moveForward = _moveQuaternion * _moveAxis;
            if (_isSlope)
            {
                moveForward = Vector3.ProjectOnPlane(moveForward, _slopHit.normal).normalized;
            }

            var movement = moveForward * speed;
            _rootRigidbody.velocity = movement;

            //rotate character
            if (_moveAxis.sqrMagnitude > 0)
            {
                var rotation = Quaternion.LookRotation(moveForward);
                var angle = Quaternion.Angle(rotation, _characterTrans.rotation);
                if (angle != 0)
                {

                    _characterRotateStartTime = Time.time;
                    _characterRotateEndTime = Time.time + (angle * _characterRotateDurationTimePreAngle);

                    //Log.LogInfo(
                    //    $"rotation:{rotation} " +
                    //    $"Angle:{Quaternion.Angle(rotation, _characterTrans.rotation)} " +
                    //    $"StartTime:{_characterRotateStartTime} " +
                    //    $"EndTime:{_characterRotateEndTime} " +
                    //    $"D:{Mathf.Clamp01(_characterRotateStartTime / _characterRotateEndTime)} " +
                    //    $"TargetAng:{rotation.eulerAngles} " +
                    //    $"CharacterAngle:{_characterTrans.rotation.eulerAngles}");

                    _characterTrans.rotation = Quaternion.Lerp(
                        _characterTrans.rotation,
                        rotation,
                        Mathf.Clamp01(_characterRotateStartTime / _characterRotateEndTime));
                }
                else
                {
                    _characterTrans.rotation = rotation;
                }
            }

            //gravity falling
            if (!_isGround && !_isJumping)
            {
                if (!_isFalling)
                {
                    _isFalling = true;
                    _fallingStartTime = Time.time;
                }
                else
                {
                    var fallingElapsedTime = Time.time - _fallingStartTime;
                    var fallingForceValue = Mathf.Lerp(
                        _fallingForce,
                        _fallingMaxForce,
                        Mathf.Clamp01(fallingElapsedTime / _fallingMaxTime));

                    var gravity = Vector3.down * fallingForceValue;
                    _rootRigidbody.velocity += gravity;
                }
            }
            else
            {
                _isFalling = false;
            }

            if (_inputtedJump)
            {
                _isJumping = true;
                _inputtedJump = false;
                _jumpStartTime = Time.time;
            }

            //jump
            if (_isJumping)
            {
                var jumpElapsedTime = Time.time - _jumpStartTime;
                var jumpForceVaule = Mathf.Lerp(
                    _jumpForce,
                    _jumpWeak,
                    Mathf.Clamp01(jumpElapsedTime / _jumpLimitTime));
                _rootRigidbody.velocity += Vector3.up * jumpForceVaule;

                if (jumpElapsedTime > _jumpLimitTime)
                    _isJumping = false;
            }

            //reset jump
            if (_isGround && !_isJumping && !_isFalling)
                _jumpCount = 0;
        }

        public void DoOnGUI()
        {
#if UNITY_EDITOR
            if (!_enable)
                return;

            if (_rootTrans == null)
                return;

            var rayStartPoint = _rootTrans.position + Vector3.up * 0.01f;
            string hitInfo =
                $"colliderInstanceID:{_slopHit.colliderInstanceID}\n" +
                $"point:{_slopHit.point}\n" +
                $"normal:{_slopHit.normal}\n" +
                $"distance:{_slopHit.distance}\n" +
                $"angle:{Vector3.Angle(Vector3.up, _slopHit.normal)}";
            GUI.TextArea(new Rect(25, 25, 100, 200), hitInfo);
            Debug.DrawLine(
                rayStartPoint,
                rayStartPoint + new Vector3(_characterTrans.forward.x, 0, _characterTrans.forward.z) * 1,
                Color.green,
                0.1f);
#endif
        }

        /// <summary>
        /// 設定啟用 預設為關閉
        /// </summary>
        /// <param name="enable"></param>
        public void SetEnable(bool enable)
        {
            _enable = enable;
        }

        /// <summary>
        /// 設定角色Root
        /// </summary>
        /// <param name="transform"></param>
        public void SetCharacterRoot(GameObject root)
        {
            _rootTrans = root.transform;
            _rootRigidbody = root.GetComponent<Rigidbody>();

            //TODO setting or mono
            _characterTrans = _rootTrans.GetChild(0);
        }

        /// <summary>
        /// 設定移動輸入軸
        /// </summary>
        /// <param name="axis"></param>
        public void SetMoveAxis(Vector3 axis)
        {
            _moveAxis = axis;
        }

        /// <summary>
        /// 設定移動選轉方向
        /// </summary>
        /// <param name="quaternion"></param>
        public void SetMoveQuaternion(Quaternion quaternion)
        {
            //只取Y軸
            _moveQuaternion = new Quaternion(0, quaternion.y, 0, quaternion.w);
        }

        public void Jump()
        {
            if (_jumpCount >= _jumpMaxCount)
                return;

            _jumpCount++;
            _inputtedJump = true;
        }
    }
}
