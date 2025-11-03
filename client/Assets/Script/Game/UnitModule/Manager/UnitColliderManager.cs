using CollisionModule;
using Logging;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;

namespace UnitModule
{
    public class UnitColliderManager
    {
        private Dictionary<int, int> _unitIdToColliderGroupId = new Dictionary<int, int>();
        private Dictionary<int, int> _colliderGroupIdToUnitId = new Dictionary<int, int>();

        public bool RegisterCollider(int id, List<UnitCollider> collider, out int colliderGroupId)
        {
            colliderGroupId = 0;
            if (_unitIdToColliderGroupId.ContainsKey(id))
            {
                Log.LogWarning($"UnitColliderManager.RegisterAvatar Warning, 此Id已註冊Collider, 如需替換需先UnRegisterCollider Id:{id}");
                return false;
            }

            if (collider == null)
            {
                Log.LogWarning($"UnitColliderManager.RegisterAvatar Warning, Collider為空, Id:{id}");
                return false;
            }

            colliderGroupId = CollisionAreaManager.RegisterCollider(collider.GetColliderData());
            _unitIdToColliderGroupId.Add(id, colliderGroupId);
            _colliderGroupIdToUnitId.Add(colliderGroupId, id);
            return true;
        }

        public void UnRegisterCollider(int id)
        {
            if (!_unitIdToColliderGroupId.TryGetValue(id, out int colliderGroupId))
                return;

            CollisionAreaManager.UnregisterCollider(colliderGroupId);
            _unitIdToColliderGroupId.Remove(id);
            _colliderGroupIdToUnitId.Remove(colliderGroupId);
        }

        public bool TryGetUnitIdByColliderGroupId(int groupId, out int unitId)
        {
            return _colliderGroupIdToUnitId.TryGetValue(groupId, out unitId);
        }
    }
}
