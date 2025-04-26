using Logging;
using Mono.Cecil;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Utility;
using static UnityEngine.ParticleSystem;

namespace CollisionModule
{
    public class CollisionAreaManager : ICollisionAreaManager
    {
        public class RegisterColliderData
        {
            public Dictionary<int, Collider> IdToRegisterColliderDic =
               new Dictionary<int, Collider>();

            public bool HaveCollider => IdToRegisterColliderDic.Count > 0;
        }

        public class ColliderGroup
        {
            public Dictionary<int, Collider> IdToColliderDic =
               new Dictionary<int, Collider>();

            private Dictionary<int, int> _instancIdToIdDic =
                new Dictionary<int, int>();

            public void FillColliders(RegisterColliderData colliderData)
            {
                IdToColliderDic.Clear();
                foreach (var idToCollider in colliderData.IdToRegisterColliderDic)
                {
                    int id = idToCollider.Key;
                    var collider = idToCollider.Value;
                    IdToColliderDic.Add(idToCollider.Key, collider);
                    _instancIdToIdDic.Add(collider.GetInstanceID(), id);
                }
            }

            public bool TryGetColliderId(int instanceId, out int colliderId)
            {
                return _instancIdToIdDic.TryGetValue(instanceId, out colliderId);
            }
        }

        #region Registered Collider

        private int _nextGroupId = 0;

        private Dictionary<int, ColliderGroup> _groupIdToColliderGroupDic =
            new Dictionary<int, ColliderGroup>();

        private Dictionary<int, int> _instancIdToGroupIdDic =
            new Dictionary<int, int>();

        #endregion

        #region CollisionArea

        //TODO 先評估看看Activator.CreateInstance在創建時的消耗 因為有Pool基本上也不會太多次 如果真的有消耗嘗試改用已經實例化出來的做Copy
        private Dictionary<int, Type> _areaTypeToAreaClassTypeDic =
            new Dictionary<int, Type>();

        private Dictionary<int, Queue<CollisionArea>> _areaTypeToAreaPoolDic =
            new Dictionary<int, Queue<CollisionArea>>();

        private List<CollisionArea> _runingCollisionAreaList =
            new List<CollisionArea>();

        private List<CollisionArea> _endCollisionAreaList =
            new List<CollisionArea>();

        #endregion

        public CollisionAreaManager()
        {
            CollectionCollisionAreaType();
        }

        #region Pubilc Method Update DrawGizmos

        public void DoUpdate()
        {
            ProcessCollisionArea();
        }

        public void DoOnGUI()
        {
            for (int i = 0; i < _runingCollisionAreaList.Count; i++)
            {
                _runingCollisionAreaList[i].DoOnGUI();
            }
        }

        public void DoDrawGizmos()
        {
            for (int i = 0; i < _runingCollisionAreaList.Count; i++)
            {
                _runingCollisionAreaList[i].DoDrawGizmos();
            }
        }

        #endregion

        #region Public Method RegisterCollider UnregisterCollider

        public int RegisterCollider(RegisterColliderData colliderData)
        {
            if (colliderData == null)
            {
                Log.LogError("RegisterColliderData is null", true);
                return 0;
            }

            if (!colliderData.HaveCollider)
            {
                Log.LogWarning("RegisterColliderData Collider is empty", true);
                return 0;
            }

            if (!CheckColliderIsValid(colliderData, out string msg))
            {
                Log.LogWarning($"RegisterColliderData Invalid Msg:\n{msg}", true);
                return 0;
            }

            int newGroupId = GetNextGroupId();
            AddCollider(newGroupId, colliderData);

            return newGroupId;
        }

        public void UnregisterCollider(int groupId)
        {
            RemoveCollider(groupId);
        }

        #endregion

        #region Public Method CreateCollisionArea

        public void CreateCollisionArea(ICollisionAreaSetupData setupData)
        {
            InternalCreateCollisionArea(setupData);
        }

        #endregion

        #region Imp ICollisionAreaManager

        bool ICollisionAreaManager.TryGetColliderId(int instanceId, out int groupId, out int colliderId)
        {
            colliderId = 0;
            if (!_instancIdToGroupIdDic.TryGetValue(instanceId, out groupId))
                return false;

            if (!_groupIdToColliderGroupDic.TryGetValue(groupId, out var group))
                return false;

            return group.TryGetColliderId(instanceId, out colliderId);
        }

        #endregion

        #region Private Method ColliderGroup

        private int GetNextGroupId()
        {
            return _nextGroupId++;
        }

        private bool CheckColliderIsValid(RegisterColliderData colliderData, out string msg)
        {
            msg = string.Empty;
            foreach (var idToCollider in colliderData.IdToRegisterColliderDic)
            {
                int id = idToCollider.Key;
                var collider = idToCollider.Value;
                if (collider == null)
                {
                    msg += $"Id:{idToCollider.Key} Collider is null\n";
                    continue;
                }

                int colliderInsId = collider.GetInstanceID();
                if (_instancIdToGroupIdDic.ContainsKey(colliderInsId))
                {
                    msg += $"Id:{idToCollider.Key} Collider is exist InsId:{colliderInsId}\n";
                    continue;
                }
            }

            return string.IsNullOrEmpty(msg);
        }

