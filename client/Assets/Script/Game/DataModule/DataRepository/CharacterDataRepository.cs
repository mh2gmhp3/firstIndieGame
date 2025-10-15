using Extension;
using GameMainModule;
using System.Collections.Generic;
using UIModule;

namespace DataModule
{
    #region Runtime UIData

    public class WeaponBehaviorSetupData : IUIData
    {
        public int WeaponRefItemId;
        public readonly List<int> AttackBehaviorRefItemIdList = new List<int>();

        public void SyncData(WeaponBehaviorSetupRepoData syncData)
        {
            WeaponRefItemId = syncData.WeaponRefItemId;
            AttackBehaviorRefItemIdList.Clear();
            AttackBehaviorRefItemIdList.AddRange(syncData.AttackBehaviorRefItemIdList);
        }
    }

    public class CharacterData : IUIData
    {
        public readonly List<int> WeaponRefItemIdList = new List<int>();
        public readonly Dictionary<int, WeaponBehaviorSetupData> WeaponBehaviorSetupDic =
            new Dictionary<int, WeaponBehaviorSetupData>();

        public void SyncData(CharacterRepoData syncData)
        {
            SyncWeaponData(syncData);
            SyncFullWeaponToBehaviorData(syncData);
        }

        public void SyncWeaponData(CharacterRepoData syncData)
        {
            WeaponRefItemIdList.Clear();
            WeaponRefItemIdList.AddRange(syncData.WeaponRefItemIdList);
        }

        public void SyncFullWeaponToBehaviorData(CharacterRepoData syncData)
        {
            WeaponBehaviorSetupDic.Clear();
            for (int i = 0; i < syncData.WeaponToBehaviorRelationList.Count; i++)
            {
                SyncWeaponToBehaviorData(syncData.WeaponToBehaviorRelationList[i]);
            }
        }

        public void SyncWeaponToBehaviorData(WeaponBehaviorSetupRepoData syncData)
        {
            if (!WeaponBehaviorSetupDic.TryGetValue(syncData.WeaponRefItemId, out var data))
            {
                data = new WeaponBehaviorSetupData();
                WeaponBehaviorSetupDic.Add(syncData.WeaponRefItemId, data);
            }

            data.SyncData(syncData);
        }

        public void SyncRemoveWeaponToBehaviorData(int weaponRefItemId)
        {
            WeaponBehaviorSetupDic.Remove(weaponRefItemId);
        }

        public void GetWeaponBehaviorListByEquip(List<WeaponBehaviorSetupData> result)
        {
            if (result == null)
                return;
            result.Clear();
            for (int i = 0; i < WeaponRefItemIdList.Count; i++)
            {
                var weaponRefItemId = WeaponRefItemIdList[i];
                if (WeaponBehaviorSetupDic.TryGetValue(weaponRefItemId, out var behaviorSetup))
                {
                    result.Add(behaviorSetup);
                }
                else
                {
                    result.Add(new WeaponBehaviorSetupData() { WeaponRefItemId = CommonDefine.EmptyWeaponId });
                }
            }
        }
    }

    #endregion

    public class WeaponBehaviorSetupRepoData
    {
        public int WeaponRefItemId;
        public List<int> AttackBehaviorRefItemIdList = new List<int>();

        public WeaponBehaviorSetupRepoData(int weaponRefItemId)
        {
            WeaponRefItemId = weaponRefItemId;
        }
    }

    public class CharacterRepoData
    {
        public List<int> WeaponRefItemIdList = new List<int>();
        public List<WeaponBehaviorSetupRepoData> WeaponToBehaviorRelationList = new List<WeaponBehaviorSetupRepoData>();
    }

    [DataRepository(1)]
    public class CharacterDataRepository : DataRepository<CharacterRepoData>
    {
        private CharacterData _runtimeData = new CharacterData();

        public CharacterDataRepository(DataManager dataManager, int version) : base(dataManager, version)
        {
            EnsureWeaponList();
            _runtimeData.SyncData(_data);
        }

        protected override void OnLoad(int currentVersion, int loadedVersion)
        {
            EnsureWeaponList();
            _runtimeData.SyncData(_data);
        }

        public CharacterData GetCharacterData()
        {
            return _runtimeData;
        }

        private void EnsureWeaponList()
        {
            //填充空格
            _data.WeaponRefItemIdList.EnsureCount(CommonDefine.WeaponCount, () => { return CommonDefine.EmptyWeaponId; });
        }

        private void EnsureWeaponBehaviorList(List<int> weaponBehaviorList, int count)
        {
            weaponBehaviorList.EnsureCount(count, () => { return CommonDefine.EmptyAttackBehaviorId; });
        }

        /// <summary>
        /// 設定武器
        /// </summary>
        /// <param name="index"></param>
        /// <param name="weaponRefItemId"></param>
        public bool SetWeapon(int index, int weaponRefItemId)
        {
            EnsureWeaponList();
            if (index < 0 || index >= _data.WeaponRefItemIdList.Count)
                return false;

            if (weaponRefItemId == CommonDefine.EmptyWeaponId)
            {
                //清空
                _data.WeaponRefItemIdList[index] = CommonDefine.EmptyWeaponId;
            }
            else
            {
                //檢查是否有裝在其他的位置上
                var oriIndex = _data.WeaponRefItemIdList.IndexOf(weaponRefItemId);
                if (oriIndex >= 0)
                {
                    //相同位置 不替換
                    if (oriIndex == index)
                        return false;

                    //將目標索引的交換過去
                    var targetRefItemId = _data.WeaponRefItemIdList[index];
                    _data.WeaponRefItemIdList[oriIndex] = targetRefItemId;
                }

                _data.WeaponRefItemIdList[index] = weaponRefItemId;
            }
            _runtimeData.SyncWeaponData(_data);
            _runtimeData.Notify(default);
            return true;
        }

        public bool SetWeaponBehavior(int weaponRefItemId, int index, int attackBehaviorRefItemId)
        {
            if (index < 0)
                return false;

            if (!TryGetWeaponToBehaviorData(weaponRefItemId, out var data))
            {
                data = new WeaponBehaviorSetupRepoData(weaponRefItemId);
                _data.WeaponToBehaviorRelationList.Add(data);
            }

            EnsureWeaponBehaviorList(data.AttackBehaviorRefItemIdList, index + 1);
            data.AttackBehaviorRefItemIdList[index] = attackBehaviorRefItemId;
            _runtimeData.SyncWeaponToBehaviorData(data);
            _runtimeData.Notify(default);
            return true;
        }

        private bool TryGetWeaponToBehaviorData(int weaponRefItemId, out WeaponBehaviorSetupRepoData result)
        {
            for (int i = 0; i < _data.WeaponToBehaviorRelationList.Count; i++)
            {
                if (_data.WeaponToBehaviorRelationList[i].WeaponRefItemId == weaponRefItemId)
                {
                    result = _data.WeaponToBehaviorRelationList[i];
                    return true;
                }
            }

            result = null;
            return false;
        }
    }
}
