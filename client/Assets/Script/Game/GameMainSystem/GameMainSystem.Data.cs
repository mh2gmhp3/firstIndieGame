using DataModule;
using FormModule;
using Logging;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameMainModule
{
    public partial class GameMainSystem
    {
        private DataManager _dataManager;
        private static DataManager DataManager => _instance._dataManager;

        private void InitGameData()
        {
            _dataManager = new DataManager();
            _dataManager.Init();
            _dataManager.LoadGlobal();
        }

        //TODO 對於各Repository的操作應該可以建立Manager來管理不同Repo之間的資料處理關係 只用Region隔開可能不好處理

        #region Item

        /// <summary>
        /// 新增表上全部道具
        /// </summary>
        public static void AddAllItem()
        {
            var itemDataRepo = DataManager.GetDataRepository<ItemDataRepository>();
            var itemRowList = FormSystem.Table.ItemTable.GetDataList();
            for (int i = 0; i < itemRowList.Count; i++)
            {
                var count = 99999;
                var itemType = (TableDefine.ItemType)itemRowList[i].Type;
                switch (itemType)
                {
                    case TableDefine.ItemType.Weapon:
                        count = 5;
                        break;
                    case TableDefine.ItemType.AttackBehavior:
                        count = 5;
                        break;
                }
                itemDataRepo.AddItem(itemRowList[i].Id, count, OnAddItemNotify);
            }
        }

        /// <summary>
        /// 新增指定道具
        /// </summary>
        /// <param name="settingId"></param>
        /// <param name="count"></param>
        public static void AddItem(int settingId, int count)
        {
            var itemDataRepo = DataManager.GetDataRepository<ItemDataRepository>();
            itemDataRepo.AddItem(settingId, count, OnAddItemNotify);
        }

        /// <summary>
        /// 移除指定道具唯一Id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="count"></param>
        public static void RemoveItem(int id, int count)
        {
            var itemDataRepo = DataManager.GetDataRepository<ItemDataRepository>();
            itemDataRepo.RemoveItem(id, count, OnRemoveItemNotify);
        }

        private static void OnAddItemNotify(UIItemData itemData, bool isNew)
        {
            if (itemData == null)
                return;

            if (!FormSystem.Table.ItemTable.TryGetData(itemData.SettingId, out var itemRow))
            {
                Log.LogError($"GameMainSystem OnAddItemNotify Error, ItemRow can not found, " +
                    $"Id:{itemData.Id}, SettingId:{itemData.SettingId}");
                return;
            }

            var itemType = (TableDefine.ItemType)itemRow.Type;
            if (isNew)
            {
                switch (itemType)
                {
                    case TableDefine.ItemType.Weapon:
                        AddWeapon(itemData.Id, itemData.SettingId);
                        break;
                    case TableDefine.ItemType.AttackBehavior:
                        AddBehavior(itemData.Id, itemData.SettingId);
                        break;

                }
            }
        }

        private static void OnRemoveItemNotify(UIItemData itemData, bool isRemove)
        {
            if (itemData == null)
                return;

            if (!FormSystem.Table.ItemTable.TryGetData(itemData.SettingId, out var itemRow))
            {
                Log.LogError($"GameMainSystem OnRemoveItemNotify Error, ItemRow can not found, " +
                    $"Id:{itemData.Id}, SettingId:{itemData.SettingId}");
                return;
            }

            var itemType = (TableDefine.ItemType)itemRow.Type;
            switch (itemType)
            {
                case TableDefine.ItemType.Weapon:
                    if (isRemove)
                        RemoveWeapon(itemData.Id);
                    break;
            }
        }

        private static List<UIItemData> _cacheGetItemData = new List<UIItemData>();
        public static void GetItemDataList(List<UIItemData> result, TableDefine.ItemType type = TableDefine.ItemType.None)
        {
            if (result == null)
                return;

            var itemDataRepo = DataManager.GetDataRepository<ItemDataRepository>();
            itemDataRepo.GetAllItemList(_cacheGetItemData);

            // None回傳整筆
            if (type == TableDefine.ItemType.None)
            {
                result.AddRange(_cacheGetItemData);
                return;
            }

            var typeInt = (int)type;
            for (int i = 0; i < _cacheGetItemData.Count; i++)
            {
                var itemData = _cacheGetItemData[i];
                if (FormSystem.Table.ItemTable.TryGetData(itemData.SettingId, out var itemRow))
                {
                    if (itemRow.Type != typeInt)
                        continue;

                    result.Add(itemData);
                }
            }
        }

        #endregion

        #region Weapon

        /// <summary>
        /// 新增武器
        /// </summary>
        /// <param name="refItemId">道具參考唯一Id</param>
        /// <param name="settingId">道具設定Id等於武器設定Id</param>
        private static void AddWeapon(int refItemId, int settingId)
        {
            var weaponDataRepo = DataManager.GetDataRepository<WeaponDataRepository>();
            weaponDataRepo.AddWeapon(refItemId, settingId);
        }

        /// <summary>
        /// 移除武器
        /// </summary>
        /// <param name="refItemId">道具參考唯一Id</param>
        private static void RemoveWeapon(int refItemId)
        {
            var weaponDataRepo = DataManager.GetDataRepository<WeaponDataRepository>();
            weaponDataRepo.RemoveWeapon(refItemId);
        }

        #endregion

        #region AttackBehavior

        public static List<AttackBehaviorData> GetAttackBehaviorDataList()
        {
            var repo = DataManager.GetDataRepository<AttackBehaviorDataRepository>();
            if (repo == null)
                return null;

            return repo.GetAttackBehaviorDataList();
        }

        /// <summary>
        /// 新增攻擊行為
        /// </summary>
        /// <param name="refItemId">道具參考唯一Id</param>
        /// <param name="settingId">道具設定Id等於攻擊動作設定Id</param>
        private static void AddBehavior(int refItemId, int settingId)
        {
            var attackBehaviorDataRepo = DataManager.GetDataRepository<AttackBehaviorDataRepository>();
            attackBehaviorDataRepo.AddBehavior(refItemId, settingId);
        }

        /// <summary>
        /// 移除攻擊行為
        /// </summary>
        /// <param name="refItemId">道具參考唯一Id</param>
        private static void RemoveBehavior(int refItemId)
        {
            var attackBehaviorDataRepo = DataManager.GetDataRepository<AttackBehaviorDataRepository>();
            attackBehaviorDataRepo.RemoveBehavior(refItemId);
        }

        #endregion
    }
}
