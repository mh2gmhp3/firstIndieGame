using AssetModule;
using CameraModule;
using GameMainModule.Attack;
using CollisionModule;
using UnitModule.Movement;
using SceneModule;
using System.Collections;
using System.Collections.Generic;
using UIModule;
using UnityEngine;
using Utility;
using UnitModule;
using GameMainModule.Animation;

namespace GameMainModule
{
    public partial class GameMainSystem
    {
        private void InitGameTest()
        {
            InitTestPlayerCharacterUnit();
            SceneSystem.SwitchStage(new SwitchStageData("world_map_test"));
        }

        /// <summary>
        /// 初始化測試用角色單位
        /// </summary>
        private void InitTestPlayerCharacterUnit()
        {
            //讀取Prefab
            var testCharacterAssets = AssetSystem.LoadAsset<GameObject>("Prototype/TestObject/Character_02/Character_Root");
            var testCharacterGo = ObjectUtility.InstantiateWithoutClone(testCharacterAssets);
            var testCharacterGameUnit = testCharacterGo.GetComponent<GameUnit>();
            var unitData = testCharacterGameUnit.UnitData;
            //玩家角色控制
            //角色移動設定
            var movementSetting = AssetSystem.LoadAsset<MovementSetting>("Setting/MovementSetting");
            var unitMovementSetting = unitData.MovementSetting;
            //角色動畫控制
            var animatorTransitionSetting = AssetSystem.LoadAsset<AnimatorTransitionSetting>("Setting/AnimatorTransitionSetting/PrototypeCharacter");
            _characterController.InitController(unitMovementSetting, movementSetting, animatorTransitionSetting);
            //第三人稱相機註冊
            CameraSystem.CameraCommand(
                new ThirdPersonModeCommandData
                {
                    TargetTrans = unitData.CameraLookupCenterTransform,
                    FocusTargetOffset = new Vector3(0, 0.5f, 0),
                    Distance = 10,
                    CameraRotateSensitivity = 100,
                    ScreenAxisValue = Vector2.zero,
                    CameraSpeed = 50,
                });

            //測試攻擊
            List<AttackCombination> attackCombinationList = new List<AttackCombination>();
            List<AttackBehavior> mainAttackBehaviorList = new List<AttackBehavior>();
            mainAttackBehaviorList.Add(new AttackBehavior("Attack_01", 0.1f, 0.3f));
            mainAttackBehaviorList.Add(new AttackBehavior("Attack_02", 0.1f, 0.3f));
            mainAttackBehaviorList.Add(new AttackBehavior("Attack_03", 0.1f, 0.3f));
            List<AttackBehavior> subAttackBehaviorList = new List<AttackBehavior>();
            subAttackBehaviorList.Add(new AttackBehavior("subAttack 1", 0.1f, 0.5f));
            subAttackBehaviorList.Add(new AttackBehavior("subAttack 2", 0.2f, 0.5f));
            subAttackBehaviorList.Add(new AttackBehavior("subAttack 3", 0.2f, 0.5f));
            attackCombinationList.Add(new AttackCombination(mainAttackBehaviorList, subAttackBehaviorList));
            _characterController.SetCombinationList(attackCombinationList);
            _characterController.SetNowCombination(0);

            //測試註冊Collider
            int groupId = CollisionAreaManager.RegisterCollider(unitData.GetColliderData());

            var testNpcCharacterGo = ObjectUtility.InstantiateWithoutClone(testCharacterAssets);
            var testNpcCharacterGameUnit = testNpcCharacterGo.GetComponent<GameUnit>();
            testNpcCharacterGameUnit.transform.position = new Vector3(5, 0, 0);
            var unitNpcData = testNpcCharacterGameUnit.UnitData;
            CollisionAreaManager.RegisterCollider(unitNpcData.GetColliderData());
        }
    }
}
