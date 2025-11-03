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
            var prototypeCharacterAvatarName = "Character_06";
            if (!AddCharacterUnit(prototypeCharacterAvatarName, out var characterUnit))
                return;
            //玩家角色控制
            //角色移動設定
            var movementSetting = AssetSystem.LoadAsset<MovementSetting>("Setting/MovementSetting");
            var unitMovementSetting = characterUnit.UnitMovementSetting;
            //角色動畫控制
            var characterAnimationSetting = AssetSystem.LoadAsset<CharacterAnimationSetting>("Setting/CharacterAnimationSetting/PrototypeCharacter");
            //武器位置參考
            var weaponTransformSetting = characterUnit.WeaponTransformSetting;
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
                    TargetTrans = unitMovementSetting.RootTransform,
                    FocusTargetOffset = new Vector3(0, 2.0f, 0),
                    Distance = 5,
                    CameraRotateSensitivity = 100,
                    ScreenAxisValue = Vector2.zero,
                    CameraSpeed = 50,
                });
            //初始化攻擊行為
            SetCombinationMaxCount(CommonDefine.WeaponCount); // 先用Define 之後有變動數值在調整
            SetWeaponAttackBehaviorToController(GetWeaponBehaviorListByEquip());
            SetCurCombination(0);

            AddNpcUnit(prototypeCharacterAvatarName, out var npcUnit);
            if (_unitAvatarManager.TryGetAvatarIns(npcUnit.Id, out var npcAvatarIns))
                npcAvatarIns.UnitSetting.RootTransform.position = new Vector3(5.0f, 0, 0);

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
