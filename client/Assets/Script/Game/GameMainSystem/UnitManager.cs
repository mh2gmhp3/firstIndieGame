using CollisionModule;
using System.Collections;
using System.Collections.Generic;
using UnitModule;
using UnityEngine;
using Utility;

namespace GameMainModule
{
    public class UnitManager
    {
        private struct UnitInfo
        {
            public int Id;
            public GameUnit GameUnit;
            public int ColliderGroupId;
        }

        private Dictionary<int, UnitInfo> _idToUnitInfoDic = new Dictionary<int, UnitInfo>();
        private Dictionary<int, UnitInfo> _colliderGroupIdToUnitInfoDic = new Dictionary<int, UnitInfo>();
        private Dictionary<GameUnit, int> _unitToIdDic = new Dictionary<GameUnit, int>();

        private int _nextUnitId = 0;

        public int AllocUnitId()
        {
            return ++_nextUnitId;
        }

        public void AddUnit(int id, GameUnit gameUnit)
        {
            if (gameUnit == null)
                return;

            if (_unitToIdDic.ContainsKey(gameUnit))
                return;

            var colliderGroupId = CollisionAreaManager.RegisterCollider(gameUnit.UnitData.GetColliderData());
            var unitInfo = new UnitInfo
            {
                Id = id,
                GameUnit = gameUnit,
                ColliderGroupId = colliderGroupId
            };

            _idToUnitInfoDic[id] = unitInfo;
            _colliderGroupIdToUnitInfoDic[colliderGroupId] = unitInfo;
        }

        public void RemoveUnit(int id)
        {
            if (!_idToUnitInfoDic.TryGetValue(id, out var unitInfo))
                return;

            CollisionAreaManager.UnregisterCollider(unitInfo.ColliderGroupId);
            _idToUnitInfoDic.Remove(id);
            _colliderGroupIdToUnitInfoDic.Remove(unitInfo.ColliderGroupId);
        }

        public bool TryGetUnitIdByColliderGroupId(int colliderGroupId, out int unitId)
        {
            unitId = 0;
            if (!_colliderGroupIdToUnitInfoDic.TryGetValue(colliderGroupId, out var unitInfo))
                return false;

            unitId = unitInfo.Id;
            return true;
        }
    }
}
