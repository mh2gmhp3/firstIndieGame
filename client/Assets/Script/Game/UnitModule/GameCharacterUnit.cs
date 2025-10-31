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

    /// <summary>
    /// 遊戲角色單位
    /// </summary>
    public class GameCharacterUnit : GameUnit
    {
        [SerializeField]
        private WeaponTransformSetting _weaponTransform;

        public WeaponTransformSetting WeaponTransform => _weaponTransform;
    }
}
