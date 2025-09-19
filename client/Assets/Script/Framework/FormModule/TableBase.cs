using Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;

namespace FormModule
{
    public abstract class TableBase<T> : ITable where T : ITableRow
    {
        private readonly List<T> _rows = new List<T>();
        private readonly Dictionary<int, T> _rowDic = new Dictionary<int, T>();

        public void DeserializeJson(string json)
        {
            var typeName = this.GetType().Name;
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            var rows = JsonConvert.DeserializeObject<List<T>>(json, settings);
            if (rows == null)
            {
                Log.LogError($"TableBase.{typeName}.DeserializeJson failed, Json Deserialize failed");
                return;
            }
            _rows.AddRange(rows);
            for (int i = 0; i < _rows.Count; i++)
            {
                var row = _rows[i];
                if (_rowDic.ContainsKey(row.Id))
                {
                    Log.LogError($"TableBase.{typeName}.DeserializeJson, Id duplicate:{row.Id}");
                    continue;
                }
                _rowDic[row.Id] = row;
            }
            DoInit();
        }

        public bool TryGetData(int id, out T data)
        {
            return _rowDic.TryGetValue(id, out data);
        }

        public List<T> GetDataList() { return _rows; }

        protected virtual void DoInit() { }
    }
}
