using AssetModule;
using GameMainModule.Attack;
using System.Collections;
using System.Collections.Generic;
using UnitModule.Movement;
using UnityEngine;

namespace GameMainModule
{
    public partial class GameMainSystem
    {
        private AttackBehaviorAssetSetting _attackBehaviorAssetSetting;
        public static AttackBehaviorAssetSetting AttackBehaviorAssetSetting => _instance._attackBehaviorAssetSetting;

        private MovementSetting _movementSetting;
        public static MovementSetting MovementSetting => _instance._movementSetting;

        public void InitAssetSetting()
        {
            _attackBehaviorAssetSetting = AssetSystem.LoadAsset<AttackBehaviorAssetSetting>("Setting/AttackBehaviorAssetSetting/PrototypeCharacter");
            _movementSetting = AssetSystem.LoadAsset<MovementSetting>("Setting/MovementSetting");
        }
    }
}
