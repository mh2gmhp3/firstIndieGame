using AssetsModule;
using CameraModule;
using SceneModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;

namespace GameMainSystem
{
    public partial class GameMainSystem
    {
        private void InitGameTest()
        {
            var testCharacterAssets = AssetsSystem.LoadAssets<GameObject>("Prototype/TestObject/Character_Sphere");
            var testCharacterGo = ObjectUtility.InstantiateWithoutClone(testCharacterAssets);
            var testCharacterTrans = testCharacterGo.transform;
            _characterController.SetCharacterTransform(testCharacterTrans);
            _characterController.SetEnable(true);
            CameraSystem.CameraCommand(
                new ThirdPersonModeCommandData
                {
                    TargetTrans = testCharacterTrans,
                    FocusTargetOffset = new Vector3(0, 0.5f, 0),
                    Distance = 10,
                    CameraRotateSensitivity = 50,
                    ScreenAxisValue = Vector2.zero,
                });
            SceneSystem.SwitchStage(new SwitchStageData("world_map_test"));
        }
    }
}
