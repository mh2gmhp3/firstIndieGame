using GameMainModule;
using GameMainModule.Attack;
using GameSystem;
using Logging;
using System.Collections.Generic;
using UnityEngine;

namespace CollisionModule
{
    public class GameCollisionAreaTriggerReceiver : ICollisionAreaTriggerReceiver, IUpdateTarget
    {
        private struct HitHint
        {
            public float Time;
            public RaycastHit Hit;

            public bool IsEnd()
            {
                return UnityEngine.Time.time - Time > 0.2f;
            }
        }
        private List<HitHint> _hits = new List<HitHint>();

        public GameCollisionAreaTriggerReceiver()
        {

        }

        public void OnTrigger(int groupId, int colliderId, RaycastHit hit, ICollisionAreaTriggerInfo triggerInfo)
        {
            if (!GameMainSystem.TryGetUnitIdByColliderGroupId(groupId, out var unitId))
                return;

            if (triggerInfo is FakeCharacterTriggerInfo fakeTriggerInfo)
            {
                if (fakeTriggerInfo.ToCharacter)
                {
                    Log.LogInfo($"ToCharacter Hit Unit Id:{unitId}, Damage:{fakeTriggerInfo.Attack}");
                }
                else
                {
                    GameMainSystem.TestCauseToEnemyDamage(unitId, fakeTriggerInfo.Attack);
                }
                _hits.Add(new HitHint() { Time = Time.time, Hit = hit });
                //Log.LogInfo($"Hit Unit Id:{unitId}, Damage:{fakeTriggerInfo.Attack}");
            }
        }

        void IUpdateTarget.DoDrawGizmos()
        {
            for (int i = _hits.Count - 1; i >= 0; i--)
            {
                var hint = _hits[i];
                var hit = hint.Hit;
                Gizmos.DrawWireSphere(hit.point, ((Time.time - hint.Time) / 0.2f) * 0.5f);
                Gizmos.DrawLine(hit.point, hit.point + hit.normal * 2f);
                if (hint.IsEnd())
                {
                    _hits.RemoveAt(i);
                }
            }
        }

        void IUpdateTarget.DoFixedUpdate()
        {

        }

        void IUpdateTarget.DoLateUpdate()
        {

        }

        void IUpdateTarget.DoOnGUI()
        {

        }

        void IUpdateTarget.DoUpdate()
        {

        }
    }
}
