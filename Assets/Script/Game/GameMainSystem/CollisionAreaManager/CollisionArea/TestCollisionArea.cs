using Logging;
using Mono.Cecil;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameMainModule.Collision
{
    public class TestCollisionAreaTriggerReceiver : ICollisionAreaTriggerReceiver
    {
        public void OnTrigger(int groupId, int colliderId)
        {
            Log.LogInfo($"OnTrigger GroupId:{groupId} ColliderId:{colliderId}", true);
        }
    }

    public class TestCollisionAreaSetupData : ICollisionAreaSetupData
    {
        public int AreaType { get => (int)CollisionAreaDefine.AreaType.Test; }

        public Vector3 WorldPosition { get; set; }
        public Vector3 Direction { get; set; }
        public float TimeDuration { get; set; }

        public ICollisionAreaTriggerReceiver TriggerReceiver { get; set; }

        public TestCollisionAreaSetupData(float direction)
        {
            WorldPosition = Vector3.zero + new Vector3(0, 1, 0);
            Direction = Vector3.forward;

            TimeDuration = direction;
        }
    }

    [CollisionArea((int)CollisionAreaDefine.AreaType.Test)]
    public class TestCollisionArea : CollisionArea
    {
        private RaycastHit _hit;

        //讓對同群組的只會碰撞一次
        //TODO 可能都會使用? 泛用的話拉到底層
        private HashSet<int> _colliedGroupIdSet = new HashSet<int>();

        public TestCollisionArea(ICollisionAreaManager collisionAreaManager, int areaType) : base(collisionAreaManager, areaType)
        {
        }

        protected override void DoSetup(ICollisionAreaSetupData setupData)
        {
            _colliedGroupIdSet.Clear();
        }

        public override void DoUpdate()
        {
            int timeValue = (int)(ProgressRate * 100);
            bool trigger = timeValue % 2 == 0;
            if (!trigger)
            {
                _hit = default;
                return;
            }

            if (!Physics.Raycast(
                _worldPosition,
                _direction,
                out _hit,
                1))
            {
                return;
            }

            if (!TryGetColliderId(_hit.colliderInstanceID, out int groupId, out int colliderId))
                return;

            if (_colliedGroupIdSet.Contains(groupId))
                return;

            _colliedGroupIdSet.Add(groupId);

            NotifyTriggerReceiver(groupId, colliderId);
        }

        public override void DoDrawGizmos()
        {
            var oriColor = Gizmos.color;
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_worldPosition, 0.1f);
            if (_hit.collider == null)
                return;

            Debug.DrawLine(_worldPosition, _hit.point, Color.red);
            Gizmos.DrawWireSphere(_hit.point, 0.1f);
            Gizmos.color = oriColor;
        }
    }
}
