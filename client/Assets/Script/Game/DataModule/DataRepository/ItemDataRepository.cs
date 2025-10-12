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
    public class UIItemData : IUIData
    {
        private readonly ItemData _rawData;

        public int Id => _rawData.Id;
        public int SettingId => _rawData.SettingId;

        public int Count => _rawData.Count;

        public UIItemData(ItemData rawData)
        {
            _rawData = rawData;
        }
    }

    public class ItemData
    {
        public int Id;
        public int SettingId;

        public int Count;

        public ItemData(int id, int settingId, int count)
        {
            Id = id;
            SettingId = settingId;
            Count = count;
        }
    }

    public class ItemDataContainer
    {
        public int NextId;
        public List<ItemData> ItemDataList = new List<ItemData>();
    }

    [DataRepository(1)]
    public class ItemDataRepository : DataRepository<ItemDataContainer>
    {
        private Dictionary<int, ItemData> _idToDataDic = new Dictionary<int, ItemData>();

        //Runtime UIData
        private List<UIItemData> _uiDataList = new List<UIItemData>();
        private Dictionary<int, UIItemData> _idToUIDataDic = new Dictionary<int, UIItemData>();

        private List<ItemData> _cacheGetItemList = new List<ItemData>();

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

                    var uiItemData = new UIItemData(data);
                    _idToUIDataDic.Add(uiItemData.Id, uiItemData);
                    _uiDataList.Add(uiItemData);
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

        public void GetAllItemList(List<UIItemData> result)
        {
            if (result == null)
                return;

            result.AddRange(_uiDataList);
        }

        /// <summary>
        /// 新增道具
        /// </summary>
        /// <param name="settingId"></param>
        /// <param name="count"></param>
        /// <param name="notifyEvent">變動資料, 是否新增</param>
        public void AddItem(int settingId, int count, Action<UIItemData, bool> notifyEvent = null)
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
                    if (_idToUIDataDic.TryGetValue(oriItem.Id, out var uIItemData))
                    {
                        if (notifyEvent != null)
                            notifyEvent.Invoke(uIItemData, false);
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
        public void RemoveItem(int id, int count, Action<UIItemData, bool> notifyEvent = null)
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
            if (_idToUIDataDic.TryGetValue(id, out var uIItemData))
            {
                _uiDataList.Remove(uIItemData);
                _idToUIDataDic.Remove(id);

                if (notifyEvent != null)
                    notifyEvent.Invoke(uIItemData, needRemove);
            }
        }

        /// <summary>
        /// 新增道具至資料內
        /// </summary>
        /// <param name="settingId"></param>
        /// <param name="count"></param>
        /// <param name="notifyEvent">新增通知事件</param>
        private void AddNewItem(int settingId, int count, Action<UIItemData, bool> notifyEvent = null)
        {
            var nextId = GetNextId();
            if (_idToDataDic.ContainsKey(nextId))
            {
                Log.LogError($"ItemDataRepository AddNewItem Error, id is exist Id:{nextId} SettingId:{settingId}");
                return;
            }

            var newItemData = new ItemData(nextId, settingId, count);
            //Data
            _data.ItemDataList.Add(newItemData);
            //MappingData
            _idToDataDic.Add(newItemData.Id, newItemData);
            //Runtime
            var newUIItemData = new UIItemData(newItemData);
            _uiDataList.Add(newUIItemData);
            _idToUIDataDic.Add(newItemData.Id, newUIItemData);
            if (notifyEvent != null)
                notifyEvent.Invoke(newUIItemData, true);
        }

        /// <summary>
        /// 獲取此設定Id的道具列表 對應加入規則可能會有多筆
        /// </summary>
        /// <param name="settingId"></param>
        /// <param name="result"></param>
        private void GetItemListBySettingId(int settingId, List<ItemData> result)
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
