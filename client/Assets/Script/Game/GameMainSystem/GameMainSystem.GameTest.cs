using AssetModule;
using CameraModule;
using DataModule;
using GameMainModule.Animation;
using SceneModule;
using UIModule;
using UIModule.Game;
using UnitModule;
using UnitModule.Movement;
using UnityEngine;
using Utility;

namespace GameMainModule
{
    public partial class GameMainSystem
    {
        private void InitGameTest()
        {
            SetCurGameState(GameState.Normal);
            InitTestPlayerCharacterUnit();
            SceneSystem.SwitchStage(new SwitchStageData("world_map_test"));
        }

        /// <summary>
        /// 初始化測試用角色單位
        /// </summary>
        private void InitTestPlayerCharacterUnit()
        {
            //讀取Prefab
            var testCharacterAssets = AssetSystem.LoadAsset<GameObject>("Prototype/TestObject/Character_06/Character_Root");
            var testCharacterGo = ObjectUtility.InstantiateWithoutClone(testCharacterAssets);
            var testCharacterGameUnit = testCharacterGo.GetComponent<GameCharacterUnit>();
            var unitData = testCharacterGameUnit.UnitData;
            //玩家角色控制
            //角色移動設定
            var movementSetting = AssetSystem.LoadAsset<MovementSetting>("Setting/MovementSetting");
            var unitMovementSetting = unitData.MovementSetting;
            //角色動畫控制
            var characterAnimationSetting = AssetSystem.LoadAsset<CharacterAnimationSetting>("Setting/CharacterAnimationSetting/PrototypeCharacter");
            //武器位置參考
            var weaponTransformSetting = testCharacterGameUnit.WeaponTransform;
            //初始化角色控制器
            _characterController.InitController(
                unitMovementSetting,
                movementSetting,
                characterAnimationSetting,
                weaponTransformSetting);
            //第三人稱相機註冊
            CameraSystem.CameraCommand(
                new ThirdPersonModeCommandData
                {
                    TargetTrans = unitData.CameraLookupCenterTransform,
                    FocusTargetOffset = new Vector3(0, 0.5f, 0),
                    Distance = 5,
                    CameraRotateSensitivity = 100,
                    ScreenAxisValue = Vector2.zero,
                    CameraSpeed = 50,
                });
            //初始化攻擊行為
            SetCombinationMaxCount(CommonDefine.WeaponCount); // 先用Define 之後有變動數值在調整
            SetWeaponAttackBehaviorToController(GetWeaponBehaviorListByEquip());
            SetCurCombination(0);

            //測試註冊Collider
            _unitManager.AddUnit(_unitManager.AllocUnitId(), testCharacterGameUnit);

            var testNpcCharacterGo = ObjectUtility.InstantiateWithoutClone(testCharacterAssets);
            var testNpcCharacterGameUnit = testNpcCharacterGo.GetComponent<GameUnit>();
            testNpcCharacterGameUnit.transform.position = new Vector3(5, 0, 0);
            _unitManager.AddUnit(_unitManager.AllocUnitId(), testNpcCharacterGameUnit);

            UISystem.OpenUIWindow(WindowId.Window_Game, null);
        }

        private void CreateTestData(int id)
        {
            DataManager.CreateNew(id);
            AddAllItem();
            DataManager.Save(id);
        }
    }
}
