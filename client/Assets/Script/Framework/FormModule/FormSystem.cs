using AssetModule;
using GameSystem;
using Logging;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Utility;

namespace FormModule
{
    public interface ITableRow
    {
        int Id { get; }
    }

    public interface ITable
    {
        void DeserializeJson(string json);
    }

    public class TableAttribute : Attribute
    {
        public string SheetName;
        public TableAttribute(string name)
        {
            SheetName = name;
        }
    }

    /// <summary>
    /// 具有TableAttribute的會自動初始化並根據SheetName讀取資料
    /// <para>需要對初始化後的表格做處理可以在System內接收Init_FormTableDone</para>
    /// <para>TODO 讀取跟反序列化先用Queue，之後真的有一次讀整個卡住的再把他分次處理</para>
    /// </summary>
    [GameSystem(GameSystemPriority.FORM_SYSTEM)]
    public partial class FormSystem : BaseGameSystem<FormSystem>
    {
        private class TableInfo
        {
            public string TableTypeName;
            public string SheetName;
            public ITable Table;
            public TableInfo(string tableTypeName, string sheetName, ITable table)
            {
                TableTypeName = tableTypeName;
                SheetName = sheetName;
                Table = table;
            }
        }

        private enum InitState
        {
            None,
            CreateTableInstance,
            CreateTableInstanceDone,
            LoadTable,
            LoadTableDone,
            Deserialize,
            DeserializeDone,
            Done
        }

        private const string FOLDER_PATH = "Form";

        private Dictionary<string, TableInfo> _typeNameToTableInfoDic = new Dictionary<string, TableInfo>();

        #region Init

        private InitState _initState = InitState.None;
        //LoadTable
        /// <summary>
        /// 要讀取的表格Queue
        /// </summary>
        private Queue<TableInfo> _loadTableInfoQueue = new Queue<TableInfo>();
        /// <summary>
        /// 表格類型名稱, 讀取的TextAsset
        /// </summary>
        private Dictionary<string, TextAsset> _typeNameToTextAssetDic = new Dictionary<string, TextAsset>();
        //Deserialize
        /// <summary>
        /// 將要讀取的資料反序列化的Queue
        /// </summary>
        private Queue<TableInfo> _deserializeTableQueue = new Queue<TableInfo>();

        #endregion

        protected override bool DoEnterGameFlowProcessStep(int flowStep)
        {
            if (flowStep == (int)EnterGameFlowStepDefine.FrameworkEnterGameFlowStep.Init_FormTable)
            {
                switch (_initState)
                {
                    case InitState.None:
                    case InitState.CreateTableInstance:
                        CreateTableInstance();
                        break;
                    case InitState.CreateTableInstanceDone:
                    case InitState.LoadTable:
                        LoadTable();
                        break;
                    case InitState.LoadTableDone:
                    case InitState.Deserialize:
                        DeserializeTable();
                        break;
                    case InitState.DeserializeDone:
                        InitDone();
                        break;
                }
                return _initState == InitState.Done;
            }
            return true;
        }

        /// <summary>
        /// 獲取Table 如果是無效類型將會回傳null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static  T GetTable<T>() where T : class, ITable
        {
            if (_instance == null)
                return null;

            var typeName = typeof(T).Name.ToLower();
            if (!_instance._typeNameToTableInfoDic.TryGetValue(typeName, out var tableInfo) || tableInfo.Table == null)
            {
                Log.LogError($"FormSystem.GetTable table not found Type:{typeName}");
                return null;
            }

            return (T)tableInfo.Table;
        }

        #region Init

        private void CreateTableInstance()
        {
            _initState = InitState.CreateTableInstance;
            var typeAndAttributeList = AttributeUtility.GetAllAtttibuteTypeList<TableAttribute>();
            for (int i = 0; i < typeAndAttributeList.Count; i++)
            {
                var type = typeAndAttributeList[i].Type;
                var attribute = typeAndAttributeList[i].Attribute;

                var typeName = type.Name.ToLower();
                if (_typeNameToTableInfoDic.ContainsKey(typeName))
                {
                    Log.LogError($"FormSystem.InitTable: have duplicate Table name:{typeName}");
                    continue;
                }

                var table = Activator.CreateInstance(type) as ITable;
                var tableInfo = new TableInfo(typeName, attribute.SheetName, table);
                _typeNameToTableInfoDic.Add(typeName, tableInfo);
            }
            _initState = InitState.CreateTableInstanceDone;
        }

        private void LoadTable()
        {
            _initState = InitState.LoadTable;
            if (_typeNameToTableInfoDic.Count == 0)
            {
                _initState = InitState.LoadTableDone;
                return;
            }

            //加入要讀取的
            if (_loadTableInfoQueue.Count == 0)
            {
                foreach (var tableInfo in _typeNameToTableInfoDic.Values)
                {
                    _loadTableInfoQueue.Enqueue(tableInfo);
                }
            }

            //讀取
            while (_loadTableInfoQueue.Count > 0)
            {
                var tableInfo = _loadTableInfoQueue.Dequeue();
                var path = Path.Combine(FOLDER_PATH, tableInfo.SheetName);
                var loadedTextAsset = AssetSystem.LoadAsset<TextAsset>(path);
                if (loadedTextAsset == null)
                    continue;

                _typeNameToTextAssetDic.Add(tableInfo.TableTypeName, loadedTextAsset);
            }

            _initState = InitState.LoadTableDone;
        }

        private void DeserializeTable()
        {
            _initState = InitState.Deserialize;
            if (_typeNameToTextAssetDic.Count == 0)
            {
                _initState = InitState.DeserializeDone;
                return;
            }

            //加入要反序列化的
            if (_deserializeTableQueue.Count == 0)
            {
                foreach (var tableTypeName in _typeNameToTextAssetDic.Keys)
                {
                    if (!_typeNameToTableInfoDic.TryGetValue(tableTypeName, out var tableInfo))
                        continue;

                    _deserializeTableQueue.Enqueue(tableInfo);
                }
            }

            //反序列化
            while (_deserializeTableQueue.Count > 0)
            {
                var tableInfo = _deserializeTableQueue.Dequeue();
                if (!_typeNameToTextAssetDic.TryGetValue(tableInfo.TableTypeName, out var textAsset))
                    continue;

                tableInfo.Table.DeserializeJson(textAsset.text);
            }

            _initState = InitState.DeserializeDone;
        }

        private void InitDone()
        {
            _initState = InitState.Done;
            _loadTableInfoQueue.Clear();
            _typeNameToTextAssetDic.Clear();
            _deserializeTableQueue.Clear();
        }

        #endregion
    }
}
