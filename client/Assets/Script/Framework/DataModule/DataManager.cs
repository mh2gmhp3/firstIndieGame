using Logging;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Utility;

namespace DataModule
{
    public class DataManager
    {
        private const string DEFAULT_DATA_FOLDER_PREFIX = "data_";

        private string _dataRootFolderPath = string.Empty;
        private string _dataFolderPrefix = string.Empty;

        private readonly Dictionary<string, IDataRepository> _globalTypeNameToDataRepositoryDic =
            new Dictionary<string, IDataRepository>();
        private readonly Dictionary<string, IDataRepository> _slotTypeNameToDataRepositoryDic =
            new Dictionary<string, IDataRepository>();

        private int _curDataId = -1;
        public int CurDataId => _curDataId;

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="rootFolderPath"></param>
        /// <param name="folderPrefix"></param>
        public void Init(string rootFolderPath = "", string folderPrefix = "")
        {
            if (string.IsNullOrEmpty(rootFolderPath))
                _dataRootFolderPath = Path.Combine(Application.persistentDataPath, "save_data");
            else
                _dataRootFolderPath = rootFolderPath;
            if (string.IsNullOrEmpty(folderPrefix))
                _dataFolderPrefix = DEFAULT_DATA_FOLDER_PREFIX;
            else
                _dataFolderPrefix = folderPrefix;
            InitRepository();
        }

        /// <summary>
        /// 初始化所有資料庫
        /// </summary>
        private void InitRepository()
        {
            InitGlobalRepository();
            InitSlotRepository();
        }

        #region Slot資料庫 Id為單位

        /// <summary>
        /// 獲取資料庫
        /// </summary>
        /// <typeparam name="TDataRepository"></typeparam>
        /// <returns></returns>
        public TDataRepository GetDataRepository<TDataRepository>() where TDataRepository : IDataRepository
        {
            var type = typeof(TDataRepository);
            var typeName = type.Name.ToLower();
            if (!_slotTypeNameToDataRepositoryDic.TryGetValue(typeName, out var result))
                return default;

            return (TDataRepository)result;
        }

        /// <summary>
        /// 保存指定資料
        /// </summary>
        /// <param name="id"></param>
        public void Save(int id)
        {
            var dataFolderPath = GetDataFolderPath(id);
            EnsureFolder(dataFolderPath);
            foreach (var typeNameToDataRepository in _slotTypeNameToDataRepositoryDic)
            {
                var typeName = typeNameToDataRepository.Key;
                var dataRepository = typeNameToDataRepository.Value;

                var path = Path.Combine(dataFolderPath, typeName);
                SaveJsonFormat(path, dataRepository);
            }
        }

        /// <summary>
        /// 讀取指定資料
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool Load(int id)
        {
            var dataFolderPath = GetDataFolderPath(id);
            if (!Directory.Exists(dataFolderPath))
                return false;
            //重建
            InitSlotRepository();
            foreach (var typeNameToDataRepository in _slotTypeNameToDataRepositoryDic)
            {
                var typeName = typeNameToDataRepository.Key;
                var dataRepository = typeNameToDataRepository.Value;

                var path = Path.Combine(dataFolderPath, typeName);
                LoadJsonFormat(path, dataRepository);
            }
            _curDataId = id;
            return true;
        }

        /// <summary>
        /// 刪除指定資料
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool Delete(int id)
        {
            var dataFolderPath = GetDataFolderPath(id);
            if (!Directory.Exists(dataFolderPath))
                return false;

            Directory.Delete(dataFolderPath, true);
            return true;
        }

        /// <summary>
        /// 指定資料是否存在
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool Exist(int id)
        {
            var dataFolderPath = GetDataFolderPath(id);
            return Directory.Exists(dataFolderPath);
        }

        /// <summary>
        /// 創建新資料
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public void CreateNew(int id)
        {
            Delete(id);
            //直接重建
            InitSlotRepository();
            Save(id);
            _curDataId = id;
        }

