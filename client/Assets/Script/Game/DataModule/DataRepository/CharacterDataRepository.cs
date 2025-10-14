using Extension;
using GameMainModule;
using System.Collections.Generic;
using UIModule;

namespace DataModule
{
    #region Runtime UIData

    public class UIWeaponBehaviorSetup : IUIData
    {
        public int WeaponRefItemId;
        public readonly List<int> AttackBehaviorRefItemIdList = new List<int>();

        public void SyncData(WeaponBehaviorSetup syncData)
        {
            WeaponRefItemId = syncData.WeaponRefItemId;
            AttackBehaviorRefItemIdList.Clear();
            AttackBehaviorRefItemIdList.AddRange(syncData.AttackBehaviorRefItemIdList);
        }
    }

    public class UICharacterData : IUIData
    {
        public readonly List<int> WeaponRefItemIdList = new List<int>();
        public readonly Dictionary<int, UIWeaponBehaviorSetup> WeaponBehaviorSetupDic =
            new Dictionary<int, UIWeaponBehaviorSetup>();

        public void SyncData(CharacterData syncData)
        {
            SyncWeaponData(syncData);
            SyncFullWeaponToBehaviorData(syncData);
        }

        public void SyncWeaponData(CharacterData syncData)
        {
            WeaponRefItemIdList.Clear();
            WeaponRefItemIdList.AddRange(syncData.WeaponRefItemIdList);
        }

        public void SyncFullWeaponToBehaviorData(CharacterData syncData)
        {
            WeaponBehaviorSetupDic.Clear();
            for (int i = 0; i < syncData.WeaponToBehaviorRelationList.Count; i++)
            {
                SyncWeaponToBehaviorData(syncData.WeaponToBehaviorRelationList[i]);
            }
        }

        public void SyncWeaponToBehaviorData(WeaponBehaviorSetup syncData)
        {
            if (!WeaponBehaviorSetupDic.TryGetValue(syncData.WeaponRefItemId, out var data))
            {
                data = new UIWeaponBehaviorSetup();
                WeaponBehaviorSetupDic.Add(syncData.WeaponRefItemId, data);
            }

            data.SyncData(syncData);
        }

        public void SyncRemoveWeaponToBehaviorData(int weaponRefItemId)
        {
            WeaponBehaviorSetupDic.Remove(weaponRefItemId);
        }

        public void GetWeaponBehaviorListByEquip(List<UIWeaponBehaviorSetup> result)
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
                    result.Add(new UIWeaponBehaviorSetup() { WeaponRefItemId = CommonDefine.EmptyWeaponId });
                }
            }
        }
    }

    #endregion

    public class WeaponBehaviorSetup
    {
        public int WeaponRefItemId;
        public List<int> AttackBehaviorRefItemIdList = new List<int>();

        public WeaponBehaviorSetup(int weaponRefItemId)
        {
            WeaponRefItemId = weaponRefItemId;
        }
    }

    public class CharacterData
    {
        public List<int> WeaponRefItemIdList = new List<int>();
        public List<WeaponBehaviorSetup> WeaponToBehaviorRelationList = new List<WeaponBehaviorSetup>();
    }

    [DataRepository(1)]
    public class CharacterDataRepository : DataRepository<CharacterData>
    {
        private UICharacterData _uiCharacterData = new UICharacterData();

        public CharacterDataRepository(DataManager dataManager, int version) : base(dataManager, version)
        {
            EnsureWeaponList();
            _uiCharacterData.SyncData(_data);
        }

        protected override void OnLoad(int currentVersion, int loadedVersion)
        {
            EnsureWeaponList();
            _uiCharacterData.SyncData(_data);
        }

        public UICharacterData GetUICharacterData()
        {
            return _uiCharacterData;
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
            _uiCharacterData.SyncWeaponData(_data);
            _uiCharacterData.Notify(default);
            return true;
        }

        public bool SetWeaponBehavior(int weaponRefItemId, int index, int attackBehaviorRefItemId)
        {
            if (index < 0)
                return false;

            if (!TryGetWeaponToBehaviorData(weaponRefItemId, out var data))
            {
                data = new WeaponBehaviorSetup(weaponRefItemId);
                _data.WeaponToBehaviorRelationList.Add(data);
            }

            EnsureWeaponBehaviorList(data.AttackBehaviorRefItemIdList, index + 1);
            data.AttackBehaviorRefItemIdList[index] = attackBehaviorRefItemId;
            _uiCharacterData.SyncWeaponToBehaviorData(data);
            _uiCharacterData.Notify(default);
            return true;
        }

        private bool TryGetWeaponToBehaviorData(int weaponRefItemId, out WeaponBehaviorSetup result)
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
