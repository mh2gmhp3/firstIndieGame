using Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

namespace FormModule
{
    public abstract class TableBase<T> : ITable where T : ITableData
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
        }
    }
}
