using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace TerrainModule
{
    public class PrefabController
    {
        private class PrefabInstanceInfo
        {
            public GameObject GameObject;
            public Transform Transform;

            public PrefabInstanceInfo(GameObject gameObject)
            {
                GameObject = gameObject;
                Transform = gameObject.transform;
            }
        }

        private GameObject _prefab;
        private Transform _parent;
        private Dictionary<int, PrefabInstanceInfo> _idToInstanceDic = new Dictionary<int, PrefabInstanceInfo>();
        private List<int> _instanceIdList = new List<int>();
        private ObjectPool<PrefabInstanceInfo> _pool;

        public PrefabController(GameObject prefab, Transform parent)
        {
            _parent = parent;
            _prefab = prefab;
            _pool = new ObjectPool<PrefabInstanceInfo>(CreateInstance, actionOnRelease:OnInstanceRelease);
        }

        public void AddInstance(int id, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            if (_idToInstanceDic.ContainsKey(id))
                return;

            var instance = _pool.Get();
            instance.Transform.position = position;
            instance.Transform.rotation = rotation;
            instance.Transform.localScale = scale;
            _idToInstanceDic.Add(id, instance);
            _instanceIdList.Add(id);
        }

        public void RemoveInstance(int id)
        {
            if (!_idToInstanceDic.TryGetValue(id, out var instanceInfo))
                return;

            _pool.Release(instanceInfo);
            _idToInstanceDic.Remove(id);
            _instanceIdList.Remove(id);
        }

        private PrefabInstanceInfo CreateInstance()
        {
            var go = Object.Instantiate(_prefab);
            if (_parent != null)
                go.transform.SetParent(_parent);

            return new PrefabInstanceInfo(go);
        }

        private void OnInstanceRelease(PrefabInstanceInfo instanceInfo)
        {
            instanceInfo.Transform.position = new Vector3(9999, 9999, 9999);
        }
    }
}
