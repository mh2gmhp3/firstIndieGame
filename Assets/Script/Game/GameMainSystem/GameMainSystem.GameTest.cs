using AssetsModule;
using CameraModule;
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
            var textCharacterTrans = testCharacterGo.transform;
            CameraSystem.CameraCommand(
                new ThirdPersonModeCommandData
                {
                    TargetTrans = textCharacterTrans,
                    FocusTargetOffset = Vector3.zero,
                    Distance = 10,
                    CameraRotateSensitivity = 50,
                    ScreenAxisValue = Vector2.zero,
                });
        }
    }
}
