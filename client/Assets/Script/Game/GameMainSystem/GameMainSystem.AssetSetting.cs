using AssetModule;
using GameMainModule.Attack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameMainModule
{
    public partial class GameMainSystem
    {
        private AttackBehaviorAssetSetting _attackBehaviorAssetSetting;
        public static AttackBehaviorAssetSetting AttackBehaviorAssetSetting => _instance._attackBehaviorAssetSetting;

        public void InitAssetSetting()
        {
            _attackBehaviorAssetSetting = AssetSystem.LoadAsset<AttackBehaviorAssetSetting>("Setting/AttackBehaviorAssetSetting/PrototypeCharacter");
        }
    }
}
