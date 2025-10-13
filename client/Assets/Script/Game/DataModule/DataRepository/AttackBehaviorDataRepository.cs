using FormModule;
using Logging;
using System.Collections;
using System.Collections.Generic;
using UIModule;
using UnityEngine;

namespace DataModule
{
    #region Runtime UIData

    public class UIAttackBehaviorData : IUIData
    {
        public int RefItemId;
        public int SettingId;

        public UIAttackBehaviorData(AttackBehaviorData data)
        {
            SyncData(data);
        }

        public void SyncData(AttackBehaviorData data)
        {
            RefItemId = data.RefItemId;
            SettingId = data.SettingId;
        }
    }

    #endregion

    public class AttackBehaviorData
    {
        public int RefItemId;
        public int SettingId;

        public AttackBehaviorData(int refItemId, int settingId)
        {
            RefItemId = refItemId;
            SettingId = settingId;
        }
    }

    public class AttackBehaviorDataContainer
    {
        public List<AttackBehaviorData> AttackBehaviorDataList = new List<AttackBehaviorData>();
    }

    [DataRepository(1)]
    public class AttackBehaviorDataRepository : DataRepository<AttackBehaviorDataContainer>
    {
        private Dictionary<int, AttackBehaviorData> _idToDataDic = new Dictionary<int, AttackBehaviorData>();

        //Runtime UIData
        private List<UIAttackBehaviorData> _uiDataList = new List<UIAttackBehaviorData>();
        private Dictionary<int, UIAttackBehaviorData> _idToUIDataDic = new Dictionary<int, UIAttackBehaviorData>();

        public AttackBehaviorDataRepository(DataManager dataManager, int version) : base(dataManager, version)
        {
        }

        protected override void OnLoad(int currentVersion, int loadedVersion)
        {
            for (int i = 0; i < _data.AttackBehaviorDataList.Count; i++)
            {
                var data = _data.AttackBehaviorDataList[i];
                //整理Mapping
                if (_idToDataDic.ContainsKey(data.RefItemId))
                {
                    Log.LogError($"AttackBehaviorDataRepository OnLoad Error, 有相同Id資料! RefItemId:Id{data.RefItemId} SettingId:{data.SettingId}");
                }
                else
                {
                    _idToDataDic.Add(data.RefItemId, data);
                    //Runtime
                    var uiData = new UIAttackBehaviorData(data);
                    _uiDataList.Add(uiData);
                    _idToUIDataDic.Add(data.RefItemId, uiData);
                }
            }
        }

        /// <summary>
        /// 新增攻擊行為
        /// </summary>
        /// <param name="refItemId"></param>
        /// <param name="settingId"></param>
        public void AddBehavior(int refItemId, int settingId)
        {
            if (_idToDataDic.ContainsKey(refItemId))
            {
                Log.LogError($"AttackBehaviorDataRepository AddBehavior Error, RefItemId already exist, " +
                    $"RefItemId:{refItemId}, SettingId:{settingId}");
                return;
            }

            if (!FormSystem.Table.AttackBehaviorSettingTable.TryGetData(settingId, out _))
            {
                Log.LogError($"AttackBehaviorDataRepository AddBehavior Error, 找不到此設定資料 SettingId:{settingId}");
                return;
            }

            var data = new AttackBehaviorData(refItemId, settingId);
            _data.AttackBehaviorDataList.Add(data);
            _idToDataDic.Add(refItemId, data);
            //Runtime
            var uiData = new UIAttackBehaviorData(data);
            _uiDataList.Add(uiData);
            _idToUIDataDic.Add(uiData.RefItemId, uiData);
        }

        /// <summary>
        /// 移除攻擊行為
        /// </summary>
        /// <param name="refItemId"></param>
        public void RemoveBehavior(int refItemId)
        {
            if (!_idToDataDic.TryGetValue(refItemId, out var data))
            {
                Log.LogError($"AttackBehaviorDataRepository RemoveBehavior Error, RefItemId not exist, " +
                    $"RefItemId:{refItemId}");
                return;
            }

            _idToDataDic.Remove(refItemId);
            _data.AttackBehaviorDataList.Remove(data);
            //Runtime
            if (_idToUIDataDic.TryGetValue(refItemId, out var uiData))
            {
                _uiDataList.Remove(uiData);
                _idToUIDataDic.Remove(refItemId);
            }
        }

        public List<UIAttackBehaviorData> GetUIAttackBehaviorDataList() { return _uiDataList; }

        public bool TryGetUIAttackBehaviorData(int behaviorRefItemId, out UIAttackBehaviorData result)
        {
            return _idToUIDataDic.TryGetValue(behaviorRefItemId, out result);
        }
    }
}
