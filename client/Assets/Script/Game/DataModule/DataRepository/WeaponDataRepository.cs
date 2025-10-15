using FormModule;
using Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

namespace DataModule
{
    #region Runtime

    public class WeaponData
    {
        public int RefItemId;
        public int SettingId;

        public WeaponData(WeaponRepoData syncData)
        {
            SyncData(syncData);
        }

        public void SyncData(WeaponRepoData syncData)
        {
            RefItemId = syncData.RefItemId;
            SettingId = syncData.SettingId;
        }
    }

    #endregion

    public class WeaponRepoData
    {
        public int RefItemId;
        public int SettingId;

        public WeaponRepoData(int itemId, int settingId)
        {
            RefItemId = itemId;
            SettingId = settingId;
        }
    }

    public class WeaponRepoDataContainer
    {
        public List<WeaponRepoData> WeaponDataList = new List<WeaponRepoData>();
    }

    [DataRepository(1)]
    public class WeaponDataRepository : DataRepository<WeaponRepoDataContainer>
    {
        private Dictionary<int, WeaponRepoData> _idToDataDic = new Dictionary<int, WeaponRepoData>();

        //Runtime
        private List<WeaponData> _runtimeDataList = new List<WeaponData>();
        private Dictionary<int, WeaponData> _idToRuntimeDataDic = new Dictionary<int, WeaponData>();

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

                    //Runtime
                    var runtimeData = new WeaponData(data);
                    _runtimeDataList.Add(runtimeData);
                    _idToRuntimeDataDic.Add(data.RefItemId, runtimeData);
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

            var newWeapon = new WeaponRepoData(refItemId, settingId);
            _idToDataDic.Add(refItemId, newWeapon);
            _data.WeaponDataList.Add(newWeapon);
            //Runtime
            var runtimeData = new WeaponData(newWeapon);
            _runtimeDataList.Add(runtimeData);
            _idToRuntimeDataDic.Add(runtimeData.RefItemId, runtimeData);
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
            //Runtime
            if (_idToRuntimeDataDic.TryGetValue(refItemId, out var runtimeData))
            {
                _runtimeDataList.Remove(runtimeData);
                _idToRuntimeDataDic.Remove(refItemId);
            }
        }

        public bool TryGetWeaponData(int refItemId, out WeaponData result)
        {
            return _idToRuntimeDataDic.TryGetValue(refItemId, out result);
        }
    }
}