        /// <summary>
        /// 初始化Slot資料庫
        /// </summary>
        private void InitSlotRepository()
        {
            _slotTypeNameToDataRepositoryDic.Clear();
            var typeAndAttributeList = AttributeUtility.GetAllAtttibuteTypeList<DataRepositoryAttribute>();
            for (int i = 0; i < typeAndAttributeList.Count; i++)
            {
                var type = typeAndAttributeList[i].Type;
                var attribute = typeAndAttributeList[i].Attribute;
                var typeName = type.Name.ToLower();
                if (attribute.IsGlobal)
                    continue;

                if (_slotTypeNameToDataRepositoryDic.ContainsKey(typeName))
                {
                    Log.LogError($"DataManger.InitSlotRepository: have duplicate DataRepository name:{typeName}");
                    continue;
                }

                var dataRepository = System.Activator.CreateInstance(type, this, attribute.Version) as IDataRepository;
                _slotTypeNameToDataRepositoryDic.Add(typeName, dataRepository);
            }
        }

        #endregion

        #region Global資料庫 全局

        /// <summary>
        /// 獲取Global資料庫
        /// </summary>
        /// <typeparam name="TDataRepository"></typeparam>
        /// <returns></returns>
        public TDataRepository GetGlobalDataRepository<TDataRepository>() where TDataRepository : IDataRepository
        {
            var type = typeof(TDataRepository);
            var typeName = type.Name.ToLower();
            if (!_globalTypeNameToDataRepositoryDic.TryGetValue(typeName, out var result))
                return default;

            return (TDataRepository)result;
        }

        /// <summary>
        /// 保存Global資料
        /// </summary>
        /// <param name="id"></param>
        public void SaveGlobal()
        {
            EnsureFolder(_dataRootFolderPath);
            foreach (var typeNameToDataRepository in _globalTypeNameToDataRepositoryDic)
            {
                var typeName = typeNameToDataRepository.Key;
                var dataRepository = typeNameToDataRepository.Value;

                var path = Path.Combine(_dataRootFolderPath, typeName);
                SaveJsonFormat(path, dataRepository);
            }
        }

        /// <summary>
        /// 讀取Global資料
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool LoadGlobal()
        {
            if (!Directory.Exists(_dataRootFolderPath))
                return false;

            foreach (var typeNameToDataRepository in _globalTypeNameToDataRepositoryDic)
            {
                var typeName = typeNameToDataRepository.Key;
                var dataRepository = typeNameToDataRepository.Value;

                var path = Path.Combine(_dataRootFolderPath, typeName);
                LoadJsonFormat(path, dataRepository);
            }
            return true;
        }

        /// <summary>
        /// 初始化Global資料庫
        /// </summary>
        private void InitGlobalRepository()
        {
            _globalTypeNameToDataRepositoryDic.Clear();
            var typeAndAttributeList = AttributeUtility.GetAllAtttibuteTypeList<DataRepositoryAttribute>();
            for (int i = 0; i < typeAndAttributeList.Count; i++)
            {
                var type = typeAndAttributeList[i].Type;
                var attribute = typeAndAttributeList[i].Attribute;
                var typeName = type.Name.ToLower();
                if (!attribute.IsGlobal)
                    continue;

                if (_globalTypeNameToDataRepositoryDic.ContainsKey(typeName))
                {
                    Log.LogError($"DataManger.InitGlobalRepository: have duplicate DataRepository name:{typeName}");
                    continue;
                }

                var dataRepository = System.Activator.CreateInstance(type, this, attribute.Version) as IDataRepository;
                _globalTypeNameToDataRepositoryDic.Add(typeName, dataRepository);
            }
        }

        #endregion

        #region JsomFormat Save Load

        private void SaveJsonFormat(string path, IDataRepository dataRepository)
        {
            if (dataRepository == null)
                return;

            var json = dataRepository.GetJsonFormat();
            File.WriteAllText(path, json, System.Text.Encoding.UTF8);
        }

        private void LoadJsonFormat(string path, IDataRepository dataRepository)
        {
            if (dataRepository == null)
                return;

            if (!File.Exists(path))
            {
                Log.LogWarning($"DataManager.LoadJsonFormat, file can not found Path:{path}");
                return;
            }
            dataRepository.LoadJsonFormat(File.ReadAllText(path, System.Text.Encoding.UTF8));
        }

        #endregion

        /// <summary>
        /// 確保資料夾
        /// </summary>
        /// <param name="path"></param>
        private void EnsureFolder(string path)
        {
            if (Directory.Exists(path))
                return;

            Directory.CreateDirectory(path);
        }

        /// <summary>
        /// 獲取指定檔案資料夾路徑
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private string GetDataFolderPath(int id)
        {
            return Path.Combine(_dataRootFolderPath, _dataFolderPrefix + id.ToString());
        }
    }
}
