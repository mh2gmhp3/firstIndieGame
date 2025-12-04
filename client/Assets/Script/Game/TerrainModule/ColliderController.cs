using Extension;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Rendering.Universal;
using UnityEngine;
using UnityEngine.Pool;
using static TerrainModule.TerrainDefine;

namespace TerrainModule
{
    public class ColliderController
    {
        private class ColliderCollection
        {
            public List<int> ColliderInstanceIdList = new List<int>();
        }

        public struct ColliderSingleInfo
        {
            public ColliderType ColliderType;
            public Vector3 Center;
            public Vector3 Size;
            public float Radius;
            public float Height;
            public int Direction;

            public Vector3 Position;
            public Quaternion Rotation;
            public Vector3 Scale;
        }

        public struct ColliderInfo
        {
            public List<ColliderSingleInfo> ColliderSingleInfoList;

            public bool IsValid()
            {
                if (ColliderSingleInfoList == null)
                    return false;
                if (ColliderSingleInfoList.Count == 0)
                    return false;
                return true;
            }
        }

        private class ColliderTemplateSingleData
        {
            public ColliderType ColliderType;
            public Vector3 Center;
            public Vector3 Size;
            public float Radius;
            public float Height;
            public int Direction;

            public Vector3 Position;
            public Quaternion Rotation;
            public Vector3 Scale;

            public ColliderTemplateSingleData(ColliderSingleInfo info)
            {
                ColliderType = info.ColliderType;
                Center = info.Center;
                Size = info.Size;
                Radius = info.Radius;
                Height = info.Height;
                Direction = info.Direction;

                Position = info.Position;
                Rotation = info.Rotation;
                Scale = info.Scale;
            }
        }

        private class ColliderTemplateData
        {
            public List<ColliderTemplateSingleData> ColliderTemplateDataList = new List<ColliderTemplateSingleData>();
        }

        private abstract class ColliderObj
        {
            public GameObject GameObject;
            public Transform Transform;
            public Collider Collider;

            public abstract ColliderType Type { get; }

            public ColliderObj(GameObject gameObject, Transform transform, Collider collider)
            {
                GameObject = gameObject;
                Transform = transform;
                Collider = collider;
            }

            public abstract void SetColliderData(ColliderTemplateSingleData data);
        }

        private class BoxColliderObj : ColliderObj
        {
            public BoxCollider BoxCollider;

            public override ColliderType Type => ColliderType.Box;

            public BoxColliderObj(GameObject gameObject, Transform transform, BoxCollider collider) : base(gameObject, transform, collider)
            {
                BoxCollider = collider;
            }

            public override void SetColliderData(ColliderTemplateSingleData data)
            {
                BoxCollider.center = data.Center;
                BoxCollider.size = data.Size;
            }
        }

        private class SphereColliderObj : ColliderObj
        {
            public SphereCollider SphereCollider;

            public override ColliderType Type => ColliderType.Sphere;

            public SphereColliderObj(GameObject gameObject, Transform transform, SphereCollider collider) : base(gameObject, transform, collider)
            {
                SphereCollider = collider;
            }

            public override void SetColliderData(ColliderTemplateSingleData data)
            {
                SphereCollider.center = data.Center;
                SphereCollider.radius = data.Radius;
            }
        }

        private class CapsuleColliderObj : ColliderObj
        {
            public CapsuleCollider CapsuleCollider;

            public override ColliderType Type => ColliderType.Capsule;

            public CapsuleColliderObj(GameObject gameObject, Transform transform, CapsuleCollider collider) : base(gameObject, transform, collider)
            {
                CapsuleCollider = collider;
            }

            public override void SetColliderData(ColliderTemplateSingleData data)
            {
                CapsuleCollider.center = data.Center;
                CapsuleCollider.radius = data.Radius;
                CapsuleCollider.height = data.Height;
                CapsuleCollider.direction = data.Direction;
            }
        }

        private Transform _colliderRoot;

        private Dictionary<int, ColliderTemplateData> _idToTemplateData = new Dictionary<int, ColliderTemplateData>();
        private Dictionary<int, ColliderCollection> _instanceIdToColliderCollection = new Dictionary<int, ColliderCollection>();
        private ObjectPool<ColliderCollection> _colliderCollectionPool;

        private Dictionary<int, ObjectPool<ColliderObj>> _typeToColliderPool = new Dictionary<int, ObjectPool<ColliderObj>>();
        private Dictionary<int, ColliderObj> _idToUsingColliderObj = new Dictionary<int, ColliderObj>();

        private Func<int, ColliderInfo> _colliderGetAction;

        private int _colliderInsId = 0;

