using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnitModule.Movement
{
    [Serializable]
    public class ThreeDimensionalMovement : IMovement
    {
        /// <summary>
        /// 啟用
        /// </summary>
        [SerializeField]
        private bool _enable = false;

        /// <summary>
        /// Root Transform
        /// </summary>
        [SerializeField]
        private Transform _rootTrans = null;

        /// <summary>
        /// Root Rigidbody
        /// </summary>
        [SerializeField]
        private Rigidbody _rootRigidbody = null;

        /// <summary>
        /// Character Transform
        /// </summary>
        [SerializeField]
        private Transform _characterTrans = null;

        /// <summary>
        /// 移動動畫控制
        /// </summary>
        [SerializeField]
        private IMovementAnimationController _movementAnimationController = null;

        /// <summary>
        /// 移動輸入軸
        /// </summary>
        [SerializeField]
        private Vector3 _moveAxis = Vector3.zero;
        /// <summary>
        /// 移動選轉方向
        /// </summary>
        [SerializeField]
        private Quaternion _moveQuaternion = Quaternion.identity;

        /// <summary>
        /// 是否在地面
        /// </summary>
        [SerializeField]
        private bool _isGround = false;
        [SerializeField]
        private float _isGroundCheckDistance = 0.65f;
        [SerializeField]
        private RaycastHit _groundHits;

        [SerializeField]
        private float _moveSpeed = 8f;

        [SerializeField]
        private float _airSpeedMultiplier = 0.7f;

        [SerializeField]
        private float _rotateDurationTimePreAngle = 10f;
        [SerializeField]
        private float _rotateStartTime = 0;
        [SerializeField]
        private float _rotateEndTime = 0f;

        [SerializeField]
        private bool _isFalling = false;
        [SerializeField]
        private float _fallingMaxTime = 0.5f;
        [SerializeField]
        private float _fallingStartTime = 0;
        [SerializeField]
        private float _fallingForce = 5;
        [SerializeField]
        private float _fallingMaxForce = 15;

        [SerializeField]
        private bool _inputtedJump = false;
        [SerializeField]
        private bool _isJumping = false;
        [SerializeField]
        private float _jumpLimitTime = 0.3f;
        [SerializeField]
        private float _jumpStartTime = 0;
        [SerializeField]
        private float _jumpForce = 10;
        [SerializeField]
        private float _jumpWeak = 5f;
        [SerializeField]
        private int _jumpMaxCount = 2;
        [SerializeField]
        private int _jumpCount = 0;

        [SerializeField]
        private bool _isSlope = false;
        [SerializeField]
        private float _slopeAngle = 45f;
        [SerializeField]
        private RaycastHit _slopHit;

        [SerializeField]
        private float _landingStiffTime = 0.5f;
        [SerializeField]
        private float _landingStartTime = 0f;
        [SerializeField]
        private bool _landingStiff = false;

        public ThreeDimensionalMovement()
        {
            _movementAnimationController = new DefaultMovementAnimationController();
        }

        public void DoUpdate()
        {
            if (!_enable)
                return;

            if (_rootTrans == null)
                return;

            var rayStartPoint = _rootTrans.position + Vector3.up * 0.5f;

            if (Physics.Raycast(
                rayStartPoint,
                Vector3.down,
                out _slopHit,
                1))
            {
                float angle = Vector3.Angle(Vector3.up, _slopHit.normal);
                _isSlope = angle != 0 && angle < _slopeAngle;
            }
            else
            {
                _isSlope = false;
            }

            if (Physics.Raycast(
                rayStartPoint,
                Vector3.down,
                out _groundHits,
                _isGroundCheckDistance))
            {
                _isGround = true;//(_groundHits.point - _rootTrans.position).sqrMagnitude < (_isSlope ? 0.1 : 0.01);
            }
            else
            {
                _isGround = false;
            }
        }

        public void DoFixedUpdate()
        {
            if (!_enable)
                return;

            if (_rootRigidbody == null)
                return;

            //movement
            float speedMultiplier = _isGround ? 1 : _airSpeedMultiplier;
            float speed = _moveSpeed * speedMultiplier;
            Vector3 moveForward = _moveQuaternion * _moveAxis;
            Vector3 lookForward = moveForward;
            if (_isSlope)
            {
                moveForward = Vector3.ProjectOnPlane(moveForward, _slopHit.normal).normalized;
            }

            var movement = moveForward * speed;

            //landing effect Movement
            if (_landingStiff)
            {
                if (Time.time - _landingStartTime >= _landingStiffTime)
                {
                    _landingStiff = false;
                }
            }
            if (_landingStiff)
            {
                //清空移動輸入的方向
                movement = Vector3.zero;
            }
            else
            {
                //沒有僵直才發出移動動畫通知
                _movementAnimationController.MoveInput(_moveAxis, _moveQuaternion);
            }


            _rootRigidbody.velocity = movement;

            //rotate character
            if (_moveAxis.sqrMagnitude > 0)
            {
                var rotation = Quaternion.LookRotation(lookForward);
                var angle = Quaternion.Angle(rotation, _characterTrans.rotation);
                if (angle != 0)
                {

                    _rotateStartTime = Time.time;
                    _rotateEndTime = Time.time + (angle * _rotateDurationTimePreAngle);

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
                        _rotateDurationTimePreAngle * Time.deltaTime);
                    //Mathf.Clamp01(_characterRotateStartTime / _characterRotateEndTime));
                }
                else
                {
                    _characterTrans.rotation = rotation;
                }
            }

            bool oriIsJumping = _isJumping;
            bool oriIsFalling = _isFalling;

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
                    _movementAnimationController.OnFalling(fallingElapsedTime, _fallingMaxTime);
                }
            }
            else
            {
                _isFalling = false;
            }

            if (!_isGround && _groundHits.collider != null &&(_groundHits.point - _rootTrans.position).sqrMagnitude > 0.01)
            {
                float fixGroundY = _groundHits.point.y - _rootTrans.position.y;
                _rootRigidbody.velocity += new Vector3(0, fixGroundY, 0);
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

                _movementAnimationController.OnJumping(_jumpCount, jumpElapsedTime, _jumpLimitTime);
            }

            //reset jump
            if (_isGround && !_isJumping && !_isFalling)
            {
                _jumpCount = 0;
                if (oriIsJumping || oriIsFalling)
                {
                    _movementAnimationController.OnLand();
                    _landingStartTime = Time.time;
                    _landingStiff = true;
                }
            }
        }

        public void DoOnGUI()
        {
#if UNITY_EDITOR
            if (!_enable)
                return;

            if (_rootTrans == null)
                return;

            var rayStartPoint = _rootTrans.position + Vector3.up * 0.5f;
            string hitInfo =
                $"colliderInstanceID:{_slopHit.colliderInstanceID}\n" +
                $"point:{_slopHit.point}\n" +
                $"normal:{_slopHit.normal}\n" +
                $"distance:{_slopHit.distance}\n" +
                $"angle:{Vector3.Angle(Vector3.up, _slopHit.normal)}";
            GUI.TextArea(new Rect(25, 25, 100, 200), hitInfo);
            Debug.DrawLine(
                rayStartPoint,
                rayStartPoint + Vector3.down * _isGroundCheckDistance,
                Color.green,
                0.1f);
#endif
        }


        public void SetEnable(bool enable)
        {
            _enable = enable;
        }

        public void SetMovementUnitData(UnitData unitData)
        {
            _rootTrans = unitData.Transform;
            _rootRigidbody = unitData.Rigidbody;

            _characterTrans = unitData.RotateTransform;
        }

        public void SetMovementAnimationController(IMovementAnimationController movementAnimationController)
        {
            //null不設定 避免為null的狀態
            if (movementAnimationController == null)
                return;

            _movementAnimationController = movementAnimationController;
        }

        public void SetMoveAxis(Vector3 axis)
        {
            _moveAxis = axis;
        }

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