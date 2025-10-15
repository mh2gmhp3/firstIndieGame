using FormModule;
using Logging;
using System;
using System.Collections.Generic;
using UIModule;

namespace DataModule
{
    /// <summary>
    /// 玩家道具資料在UI顯示時使用
    /// <para>如果有要單純顯示道具而不是玩家道具的需另外建立新類</para>
    /// </summary>
    public class ItemData : IUIData
    {
        public int Id;
        public int SettingId;

        public int Count;

        public ItemData(ItemRepoData rawData)
        {
            SyncData(rawData);
        }

        public void SyncData(ItemRepoData rawData)
        {
            Id = rawData.Id;
            SettingId = rawData.SettingId;
            Count = rawData.Count;
        }
    }

    public class ItemRepoData
    {
        public int Id;
        public int SettingId;

        public int Count;

        public ItemRepoData(int id, int settingId, int count)
        {
            Id = id;
            SettingId = settingId;
            Count = count;
        }
    }

    public class ItemRepoDataContainer
    {
        public int NextId;
        public List<ItemRepoData> ItemDataList = new List<ItemRepoData>();
    }

    [DataRepository(1)]
    public class ItemDataRepository : DataRepository<ItemRepoDataContainer>
    {
        private Dictionary<int, ItemRepoData> _idToDataDic = new Dictionary<int, ItemRepoData>();

        //Runtime
        private List<ItemData> _runtimeDataList = new List<ItemData>();
        private Dictionary<int, ItemData> _idToRuntimeDataDic = new Dictionary<int, ItemData>();

        private List<ItemRepoData> _cacheGetItemList = new List<ItemRepoData>();

        public ItemDataRepository(DataManager dataManager, int version) : base(dataManager, version)
        {
        }

        protected override void OnLoad(int currentVersion, int loadedVersion)
        {
            for (int i = 0; i < _data.ItemDataList.Count; i++)
            {
                var data = _data.ItemDataList[i];
                //整理Mapping
                if (_idToDataDic.ContainsKey(data.Id))
                {
                    Log.LogError($"ItemDataRepository OnLoad Error, 有相同Id資料! Data:Id{data.Id} SettingId:{data.SettingId}");
                }
                else
                {
                    _idToDataDic.Add(data.Id, data);

                    var runtimeItemData = new ItemData(data);
                    _idToRuntimeDataDic.Add(runtimeItemData.Id, runtimeItemData);
                    _runtimeDataList.Add(runtimeItemData);
                }

                //更新下一個Id 避免重複
                if (data.Id > _data.NextId)
                    _data.NextId = data.Id;
            }
        }

        private int GetNextId()
        {
            return ++_data.NextId;
        }

        public void GetAllItemList(List<ItemData> result)
        {
            if (result == null)
                return;

            result.AddRange(_runtimeDataList);
        }

        public bool TryGetItemData(int id, out ItemData itemData)
        {
            return _idToRuntimeDataDic.TryGetValue(id, out itemData);
        }

