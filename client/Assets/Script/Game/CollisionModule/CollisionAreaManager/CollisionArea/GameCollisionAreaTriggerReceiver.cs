using GameMainModule;
using GameMainModule.Attack;
using Logging;
using UnityEngine;

namespace CollisionModule
{
    public class GameCollisionAreaTriggerReceiver : ICollisionAreaTriggerReceiver
    {
        private GameMainSystem _gameMainSystem;

        public GameCollisionAreaTriggerReceiver(GameMainSystem gameMainSystem)
        {
            _gameMainSystem = gameMainSystem;
        }

        public void OnTrigger(int groupId, int colliderId, RaycastHit hit, ICollisionAreaTriggerInfo triggerInfo)
        {
            if (!_gameMainSystem.UnitManager.TryGetUnitIdByColliderGroupId(groupId, out var unitId))
                return;

            if (triggerInfo is FakeCharacterTriggerInfo fakeTriggerInfo)
            {
                Log.LogInfo($"Hit Unit Id:{unitId}, Damage:{fakeTriggerInfo.Attack}");
            }
        }
    }
}
