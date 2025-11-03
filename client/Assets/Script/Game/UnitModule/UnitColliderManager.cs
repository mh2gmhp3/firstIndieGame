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
        private Dictionary<int, int> _idToColliderGroupId = new Dictionary<int, int>();

        public bool RegisterCollider(int id, List<UnitCollider> collider, out int colliderGroupId)
        {
            colliderGroupId = 0;
            if (_idToColliderGroupId.ContainsKey(id))
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
            _idToColliderGroupId.Add(id, colliderGroupId);
            return true;
        }

        public void UnRegisterCollider(int id)
        {
            if (!_idToColliderGroupId.TryGetValue(id, out int colliderGroupId))
                return;

            CollisionAreaManager.UnregisterCollider(colliderGroupId);
            _idToColliderGroupId.Remove(id);
        }
    }
}
