using Logging;
using Newtonsoft.Json;
using System;
using System.IO;

namespace DataModule
{
    public interface IDataRepository
    {
        void Save(string dataFolderPath, string name);
        void Load(string dataFolderPath, string name);
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

        void IDataRepository.Save(string dataFolderPath, string name)
        {
            var path = Path.Combine(dataFolderPath, name);
            var saveData = new SaveData
            {
                Version = _version,
                Data = _data
            };
            var json = JsonConvert.SerializeObject(saveData);
            File.WriteAllText(path, json, System.Text.Encoding.UTF8);
        }

        void IDataRepository.Load(string dataFolderPath, string name)
        {
            var path = Path.Combine(dataFolderPath, name);
            if (!File.Exists(path))
            {
                Log.LogWarning($"{this.GetType().Name}.Load, file can not found Path:{path}");
                return;
            }
            var json = File.ReadAllText(path, System.Text.Encoding.UTF8);
            var saveData = JsonConvert.DeserializeObject<SaveData>(json);
            _data = saveData.Data;
            OnLoad(_version, saveData.Version);
        }
    }
}
