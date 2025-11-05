using Logging;
using UnitModule.Movement;
using UnityEngine;
using static UnitModule.UnitAvatarManager;

namespace UnitModule
{
    public class CharacterUnit : Unit
    {
        public override int UnitType => (int)UnitDefine.UnitType.Character;
        public override Vector3 Position
        {
            get => UnitMovementSetting.RootTransform.position;
        }

        private GameUnitMovementSetting _unitMovementSetting;
        private CharacterUnitAvatarSetting _unitAvatarSetting;

        public GameUnitMovementSetting UnitMovementSetting => _unitMovementSetting;
        public WeaponTransformSetting WeaponTransformSetting => _unitAvatarSetting.WeaponTransform;

        public void SetAvatarInsInfo(UnitAvatarInstance avatarInstance)
        {
            if (avatarInstance == null)
            {
                Log.LogError("CharacterUnit.SetAvatarInsInfo Error, avatarInstance is null");
                return;
            }
            if (!(avatarInstance.UnitAvatarSetting is CharacterUnitAvatarSetting characterUnitAvatarSetting))
            {
                Log.LogError("CharacterUnit.SetAvatarInsInfo Error, UnitAvatarSetting cant convert to CharacterUnitAvatarSetting");
                return;
            }

            _unitMovementSetting = new GameUnitMovementSetting(avatarInstance);
            _unitAvatarSetting = characterUnitAvatarSetting;
        }

        protected override void DoInit()
        {

        }

        protected override void DoReset()
        {
            _unitMovementSetting = null;
            _unitAvatarSetting = null;
        }
    }
}
