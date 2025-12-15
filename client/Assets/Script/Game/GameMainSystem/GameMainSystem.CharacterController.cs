using CameraModule;
using DataModule;
using GameMainModule.Attack;
using System.Collections.Generic;
using UnityEngine;

namespace GameMainModule
{
    public partial class GameMainSystem
    {
        [SerializeField]
        private GameThreeDimensionalCharacterController _characterController =
            new GameThreeDimensionalCharacterController();

        private CharacterAttackController _characterAttackController => _characterController.AttackController;

        public static GameThreeDimensionalCharacterController CharacterController
        {
            get
            {
                if (_instance == null)
                    return null;

                return _instance._characterController;
            }
        }

        public static void SetCombinationMaxCount(int count)
        {
            _instance._characterAttackController.SetCombinationMaxCount(count);
        }

        public static void SetCurCombination(int index)
        {
            _instance._characterAttackController.SetCurCombination(index);
        }

        public static void SetWeaponAttackBehaviorToController(List<WeaponBehaviorSetupData> weaponBehaviorSetupList)
        {
            if (weaponBehaviorSetupList == null)
                return;

            for (int i = 0; i < weaponBehaviorSetupList.Count; i++)
            {
                SetWeaponAttackBehaviorToController(i, weaponBehaviorSetupList[i]);
            }
        }

        public static void SetWeaponAttackBehaviorToController(int index, WeaponBehaviorSetupData weaponBehaviorSetup)
        {
            _instance._characterAttackController.SetCombination(index, new AttackCombinationRuntimeSetupData(weaponBehaviorSetup));
        }

        public static void CharacterTeleportTo(Vector3 worldPosition, Vector3 direction)
        {
            _instance._characterController.Teleport(worldPosition, direction);
            CameraSystem.CameraCommand(new ThirdPersonImmediateLooAtTarget(direction));
        }
    }
}
