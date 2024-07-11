using Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ComponentUtility
{
    [Serializable]
    public class GameObjectReferenceDatabase
    {
        [Serializable]
        public class GameObjectReference
        {
            public string Name;
            public List<GameObject> GameObjectList =
               new List<GameObject>();
        }

        [SerializeField]
        private List<GameObjectReference> _gameObjectReferenceList =
            new List<GameObjectReference>();

        [SerializeField]
        private Dictionary<string, List<Object>> _nameToObjectListDic =
            new Dictionary<string, List<Object>>();

#if UNITY_EDITOR

        public List<GameObjectReference> GameObjectReferenceList => _gameObjectReferenceList;

#endif

#if UNITY_EDITOR

        public void AddObject(GameObject go)
        {
            if (go == null)
                return;

            string name = go.name;
            int index = _gameObjectReferenceList.FindIndex((x) => x.Name == name);
            if (index >= 0)
                return; //Same Name

            var newGoRef = new GameObjectReference();
            newGoRef.Name = name;
            newGoRef.GameObjectList = new List<GameObject>() { go };

            _gameObjectReferenceList.Add(newGoRef);
        }

#endif

        public T GetObject<T>(string name) where T : Object
        {
            for (int i = 0; i < _gameObjectReferenceList.Count; i++)
            {
                var goRef = _gameObjectReferenceList[i];
                if (goRef.Name != name)
                    continue;

                if (goRef.GameObjectList.Count == 0)
                {
                    Log.LogError($"Object is empty, name:{name}", true);
                    return null;
                }

                if (typeof(T) == typeof(GameObject))
                    return goRef.GameObjectList[0] as T;

                T resultObj = goRef.GameObjectList[0].GetComponent<T>();
                if (resultObj == null)
                {
                    Log.LogError(
                        $"Object Type is invalid, " +
                        $"name:{name}, useType:{typeof(T).Name}",
                        true);
                    return null;
                }

                return resultObj;
            }

            Log.LogError(
                $"Object can not found, " +
                $"name:{name}, useType:{typeof(T).Name}",
                true);
            return null;
        }

        public List<T> GetObjectList<T>(string name) where T : Object
        {
            for (int i = 0; i < _gameObjectReferenceList.Count; i++)
            {
                var goRef = _gameObjectReferenceList[i];
                if (goRef.Name != name)
                    continue;

                if (goRef.GameObjectList.Count == 0)
                {
                    Log.LogError($"Object is empty, name:{name}", true);
                    return null;
                }

                List<T> resultObjList = new List<T>();

                if (typeof(T) == typeof(GameObject))
                {
                    for (int j = 0; j < goRef.GameObjectList.Count; j++)
                    {
                        resultObjList.Add(goRef.GameObjectList[j] as T);
                    }

                    return resultObjList;
                }

                for (int j = 0; j < goRef.GameObjectList.Count; j++)
                {

                    T resultObj = goRef.GameObjectList[j].GetComponent<T>();
                    if (resultObj == null)
                    {
                        Log.LogError(
                            $"Object Type is invalid, " +
                            $"index:{j} name:{name}, useType:{typeof(T).Name}",
                            true);
                        continue;
                    }

                    resultObjList.Add(resultObj);
                }

                return resultObjList;
            }

            Log.LogError(
                $"Object can not found, " +
                $"name:{name}, useType:{typeof(T).Name}",
                true);
            return null;
        }
    }
}
