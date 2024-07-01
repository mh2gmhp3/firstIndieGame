using AssetsModule;
using CameraModule;
using GameMainSystem.Attack;
using GameMainSystem.Collision;
using Movement;
using SceneModule;
using System.Collections;
using System.Collections.Generic;
using UIModule;
using UnityEngine;
using Utility;

namespace GameMainSystem
{
    public partial class GameMainSystem
    {
        private void InitGameTest()
        {
            var testCharacterAssets = AssetsSystem.LoadAssets<GameObject>("Prototype/TestObject/Character_Root");
            var testCharacterGo = ObjectUtility.InstantiateWithoutClone(testCharacterAssets);
            var testCharacterTrans = testCharacterGo.transform;
            var animator = testCharacterTrans.GetChild(0).GetChild(0).GetComponent<Animator>();
            _characterController.SetCharacterRoot(testCharacterGo);
            _characterController.SetEnable(true);
            var gameAniController = new GameAnimationController();
            gameAniController.SetAnimatior(animator);
            _characterController.SetMovementAnimationController(gameAniController);
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

            //測試攻擊
            List<AttackCombination> attackCombinationList = new List<AttackCombination>();
            List<AttackBehavior> mainAttackBehaviorList = new List<AttackBehavior>();
            mainAttackBehaviorList.Add(new AttackBehavior("mainAttack 1", 0.1f, 0.5f));
            mainAttackBehaviorList.Add(new AttackBehavior("mainAttack 2", 0.2f, 0.5f));
            mainAttackBehaviorList.Add(new AttackBehavior("mainAttack 3", 0.2f, 0.5f));
            List<AttackBehavior> subAttackBehaviorList = new List<AttackBehavior>();
            subAttackBehaviorList.Add(new AttackBehavior("subAttack 1", 0.1f, 0.5f));
            subAttackBehaviorList.Add(new AttackBehavior("subAttack 2", 0.2f, 0.5f));
            subAttackBehaviorList.Add(new AttackBehavior("subAttack 3", 0.2f, 0.5f));
            attackCombinationList.Add(new AttackCombination(mainAttackBehaviorList, subAttackBehaviorList));
            _characterController.SetCombinationList(attackCombinationList);
            _characterController.SetNowCombination(0);

            //測試註冊Collider
            var characterCollider = testCharacterTrans.GetChild(0).GetComponent<Collider>();
            var testColliderGroup = new CollisionAreaManager.RegisterColliderData();
            testColliderGroup.IdToRegisterColliderDic.Add(1, characterCollider);
            int groupId = _collisionAreaManager.RegisterCollider(testColliderGroup);

            //測試發送碰撞區域請求 用Update TestMode 測試連送
            //CreateTestCollisionArea(10);

            UISystem.OpenUIWindows("UIWindowsTest", null);
        }
    }
}
