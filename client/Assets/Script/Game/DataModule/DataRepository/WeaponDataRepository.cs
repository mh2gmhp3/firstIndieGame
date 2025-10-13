using FormModule;
using Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

namespace DataModule
{
    #region Runtime UIData

    public class UIWeaponData
    {
        public int RefItemId;
        public int SettingId;

        public UIWeaponData(WeaponData syncData)
        {
            SyncData(syncData);
        }

        public void SyncData(WeaponData syncData)
        {
            RefItemId = syncData.RefItemId;
            SettingId = syncData.SettingId;
        }
    }

    #endregion

    public class WeaponData
    {
        public int RefItemId;
        public int SettingId;

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

        //Runtime UIData
        private List<UIWeaponData> _uiDataList = new List<UIWeaponData>();
        private Dictionary<int, UIWeaponData> _idToUIDataDic = new Dictionary<int, UIWeaponData>();

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
                    var uiData = new UIWeaponData(data);
                    _uiDataList.Add(uiData);
                    _idToUIDataDic.Add(data.RefItemId, uiData);
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
            //Runtime
            var uiData = new UIWeaponData(newWeapon);
            _uiDataList.Add(uiData);
            _idToUIDataDic.Add(uiData.RefItemId, uiData);
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
            if (_idToUIDataDic.TryGetValue(refItemId, out var uiData))
            {
                _uiDataList.Remove(uiData);
                _idToUIDataDic.Remove(refItemId);
            }
        }

        public bool TryGetUIWeaponData(int refItemId, out UIWeaponData result)
        {
            return _idToUIDataDic.TryGetValue(refItemId, out result);
        }
    }
}
