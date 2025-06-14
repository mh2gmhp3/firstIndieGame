using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CameraModule
{
    public interface IFollowTarget : ICameraCommand
    {
        public Transform TargetTrans { get; set; }
        public float Distance { get; set; }
    }
    public class FollowTarget : IFollowTarget
    {
        public int CommandId { get; set; }
        public Transform TargetTrans { get; set; }
        public float Distance { get; set; }

        public FollowTarget()
        {
            CommandId = (int)CameraCommandDefine.BaseCommand.FollowTarget;
        }
    }

    public class FollowTargetProcessor
    {
        private BaseCameraBehavior _baseCameraBehavior;

        private Transform _targetTrans;
        private float _distance;

        public FollowTargetProcessor(BaseCameraBehavior baseCameraBehavior)
        {
            _baseCameraBehavior = baseCameraBehavior;
        }

        public void SetFollowTargetAndDistance(Transform transform, float distance)
        {
            _targetTrans = transform;
            _distance = distance;
        }

        public void LateUpdate()
        {
            if (_targetTrans == null)
                return;

            var diff = _targetTrans.position - _baseCameraBehavior.CameraTrans.position;
            var diffNormalize = diff.normalized;
            if (diffNormalize.x == 0 && diffNormalize.y == 0)
            {
                diffNormalize = _baseCameraBehavior.CameraTrans.forward;
            }
            else
            {
                _baseCameraBehavior.CameraTrans.forward = diffNormalize;
            }

            _baseCameraBehavior.CameraTrans.position =
                _targetTrans.position - diffNormalize * _distance;
        }
    }
}
