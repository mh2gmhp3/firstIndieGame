using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameMainSystem
{
    public class GameThreeDimensionalCharacterController
    {
        private bool _enable = false;

        private Transform _characterTrans = null;

        private Vector3 _moveAxis = Vector3.zero;
        private Quaternion _moveQuaternion = Quaternion.identity;

        public void DoLateUpdate()
        {
            if (!_enable)
                return;

            if (_characterTrans == null)
                return;

            //之後要考慮其他狀態 目前只處裡操作移動 需考慮外在因素引響的移動
            float speed = 10;
            Vector3 moveForward = _moveQuaternion * _moveAxis;
            _characterTrans.position += moveForward * speed * Time.deltaTime;
        }

        public void SetEnable(bool enable)
        {
            _enable = enable;
        }

        public void SetCharacterTransform(Transform transform)
        {
            _characterTrans = transform;
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
    }
}
