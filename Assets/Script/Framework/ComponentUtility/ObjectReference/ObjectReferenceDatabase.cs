using Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Framework.ComponentUtility
{
    [Serializable]
    public class ObjectReferenceDatabase
    {
        public static Object GetObjectByType(Object obj, Type type)
        {
            Object resultObj;
            if (type == typeof(GameObject))
                resultObj = obj.GameObject();
            else
                resultObj = obj.GetComponent(type);

            return resultObj;
        }

        [Serializable]
        public class ObjectReference
        {
            public string Name;
            public string TypeName;
            public List<Object> ObjectList =
               new List<Object>();

#if UNITY_EDITOR
            public void AddObject(Object obj)
            {
                if (obj == null)
                    return;

                if (ObjectList.Contains(obj))
                    return;

                if (ObjectList.Count == 0)
                    return;

                Object newObj = GetObjectByType(obj, ObjectList[0].GetType());
                if (newObj == null)
                    return;

                ObjectList.Add(newObj);
            }

            public void ChangeType(Type type)
            {
                var newTypeObjectList = new List<Object>();
                foreach (var obj in ObjectList)
                {
                    Object newObj = GetObjectByType(obj, type);
                    //有一個錯就不變換
                    if (newObj == null)
                        return;

                    newTypeObjectList.Add(newObj);
                }

                ObjectList.Clear();
                ObjectList.AddRange(newTypeObjectList);
            }
#endif
        }

        [SerializeField]
        private List<ObjectReference> _objectReferenceList =
            new List<ObjectReference>();

#if UNITY_EDITOR
        public List<ObjectReference> ObjectReferenceList => _objectReferenceList;
#endif

#if UNITY_EDITOR
        public void AddObjectReference(Object obj)
        {
            if (obj == null)
                return;

            string name = obj.name;
            int index = _objectReferenceList.FindIndex((x) => x.Name == name);
            if (index >= 0)
                return; //Same Name

            var newGoRef = new ObjectReference();
            newGoRef.Name = name;
            //直接加入GameObject
            var go = obj.GameObject();
            newGoRef.TypeName = go.GetType().Name;
            newGoRef.ObjectList = new List<Object>() { go };

            _objectReferenceList.Add(newGoRef);
        }
#endif

        public T GetObject<T>(string name) where T : Object
        {
            for (int i = 0; i < _objectReferenceList.Count; i++)
            {
                var objRef = _objectReferenceList[i];
                if (objRef.Name != name)
                    continue;

                if (objRef.ObjectList.Count == 0)
                {
                    Log.LogError($"Object is empty, name:{name}", true);
                    return null;
                }

                Type type = typeof(T);
                T resultObj = GetObjectByType(objRef.ObjectList[0], type) as T;
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
            for (int i = 0; i < _objectReferenceList.Count; i++)
            {
                var objRef = _objectReferenceList[i];
                if (objRef.Name != name)
                    continue;

                if (objRef.ObjectList.Count == 0)
                {
                    Log.LogError($"Object is empty, name:{name}", true);
                    return null;
                }

                List<T> resultObjList = new List<T>();
                Type type = typeof(T);

                for (int j = 0; j < objRef.ObjectList.Count; j++)
                {
                    T resultObj = GetObjectByType(objRef.ObjectList[0], type) as T;
                    if (resultObj == null)
                    {
                        Log.LogError(
                            $"Object Type is invalid, " +
                            $"name:{name}, useType:{type.Name}",
                            true);
                        return null;
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
