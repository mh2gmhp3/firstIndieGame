using FormModule;
using Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DataModule
{
    public class WeaponData
    {
        public int RefItemId;
        public int SettingId;

        public List<int> RefBehaviorId = new List<int>();

        public WeaponData(int itemId, int settingId)
        {
            RefItemId = itemId;
            SettingId = settingId;
        }
    }

    public class WeaponDataContainer
    {
        public List<WeaponData> WeaponDataList = new List<WeaponData>();
    }

    [DataRepository(1)]
    public class WeaponDataRepository : DataRepository<WeaponDataContainer>
    {
        private Dictionary<int, WeaponData> _idToDataDic = new Dictionary<int, WeaponData>();

        public WeaponDataRepository(DataManager dataManager, int version) : base(dataManager, version)
        {
        }

        protected override void OnLoad(int currentVersion, int loadedVersion)
        {
            for (int i = 0; i < _data.WeaponDataList.Count; i++)
            {
                var data = _data.WeaponDataList[i];
                //整理Mapping
                if (_idToDataDic.ContainsKey(data.RefItemId))
                {
                    Log.LogError($"WeaponDataRepository OnLoad Error, 有相同Id資料! Data:Id{data.RefItemId} SettingId:{data.SettingId}");
                }
                else
                {
                    _idToDataDic.Add(data.RefItemId, data);
                }
            }
        }

        public void AddWeapon(int refItemId, int settingId)
        {
            if (_idToDataDic.ContainsKey(refItemId))
            {
                Log.LogError($"WeaponDataRepository AddWeapon Error, RefItemId already exist, " +
                    $"RefItemId:{refItemId}, SettingId:{settingId}");
                return;
            }

            if (!FormSystem.Table.WeaponTable.TryGetData(settingId, out var weaponRow))
            {
                Log.LogError($"WeaponDataRepository AddWeapon Error, SettingRow not found, " +
                    $"RefItemId:{refItemId}, SettingId:{settingId}");
                return;
            }

            var newWeapon = new WeaponData(refItemId, settingId);
            _idToDataDic.Add(refItemId, newWeapon);
            _data.WeaponDataList.Add(newWeapon);
        }

        public void RemoveWeapon(int refItemId)
        {
            if (!_idToDataDic.TryGetValue(refItemId, out var weaponData))
            {
                Log.LogError($"WeaponDataRepository RemoveWeapon Error, RefItemId not exist, " +
                    $"RefItemId:{refItemId}");
                return;
            }

            _idToDataDic.Remove(refItemId);
            _data.WeaponDataList.Remove(weaponData);
        }
    }
}