        /// <summary>
        /// 新增道具
        /// </summary>
        /// <param name="settingId"></param>
        /// <param name="count"></param>
        /// <param name="notifyEvent">變動資料, 是否新增</param>
        public void AddItem(int settingId, int count, Action<ItemData, bool> notifyEvent = null)
        {
            if (!FormSystem.Table.ItemTable.TryGetData(settingId, out var itemRow))
            {
                Log.LogError($"ItemDataRepository AddItem Error, 找不到此設定資料 SettingId:{settingId}");
                return;
            }

            if (count <= 0)
                return;

            //不可堆疊
            if (itemRow.Stack == 0)
            {
                // 每給一個就算一個新的單獨道具
                for (int i = 0; i < count; i++)
                {
                    AddNewItem(settingId, 1, notifyEvent);
                }
            }
            else
            {
                _cacheGetItemList.Clear();
                GetItemListBySettingId(settingId, _cacheGetItemList);
                // 只能依設定堆疊上限堆一堆 超出一律遺棄
                if (_cacheGetItemList.Count > 0)
                {
                    // 只會堆一堆 取第一筆
                    var oriItem = _cacheGetItemList[0];
                    var remainCount = itemRow.Stack - oriItem.Count;
                    oriItem.Count += Math.Min(remainCount, count);
                    if (_idToRuntimeDataDic.TryGetValue(oriItem.Id, out var runtimeItemData))
                    {
                        runtimeItemData.SyncData(oriItem);
                        runtimeItemData.Notify(default);
                        if (notifyEvent != null)
                            notifyEvent.Invoke(runtimeItemData, false);
                    }
                }
                else
                {
                    AddNewItem(settingId, Math.Min(count, itemRow.Stack), notifyEvent);
                }

                /* 超出單個堆疊後還能新增堆疊的作法 暫時用不到
                var remainAddCount = count;
                for (int i = 0; i < _cacheGetItemList.Count; i++)
                {
                    var item = _cacheGetItemList[i];
                    var remainCount = itemRow.Stack - item.Count;

                    // 沒有空間
                    if (remainCount == 0)
                        continue;

                    // 還有剩空間
                    if (remainAddCount >= remainCount)
                    {
                        // 能放得先放
                        item.Count += remainCount;
                        remainAddCount -= remainCount;
                    }
                    else
                    {
                        // 可以完整放入
                        item.Count += remainAddCount;
                        remainAddCount = 0;
                    }

                    if (remainAddCount <= 0)
                        break;
                }
                if (remainAddCount > 0)
                {
                    var stackItemCount = remainAddCount / itemRow.Stack;
                    for (int i = 0; i < stackItemCount;i++)
                    {
                        AddNewItem(settingId, itemRow.Stack);
                    }
                    var lastStackItem = remainAddCount % itemRow.Stack;
                    if (lastStackItem > 0)
                    {
                        AddNewItem(settingId, lastStackItem);
                    }
                }
                */
            }
        }

        /// <summary>
        /// 移除道具
        /// </summary>
        /// <param name="id"></param>
        /// <param name="count"></param>
        /// <param name="notifyEvent">變動資料, 是否移除</param>
        public void RemoveItem(int id, int count, Action<ItemData, bool> notifyEvent = null)
        {
            if (!_idToDataDic.TryGetValue(id, out var itemData))
            {
                Log.LogWarning($"ItemDataRepository RemoveItem Warning, 道具不存在 Id:{id}");
                return;
            }

            if (count <= 0)
                return;

            itemData.Count -= count;
            bool needRemove = itemData.Count <= 0;
            if (needRemove)
            {
                //Data
                _data.ItemDataList.Remove(itemData);
                //MappingData
                _idToDataDic.Remove(id);
            }

            //Runtime
            if (_idToRuntimeDataDic.TryGetValue(id, out var runtimeItemData))
            {
                if (needRemove)
                {
                    _runtimeDataList.Remove(runtimeItemData);
                    _idToRuntimeDataDic.Remove(id);
                }
                else
                {
                    runtimeItemData.SyncData(itemData);
                    runtimeItemData.Notify(default);
                }

                if (notifyEvent != null)
                    notifyEvent.Invoke(runtimeItemData, needRemove);
            }
        }

        /// <summary>
        /// 新增道具至資料內
        /// </summary>
        /// <param name="settingId"></param>
        /// <param name="count"></param>
        /// <param name="notifyEvent">新增通知事件</param>
        private void AddNewItem(int settingId, int count, Action<ItemData, bool> notifyEvent = null)
        {
            var nextId = GetNextId();
            if (_idToDataDic.ContainsKey(nextId))
            {
                Log.LogError($"ItemDataRepository AddNewItem Error, id is exist Id:{nextId} SettingId:{settingId}");
                return;
            }

            var newItemData = new ItemRepoData(nextId, settingId, count);
            //Data
            _data.ItemDataList.Add(newItemData);
            //MappingData
            _idToDataDic.Add(newItemData.Id, newItemData);
            //Runtime
            var runtimeItemData = new ItemData(newItemData);
            _runtimeDataList.Add(runtimeItemData);
            _idToRuntimeDataDic.Add(runtimeItemData.Id, runtimeItemData);
            if (notifyEvent != null)
                notifyEvent.Invoke(runtimeItemData, true);
        }

        /// <summary>
        /// 獲取此設定Id的道具列表 對應加入規則可能會有多筆
        /// </summary>
        /// <param name="settingId"></param>
        /// <param name="result"></param>
        private void GetItemListBySettingId(int settingId, List<ItemRepoData> result)
        {
            if (result == null)
                return;

            for (int i = 0; i < _data.ItemDataList.Count; i++)
            {
                if (_data.ItemDataList[i].SettingId == settingId)
                {
                    result.Add(_data.ItemDataList[i]);
                }
            }
        }
    }
}
