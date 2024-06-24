using Logging;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using Utility;
using static UnityEngine.ParticleSystem;

namespace GameMainSystem.Collision
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

            private Dictionary<int, int> _instancIdToId =
                new Dictionary<int, int>();

            public void FillColliders(RegisterColliderData colliderData)
            {
                IdToColliderDic.Clear();
                foreach (var idToCollider in colliderData.IdToRegisterColliderDic)
                {
                    int id = idToCollider.Key;
                    var collider = idToCollider.Value;
                    IdToColliderDic.Add(idToCollider.Key, collider);
                    _instancIdToId.Add(collider.GetInstanceID(), id);
                }
            }

            public bool TryGetColliderId(int instanceId, out int colliderId)
            {
                return _instancIdToId.TryGetValue(instanceId, out colliderId);
            }
        }

        private int _nextGroupId = 0;

        private Dictionary<int, ColliderGroup> _groupIdToColliderGroup =
            new Dictionary<int, ColliderGroup>();

        private Dictionary<int, int> _instancIdToGroupId =
            new Dictionary<int, int>();

        public CollisionAreaManager()
        {
            var collisionAreaTypeList = AttributeUtility.GetAllAtttibuteTypeList<CollisionAreaAttribute>();
        }

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

            if (CheckColliderIsValid(colliderData, out string msg))
            {
                Log.LogWarning($"RegisterColliderData Invalid Msg:\n{msg}");
                return 0;
            }

            int newGroupId = GetNextGroupId();
            AddCollider(newGroupId, colliderData);

            return 0;
        }

        public void UnregisterCollider(int groupId)
        {
            RemoveCollider(groupId);
        }

        #endregion

        #region Public

        public void CreateCollisionArea(ICollisionAreaSetupData setupData)
        {

        }

        #endregion

        #region Imp ICollisionAreaManager

        bool ICollisionAreaManager.TryGetColliderId(int instanceId, out int colliderId)
        {
            colliderId = 0;
            if (!_instancIdToGroupId.TryGetValue(instanceId, out int groupId))
                return false;

            if (!_groupIdToColliderGroup.TryGetValue(groupId, out var group))
                return false;

            return group.TryGetColliderId(instanceId, out colliderId);
        }

        #endregion

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
                if (_instancIdToGroupId.ContainsKey(colliderInsId))
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
            if (_groupIdToColliderGroup.ContainsKey(groupId))
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
                _instancIdToGroupId.Add(collider.GetInstanceID(), groupId);
            }

            _groupIdToColliderGroup.Add(groupId, newColliderGroup);
        }

        private void RemoveCollider(int groupId)
        {
            if (!_groupIdToColliderGroup.TryGetValue(groupId, out var colliderGroup))
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
                _instancIdToGroupId.Remove(collider.GetInstanceID());
            }

            _groupIdToColliderGroup.Remove(groupId);
        }
    }
}
