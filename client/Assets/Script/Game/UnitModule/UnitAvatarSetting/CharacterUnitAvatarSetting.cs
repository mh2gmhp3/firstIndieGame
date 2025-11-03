using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnitModule
{
    [Serializable]
    public class WeaponTransformSetting
    {
        public Transform Left_HandTransform;
        public Transform Right_HandTransform;

        public List<Transform> StorageTransformList;
    }

    public class CharacterUnitAvatarSetting : UnitAvatarSetting
    {
        [SerializeField]
        private WeaponTransformSetting _weaponTransform;

        public WeaponTransformSetting WeaponTransform => _weaponTransform;
    }
}
