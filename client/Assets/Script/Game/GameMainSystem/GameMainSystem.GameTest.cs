using AssetModule;
using CameraModule;
using CollisionModule;
using DataModule;
using GameMainModule.Animation;
using GameMainModule.Attack;
using SceneModule;
using System.Collections.Generic;
using UIModule;
using UIModule.Game;
using UnitModule;
using UnitModule.Movement;
using Unity.VisualScripting;
using UnityEngine;
using Utility;

namespace GameMainModule
{
    public partial class GameMainSystem
    {
        private void InitGameTest()
        {
            SetCurGameState(GameState.Normal);
            EnterTerrain("256_128_256_4");
            InitTestPlayerCharacterUnit();
            if (_terrainManager.TryGetCurTerrainAreaWorldPoint(1, 1, out var worldPoint, out _))
                CharacterTeleportTo(worldPoint);
            else
                CharacterTeleportTo(new Vector3(2, 4, 2));
            //SceneSystem.SwitchStage(new SwitchStageData("world_map_test"));
        }

        /// <summary>
        /// 初始化測試用角色單位
        /// </summary>
        private void InitTestPlayerCharacterUnit()
        {
            var prototypeCharacterAvatarName = "Character_06";
            if (!AddCharacterUnit(prototypeCharacterAvatarName, out var characterUnit))
                return;
            //玩家角色控制
            //角色移動設定
            var unitMovementSetting = characterUnit.UnitMovementSetting;
            //角色動畫控制
            var characterAnimationSetting = AssetSystem.LoadAsset<CharacterAnimationSetting>("Setting/CharacterAnimationSetting/PrototypeCharacter");
            //初始化角色控制器
            RegisterUpdateTarget(_characterController);
            _characterController.InitController(
                characterUnit,
                _movementSetting,
                characterAnimationSetting);
            //第三人稱相機註冊
            CameraSystem.CameraCommand(
                new ThirdPersonModeCommandData(
                    unitMovementSetting.RootTransform,
                    Vector2.zero,
                    _normalThirdPersonSettingData));
            //初始化攻擊行為
            SetCombinationMaxCount(CommonDefine.WeaponCount); // 先用Define 之後有變動數值在調整
            SetWeaponAttackBehaviorToController(GetWeaponBehaviorListByEquip());
            SetCurCombination(0);

            AddNpcUnit(prototypeCharacterAvatarName, out var npcUnit);
            if (_unitAvatarManager.TryGetAvatarIns(npcUnit.Id, out var npcAvatarIns))
                npcAvatarIns.UnitSetting.RootTransform.position = new Vector3(5.0f, 0, 0);

            //Test Enemy
            if (_terrainManager.TryGetCurTerrainAreaWorldPoint(2, 1, out var worldPoint, out var radius))
            {
                AddEnemySpawnPoint(
                    worldPoint,
                    radius,
                    new List<TestEnemySpawnData>()
                    {
                    new TestEnemySpawnData()
                    {
                        SettingId = 0,
                        Hp = 60,
                        ModelName = prototypeCharacterAvatarName
                    }
                    });
            }

            UISystem.OpenUIWindow(WindowId.Window_Game, null);
        }

        private void CreateTestData(int id)
        {
            DataManager.CreateNew(id);
            AddAllItem();
            DataManager.Save(id);
        }
        public static ICollisionAreaSetupData GetCollisionAreaSetupData(
            Vector3 worldPoint,
            Vector3 direction,
            AttackCollisionRuntimeTrack collision,
            ICollisionAreaTriggerInfo triggerInfo,
            float speedRate)
        {
            switch (collision.CollisionAreaType)
            {
                case CollisionAreaDefine.AreaType.Test:
                    return new TestCollisionAreaSetupData(1); ;
                case CollisionAreaDefine.AreaType.Quad:
                    return new QuadCollisionAreaSetupData(
                        worldPoint,
                        direction,
                        collision.Duration(speedRate),
                        5f,
                        5f,
                        triggerInfo);
            }

            return new TestCollisionAreaSetupData(1);
        }

    }
}
