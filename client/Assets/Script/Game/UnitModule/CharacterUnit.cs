using System.Collections;
using System.Collections.Generic;
using UnitModule.Movement;
using UnityEngine;

namespace UnitModule
{
    public class CharacterUnit : Unit
    {
        private CharacterUnitAvatarSetting _characterUnitAvatarSetting;

        public WeaponTransformSetting WeaponTransformSetting => _characterUnitAvatarSetting.WeaponTransform;

        protected override void DoSetup()
        {
            _characterUnitAvatarSetting = UnitAvatarSetting as CharacterUnitAvatarSetting;
        }

        protected override void DoReset()
        {
            //do nothing
        }

        protected override void DoClear()
        {
            _characterUnitAvatarSetting = null;
        }
    }
}
