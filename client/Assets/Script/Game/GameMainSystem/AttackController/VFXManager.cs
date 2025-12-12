using AssetModule;
using Logging;
using System;
using System.Collections.Generic;
using UnityEngine;
using Utility;

namespace GameMainModule.VFX
{
    public class VFXObject
    {
        public string Path;
        public GameObject GameObject;
        public Transform Transform;
        public VFXObjectSetting VFXObjectSetting;

        public VFXObject(string path, GameObject gameObject)
        {
            Path = path;
            GameObject = gameObject;
            Transform = gameObject.transform;
            VFXObjectSetting = gameObject.GetComponent<VFXObjectSetting>();
        }

        public void Play()
        {
            if (VFXObjectSetting == null)
            {
                Log.LogWarning($"VFXObject.Play VFXObjectSetting is null, Path:{Path}");
                return;
            }

            if (VFXObjectSetting.MainParticleSystem == null)
                return;

            VFXObjectSetting.MainParticleSystem.Play();
        }
    }
    public class VFXManager
    {
        private static VFXManager _instance = null;

        private Dictionary<string, Queue<VFXObject>> _pathToPool = new Dictionary<string, Queue<VFXObject>>();
        private Dictionary<string, GameObject> _pathToTemplate = new Dictionary<string, GameObject>();

        public VFXManager()
        {
            _instance = this;
        }

        public static void GetVFX(GameObjectIndirectField vfx, Action<VFXObject> action)
        {
            if (_instance == null)
                return;

            _instance.InternalGetVFX(vfx, action);
        }

        public static void RecycleVFX(VFXObject vfx)
        {
            if (_instance == null)
                return;

            _instance.InternalRecycleVFX(vfx);
        }

        private void InternalGetVFX(GameObjectIndirectField vfx, Action<VFXObject> action)
        {
            if (vfx == null || action == null)
                return;
            var path = vfx.Path;
            var pool = GetPool(path);
            VFXObject vfxO = null;
            if (pool.Count == 0)
            {
                vfxO = CreateVFX(vfx);
            }
            else
            {
                vfxO = pool.Dequeue();
            }

            vfxO.GameObject.SetActive(true);
            action(vfxO);
        }

        private void InternalRecycleVFX(VFXObject vfx)
        {
            if (vfx == null)
                return;
            var pool = GetPool(vfx.Path);
            vfx.GameObject.SetActive(false);
            pool.Enqueue(vfx);
        }

        private Queue<VFXObject> GetPool(string path)
        {
            if (!_pathToPool.TryGetValue(path, out var pool))
            {
                pool = new Queue<VFXObject>();
                _pathToPool.Add(path, pool);
            }
            return pool;
        }

        private VFXObject CreateVFX(GameObjectIndirectField vfx)
        {
            if (!_pathToTemplate.TryGetValue(vfx.Path, out var template))
            {
                template = AssetSystem.LoadAsset(vfx);
                _pathToTemplate.Add(vfx.Path, template);
            }

            var go = ObjectUtility.InstantiateWithoutClone(template);
            return new VFXObject(vfx.Path, go);
        }
    }
}