        public ColliderController(Transform parent, Func<int, ColliderInfo> getColliderAction)
        {
            var colliderRootGo = new GameObject("ColliderRoot");
            _colliderRoot = colliderRootGo.transform;
            if (parent != null)
                _colliderRoot.SetParent(parent);
            _colliderRoot.Reset();

            _colliderGetAction = getColliderAction;
            _colliderCollectionPool = new ObjectPool<ColliderCollection>(GetColliderCollection, actionOnRelease: ReleaseColliderCollection);

            _typeToColliderPool.Add((int)ColliderType.Box, new ObjectPool<ColliderObj>(() => { return CreateColliderForPool(ColliderType.Box); }));
            _typeToColliderPool.Add((int)ColliderType.Sphere, new ObjectPool<ColliderObj>(() => { return CreateColliderForPool(ColliderType.Sphere); }));
            _typeToColliderPool.Add((int)ColliderType.Capsule, new ObjectPool<ColliderObj>(() => { return CreateColliderForPool(ColliderType.Capsule); }));
        }

        public void AddColliderInstance(int id, int instanceId, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            if (_instanceIdToColliderCollection.ContainsKey(instanceId))
                return;

            if (!_idToTemplateData.TryGetValue(id, out var templateData))
            {
                if (_colliderGetAction == null)
                    return;

                var colliderInfo = _colliderGetAction(id);
                if (!colliderInfo.IsValid())
                    return;
                templateData = new ColliderTemplateData();
                for (int i = 0; i < colliderInfo.ColliderSingleInfoList.Count; i++)
                {
                    templateData.ColliderTemplateDataList.Add(new ColliderTemplateSingleData(colliderInfo.ColliderSingleInfoList[i]));
                }

                _idToTemplateData.Add(id, templateData);
            }

            var collection = _colliderCollectionPool.Get();
            for (int i = 0; i < templateData.ColliderTemplateDataList.Count; i++)
            {
                var colliderTemplate = templateData.ColliderTemplateDataList[i];
                //ColliderSetup
                var colliderObj = GetColliderObj(colliderTemplate.ColliderType);
                colliderObj.SetColliderData(colliderTemplate);
                colliderObj.Transform.position = position + colliderTemplate.Position;
                colliderObj.Transform.rotation = rotation * colliderTemplate.Rotation;
                colliderObj.Transform.localScale = new Vector3(
                    scale.x * colliderTemplate.Scale.x,
                    scale.y * colliderTemplate.Scale.y,
                    scale.z * colliderTemplate.Scale.z);
                //IdMapping
                var colliderId = ++_colliderInsId;
                collection.ColliderInstanceIdList.Add(colliderId);
                _idToUsingColliderObj.Add(colliderId, colliderObj);
            }
            _instanceIdToColliderCollection.Add(instanceId, collection);
        }

        public void RemoveColliderInstance(int instanceId)
        {
            if (!_instanceIdToColliderCollection.TryGetValue(instanceId, out var colliderCollection))
                return;

            for (int i = 0; i < colliderCollection.ColliderInstanceIdList.Count; i++)
            {
                var colliderInstanceId = colliderCollection.ColliderInstanceIdList[i];
                if (!_idToUsingColliderObj.TryGetValue(colliderInstanceId, out var colliderObj))
                    continue;

                _idToUsingColliderObj.Remove(colliderInstanceId);
                ReleaseCollider(colliderObj);
            }

            _instanceIdToColliderCollection.Remove(instanceId);
        }

        private ColliderCollection GetColliderCollection()
        {
            return new ColliderCollection();
        }

        private void ReleaseColliderCollection(ColliderCollection colliderCollection)
        {
            colliderCollection.ColliderInstanceIdList.Clear();
        }

        private ColliderObj GetColliderObj(ColliderType type)
        {
            if (!_typeToColliderPool.TryGetValue((int)type, out var pool))
                return null;

            var colliderObj = pool.Get();
            colliderObj.GameObject.SetActive(true);
            return colliderObj;
        }

        private void ReleaseCollider(ColliderObj colliderObj)
        {
            if (!_typeToColliderPool.TryGetValue((int)colliderObj.Type, out var pool))
                return;

            colliderObj.GameObject.SetActive(false);
            pool.Release(colliderObj);
        }

        private ColliderObj CreateColliderForPool(ColliderType type)
        {
            switch (type)
            {
                case ColliderType.Box:
                    var boxGo = new GameObject("BoxCollider");
                    boxGo.transform.SetParent(_colliderRoot);
                    return new BoxColliderObj(boxGo, boxGo.transform, boxGo.AddComponent<BoxCollider>());
                case ColliderType.Sphere:
                    var sphereGo = new GameObject("SphereCollider");
                    sphereGo.transform.SetParent(_colliderRoot);
                    return new SphereColliderObj(sphereGo, sphereGo.transform, sphereGo.AddComponent<SphereCollider>());
                case ColliderType.Capsule:
                    var capsuleGo = new GameObject("CapsuleCollider");
                    capsuleGo.transform.SetParent(_colliderRoot);
                    return new CapsuleColliderObj(capsuleGo, capsuleGo.transform, capsuleGo.AddComponent<CapsuleCollider>());
                default:
                    var defaultGo = new GameObject("DefaultBoxCollider");
                    defaultGo.transform.SetParent(_colliderRoot);
                    return new BoxColliderObj(defaultGo, defaultGo.transform, defaultGo.AddComponent<BoxCollider>());
            }
        }
    }
}
