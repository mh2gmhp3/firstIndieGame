using GameMainModule;
using GameMainModule.Attack;
using Logging;
using UnityEngine;

namespace CollisionModule
{
    public class GameCollisionAreaTriggerReceiver : ICollisionAreaTriggerReceiver
    {
        public GameCollisionAreaTriggerReceiver()
        {

        }

        public void OnTrigger(int groupId, int colliderId, RaycastHit hit, ICollisionAreaTriggerInfo triggerInfo)
        {
            if (!GameMainSystem.TryGetUnitIdByColliderGroupId(groupId, out var unitId))
                return;

            if (triggerInfo is FakeCharacterTriggerInfo fakeTriggerInfo)
            {
                GameMainSystem.TestCauseDamage(unitId, fakeTriggerInfo.Attack);
                //Log.LogInfo($"Hit Unit Id:{unitId}, Damage:{fakeTriggerInfo.Attack}");
            }
        }
    }
}
