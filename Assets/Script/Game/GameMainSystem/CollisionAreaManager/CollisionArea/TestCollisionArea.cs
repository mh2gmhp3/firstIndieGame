using Mono.Cecil;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameMainSystem.Collision
{
    public class TestCollisionAreaSetupData : ICollisionAreaSetupData
    {
        public int AreaType { get => (int)CollisionAreaDefine.AreaType.Test; }

        public Vector3 WorldPosition { get; set; }
        public Vector3 Direction { get; set; }
        public float TimeDuration { get; set; }

        public ICollisionAreaTriggerReceiver TriggerReceiver { get; set; }

        public TestCollisionAreaSetupData()
        {
            WorldPosition = Vector3.zero + new Vector3(0, 1, 0);
            Direction = Vector3.forward;

            TimeDuration = 10;
        }
    }

    [CollisionArea((int)CollisionAreaDefine.AreaType.Test)]
    public class TestCollisionArea : CollisionArea
    {
        private RaycastHit _hit;

        public TestCollisionArea(ICollisionAreaManager collisionAreaManager, int areaType) : base(collisionAreaManager, areaType)
        {
        }

        protected override void DoSetup(ICollisionAreaSetupData setupData)
        {

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

            Physics.Raycast(
                _worldPosition,
                _direction,
                out _hit,
                1);
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
