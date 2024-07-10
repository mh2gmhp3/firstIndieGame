using Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ComponentUtility
{
    [Serializable]
    public class ObjectReference
    {
        [SerializeField]
        private List<Object> _objectList =
            new List<Object>();

        private Dictionary<string, Object> _nameToObjectDic =
            new Dictionary<string, Object>();

        public T GetObject<T>(string name) where T : Object
        {
            T resultObj = null;
            if (!_nameToObjectDic.TryGetValue(name, out var obj))
            {
                Log.LogError($"Object not exist, name:{name}", true);
                return null;
            }

            resultObj = obj as T;
            if (resultObj == null)
            {
                Log.LogError(
                    $"Object Type is invalid, " +
                    $"name:{name}, useType:{typeof(T).Name}, " +
                    $"registerType:{obj.GetType().Name}");
                return null;
            }

            return resultObj;
        }
    }
}
