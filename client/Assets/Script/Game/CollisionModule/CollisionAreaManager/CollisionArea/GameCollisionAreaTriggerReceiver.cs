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

            if (triggerInfo is AttackCastTriggerInfo attackTriggerInfo)
            {
                //後續有受擊觸發的在處理傷害計算時觸發
                if (!GameMainSystem.IsSelf(attackTriggerInfo.CasterUnitId) && GameMainSystem.IsSelf(unitId))
                {
                    Log.LogInfo($"ToCharacter Hit Unit Id:{unitId}, Damage:{20}");
                }
                else if (GameMainSystem.IsSelf(attackTriggerInfo.CasterUnitId) && GameMainSystem.IsEnemy(unitId))
                {
                    GameMainSystem.TestCauseToEnemyDamage(unitId, 20);
                }
                //只需要顯示受擊特效 只要位置
                AttackCastManager.CastAttack(attackTriggerInfo.OnHitCastId,
                    new CastAttackInfo()
                    {
                        SpeedRate = 1,
                        TransformInfo = new CastTransformInfo()
                        {
                            WorldPoint = hit.point,
                            Direction = hit.normal
                        }
                    });
                _hits.Add(new HitHint() { Time = Time.time, Hit = hit });
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
