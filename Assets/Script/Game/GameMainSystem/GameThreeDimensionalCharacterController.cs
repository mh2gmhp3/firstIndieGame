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
        /// 角色Root Transform
        /// </summary>
        private Transform _characterTrans = null;

        /// <summary>
        /// 角色Root Rigidbody
        /// </summary>
        private Rigidbody _characterRigidbody = null;

        /// <summary>
        /// 移動輸入軸
        /// </summary>
        private Vector3 _moveAxis = Vector3.zero;
        /// <summary>
        /// 移動選轉方向
        /// </summary>
        private Quaternion _moveQuaternion = Quaternion.identity;

        public void DoFixedUpdate()
        {
            if (!_enable)
                return;

            if (_characterRigidbody == null)
                return;

            //之後要考慮其他狀態 目前只處裡操作移動 需考慮外在因素引響的移動
            float speed = 10;
            Vector3 moveForward = _moveQuaternion * _moveAxis;
            _characterRigidbody.velocity = moveForward * speed;
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
            _characterTrans = root.transform;
            _characterRigidbody = root.GetComponent<Rigidbody>();
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
    }
}
