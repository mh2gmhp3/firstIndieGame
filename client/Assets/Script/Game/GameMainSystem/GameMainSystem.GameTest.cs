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
using DataModule;
using FormModule;

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
            var attackBehaviorAssetSetting = AssetSystem.LoadAsset<AttackBehaviorAssetSetting>("Setting/AttackBehaviorAssetSetting/PrototypeCharacter");
            _characterController.InitController(unitMovementSetting, movementSetting, animatorTransitionSetting, attackBehaviorAssetSetting);
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
            if (attackBehaviorAssetSetting.TryGetGroupData(100, out var groupData))
            {
                for (int i = 0; i < groupData.DataList.Count; i++)
                {
                    var data = groupData.DataList[i];
                    mainAttackBehaviorList.Add(new AttackBehavior(unitMovementSetting, data));
                }
            }
            List<AttackBehavior> subAttackBehaviorList = new List<AttackBehavior>();
            if (attackBehaviorAssetSetting.TryGetGroupData(100, out var groupDataSub))
            {
                for (int i = 0; i < groupData.DataList.Count; i++)
                {
                    var data = groupData.DataList[i];
                    subAttackBehaviorList.Add(new AttackBehavior(unitMovementSetting, data));
                }
            }
            attackCombinationList.Add(new AttackCombination(100, mainAttackBehaviorList, subAttackBehaviorList));
            _characterController.SetCombinationList(attackCombinationList);
            _characterController.SetNowCombination(0);

            //測試註冊Collider
            _unitManager.AddUnit(_unitManager.AllocUnitId(), testCharacterGameUnit);

            var testNpcCharacterGo = ObjectUtility.InstantiateWithoutClone(testCharacterAssets);
            var testNpcCharacterGameUnit = testNpcCharacterGo.GetComponent<GameUnit>();
            testNpcCharacterGameUnit.transform.position = new Vector3(5, 0, 0);
            _unitManager.AddUnit(_unitManager.AllocUnitId(), testNpcCharacterGameUnit);

            UISystem.OpenUIWindow("Window_Game", null);
        }

        private void CreateTestData(int id)
        {
            DataManager.CreateNew(id);
            AddAllAttackBehavior();
            AddAllItem();
            DataManager.Save(id);
        }
    }
}
