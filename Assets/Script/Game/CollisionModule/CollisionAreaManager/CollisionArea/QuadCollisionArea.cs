using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace CollisionModule
{
    public class QuadCollisionAreaSetupData : ICollisionAreaSetupData
    {
        public int AreaType { get => (int)CollisionAreaDefine.AreaType.Quad; }

        public Vector3 WorldPosition { get; set; }
        public Vector3 Direction { get; set; }
        public float TimeDuration { get; set; }

        public ICollisionAreaTriggerReceiver TriggerReceiver { get; set; }

        public float Width;
        public float Distance;

        public QuadCollisionAreaSetupData(Vector3 worldPos, Vector3 direction, float duration, float width, float distance, ICollisionAreaTriggerReceiver receiver)
        {
            WorldPosition = worldPos;
            Direction = direction;

            TimeDuration = duration;

            Width = width;
            Distance = distance;

            TriggerReceiver = receiver;
        }
    }

    [CollisionArea((int)CollisionAreaDefine.AreaType.Quad)]
    public class QuadCollisionArea : CollisionArea
    {
        private float _width;
        private float _distance;

        private Vector3 _curPos;

        //讓對同群組的只會碰撞一次
        //TODO 可能都會使用? 泛用的話拉到底層
        private HashSet<int> _colliedGroupIdSet = new HashSet<int>();

        public QuadCollisionArea(ICollisionAreaManager collisionAreaManager, int areaType) : base(collisionAreaManager, areaType)
        {
        }

        protected override void DoSetup(ICollisionAreaSetupData setupData)
        {
            if (setupData is QuadCollisionAreaSetupData areaSetupData)
            {
                _width = areaSetupData.Width;
                _distance = areaSetupData.Distance;
            }
        }

        public override void DoCreate()
        {
            _colliedGroupIdSet.Clear();
        }

        public override void DoUpdate()
        {
            var dir = Quaternion.Euler(0, 90, 0) * _direction;
            var startPos = _worldPosition + dir * (_width / 2);
            var endPos = _worldPosition - dir * (_width / 2);
            _curPos = Vector3.Lerp(startPos, endPos, (Time.time - _startTime) / _timeDuration);
            if (!Physics.Raycast(_curPos, _direction, out var hit, _distance))
            {
                return;
            }

            if (!TryGetColliderId(hit.colliderInstanceID, out int groupId, out int colliderId))
                return;

            if (_colliedGroupIdSet.Contains(groupId))
                return;

            _colliedGroupIdSet.Add(groupId);

            NotifyTriggerReceiver(groupId, colliderId);
        }

        public override void DoDrawGizmos()
        {
            Debug.DrawLine(_curPos, _curPos + _direction * _distance, Color.blue);
        }
    }
}
