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
    }

    public partial class BaseCameraBehavior
    {
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

            public void Update()
            {
                if (_targetTrans == null)
                    return;

                var diff = _targetTrans.position - _baseCameraBehavior._cameraTrans.position;
                var diffNormalize = diff.normalized;
                if (diffNormalize.x == 0 && diffNormalize.y == 0)
                {
                    diffNormalize = _baseCameraBehavior._cameraTrans.forward;
                }
                else
                {
                    _baseCameraBehavior._cameraTrans.forward = diffNormalize;
                }

                _baseCameraBehavior._cameraTrans.position =
                    _targetTrans.position - diffNormalize * _distance;
            }
        }

        private FollowTargetProcessor _followTargetProcessor;

        [CameraCommand((int)CameraCommandDefine.BaseCommand.FollowTarget)]
        protected virtual void FollowTarget(ICameraCommand command)
        {
            if (!(command is IFollowTarget followTarget))
                return;

            _followTargetProcessor.SetFollowTargetAndDistance(
                followTarget.TargetTrans,
                followTarget.Distance);

            _state |= BaseCameraState.FollowTarget;
        }
    }
}