        private void AddCollider(int groupId, RegisterColliderData colliderData)
        {
            //groupId基本上是遞增的 照正常來說不可能有重複的 必免內部使用錯誤先直接return
            if (_groupIdToColliderGroupDic.ContainsKey(groupId))
            {
                Log.LogError("GroupId duplicate");
                return;
            }

            //TODO 先不用Pool 雖然在怪物大量生成時有機會導致連續new的狀況 但先不處理
            var newColliderGroup = new ColliderGroup();
            newColliderGroup.FillColliders(colliderData);

            foreach (var collider in colliderData.IdToRegisterColliderDic.Values)
            {
                if (collider == null)
                {
                    Log.LogWarning($"GroupId:{groupId} have null collider");
                    continue;
                }
                _instancIdToGroupIdDic.Add(collider.GetInstanceID(), groupId);
            }

            _groupIdToColliderGroupDic.Add(groupId, newColliderGroup);
        }

        private void RemoveCollider(int groupId)
        {
            if (!_groupIdToColliderGroupDic.TryGetValue(groupId, out var colliderGroup))
            {
                Log.LogError("GroupId not exist");
                return;
            }

            foreach (var collider in colliderGroup.IdToColliderDic.Values)
            {
                if (collider == null)
                {
                    Log.LogWarning($"GroupId:{groupId} have null collider");
                    continue;
                }
                _instancIdToGroupIdDic.Remove(collider.GetInstanceID());
            }

            _groupIdToColliderGroupDic.Remove(groupId);
        }

        #endregion

        #region Private Method CollisionArea

        private void CollectionCollisionAreaType()
        {
            var collisionAreaTypeList = AttributeUtility.GetAllAtttibuteTypeList<CollisionAreaAttribute>();
            for (int i = 0; i < collisionAreaTypeList.Count; i++)
            {
                int areaType = collisionAreaTypeList[i].Attribute.AreaType;
                var classType = collisionAreaTypeList[i].Type;

                if (_areaTypeToAreaClassTypeDic.ContainsKey(areaType))
                {
                    Log.LogWarning($"AreaType:{areaType} have duplicates type:{classType.Name} ignore this type", true);
                    continue;
                }

                _areaTypeToAreaClassTypeDic.Add(areaType, classType);
            }
        }

        private void InternalCreateCollisionArea(ICollisionAreaSetupData setupData)
        {
            if (setupData == null)
            {
                Log.LogWarning("SetupData is null, do not create CollsionArea");
                return;
            }

            if (!TryGetCollisionArea(setupData.AreaType, out var collisionArea))
            {
                Log.LogWarning("SetupData Invalid, get collisionArea failed");
                return;
            }

            collisionArea.Setup(setupData);
            collisionArea.DoCreate();
            _runingCollisionAreaList.Add(collisionArea);
        }

        private void ProcessCollisionArea()
        {
            for (int i = 0; i < _runingCollisionAreaList.Count; i++)
            {
                var runingCollisionArea = _runingCollisionAreaList[i];
                runingCollisionArea.DoUpdate();
                if (runingCollisionArea.IsEnd)
                    _endCollisionAreaList.Add(runingCollisionArea);
            }

            int endCount = _endCollisionAreaList.Count;
            if (endCount > 0)
            {
                for (int i = 0; i < endCount; i++)
                {
                    var endCollisionArea = _endCollisionAreaList[i];
                    _runingCollisionAreaList.Remove(endCollisionArea);
                    RecycleCollisionArea(endCollisionArea);
                }
                _endCollisionAreaList.Clear();
            }
        }

        private bool TryGetCollisionArea(int areaType, out CollisionArea collisionArea)
        {
            if (_areaTypeToAreaPoolDic.TryGetValue(areaType, out var pool) &&
                pool.Count > 0)
            {
                collisionArea =  pool.Dequeue();
                return true;
            }

            if (!_areaTypeToAreaClassTypeDic.TryGetValue(areaType, out var classType))
            {
                collisionArea = null;
                Log.LogError($"AreaType:{areaType} ClassType not exist");
                return false;
            }

            collisionArea = Activator.CreateInstance(classType, this, areaType) as CollisionArea;
            return true;
        }

        private void RecycleCollisionArea(CollisionArea collisionArea)
        {
            if (collisionArea == null)
            {
                Log.LogWarning("collisionArea is null, do not recycle CollsionArea");
                return;
            }

            int areaType = collisionArea.AreaType;
            if (!_areaTypeToAreaPoolDic.TryGetValue(areaType, out var pool))
            {
                pool = new Queue<CollisionArea>();
                _areaTypeToAreaPoolDic.Add(areaType, pool);
            }

            collisionArea.DoRecycle();
            pool.Enqueue(collisionArea);
        }

        #endregion
    }
}
