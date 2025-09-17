using Logging;
using Newtonsoft.Json;
using System;
using System.IO;

namespace DataModule
{
    public interface IDataRepository
    {
        string GetJsonFormat();
        void LoadJsonFormat(string json);
    }

    public class DataRepositoryAttribute : Attribute
    {
        public int Version;
        public bool IsGlobal;

        public DataRepositoryAttribute(int version, bool isGlobal = false)
        {
            Version = version;
            IsGlobal = isGlobal;
        }
    }

    public abstract class DataRepository<TData> : IDataRepository where TData : class, new()
    {
        private class SaveData
        {
            public int Version;
            public TData Data;
        }

        protected DataManager _dataManager;
        protected TData _data = new TData();
        private int _version;

        protected DataRepository(DataManager dataManager, int version)
        {
            _dataManager = dataManager;
            _version = version;
        }

        protected virtual void OnLoad(int currentVersion, int loadedVersion) { }

        string IDataRepository.GetJsonFormat()
        {
            var saveData = new SaveData
            {
                Version = _version,
                Data = _data
            };
            return JsonConvert.SerializeObject(saveData);
        }

        void IDataRepository.LoadJsonFormat(string json)
        {
            var saveData = JsonConvert.DeserializeObject<SaveData>(json);
            _data = saveData.Data;
            OnLoad(_version, saveData.Version);
        }
    }
}
