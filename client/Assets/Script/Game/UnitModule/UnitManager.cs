using AssetModule;
using CollisionModule;
using Extension;
using Logging;
using System.Collections.Generic;
using UnityEngine;
using Utility;

namespace UnitModule
{
    public class UnitManager
    {
        private Vector3 _pooledPosition = new Vector3(5000, 5000, 5000);

        private int _nextUnitId = 0;

        private UnitSetting _unitRootTemplate;

        //Using Unit
        private Dictionary<int, Unit> _idToUnitDic = new Dictionary<int, Unit>();
        private Dictionary<int, Unit> _colliderGroupIdToUnitDic = new Dictionary<int, Unit>();
        private List<Unit> _unitList = new List<Unit>();

        //Pool Unit
        private Dictionary<string, Queue<Unit>> _nameToUnitPool = new Dictionary<string, Queue<Unit>>();
        private Dictionary<string, Queue<UnitAvatarSetting>> _pathToAvatarPool = new Dictionary<string, Queue<UnitAvatarSetting>>();

        private Transform _unitRootTrans;

        public void Init(UnitSetting unitRootTemplate, Transform parent)
        {
            _unitRootTemplate = unitRootTemplate;
            var unitManagerRoot = new GameObject("UnitManager");
            _unitRootTrans = unitManagerRoot.transform;
            _unitRootTrans.SetParent(parent);
            _unitRootTrans.Reset();
        }

        private int AllocUnitId()
        {
            return ++_nextUnitId;
        }

        //public void AddUnit(int id, GameUnit gameUnit)
        //{
        //    if (gameUnit == null)
        //        return;

        //    if (_unitToIdDic.ContainsKey(gameUnit))
        //        return;

        //    var colliderGroupId = CollisionAreaManager.RegisterCollider(gameUnit.UnitData.GetColliderData());
        //    var unitInfo = new UnitInfo
        //    {
        //        Id = id,
        //        GameUnit = gameUnit,
        //        ColliderGroupId = colliderGroupId
        //    };

        //    _idToUnitInfoDic[id] = unitInfo;
        //    _colliderGroupIdToUnitInfoDic[colliderGroupId] = unitInfo;
        //}

        //public void RemoveUnit(int id)
        //{
        //    if (!_idToUnitInfoDic.TryGetValue(id, out var unitInfo))
        //        return;

        //    CollisionAreaManager.UnregisterCollider(unitInfo.ColliderGroupId);
        //    _idToUnitInfoDic.Remove(id);
        //    _colliderGroupIdToUnitInfoDic.Remove(unitInfo.ColliderGroupId);
        //}

        public bool TryGetUnitIdByColliderGroupId(int colliderGroupId, out int unitId)
        {
            unitId = 0;
            if (!_colliderGroupIdToUnitDic.TryGetValue(colliderGroupId, out var unit))
                return false;

            unitId = unit.Id;
            return true;
        }

        public bool AddUnit<T>(string avatarPath, out T unit) where T : Unit, new()
        {
            if (!TryGetFromPool(avatarPath, out unit, out var avatarSetting))
                return false;
            SetupUnit(unit, avatarPath, avatarSetting);
            AddUsingUnit(unit);
            return true;
        }

        public bool RemoveUnit(int id)
        {
            if (!_idToUnitDic.TryGetValue(id, out var unit))
            {
                Log.LogError($"UnitManager.RemoveUnit 不存在此的Unit Id:{id}");
                return false;
            }

            ResetUnit(unit);
            RemoveUsingUnit(unit);
            ReturnToPool(unit);

            unit.Clear();
            return true;
        }

        private void SetupUnit(Unit unit, string avatarPath, UnitAvatarSetting avatarSetting)
        {
            if (unit == null)
                return;

            var unitId = AllocUnitId();
            var colliderGroupId = CollisionAreaManager.RegisterCollider(
                avatarSetting.UnitColliderList.GetColliderData());
            unit.Setup(unitId, colliderGroupId, avatarPath, avatarSetting);
        }

        private void ResetUnit(Unit unit)
        {
            if (unit == null)
                return;

            unit.UnitSetting.RootTransform.position = _pooledPosition;
            //這樣每次可能都會有GC 要把UnitRoot跟Avatar綁一起?
            unit.UnitAvatarSetting.RootTransform.SetParent(_unitRootTrans);
            unit.UnitAvatarSetting.RootTransform.position = _pooledPosition;

            CollisionAreaManager.UnregisterCollider(unit.ColliderGroupId);

            unit.Reset();
        }

        private void AddUsingUnit(Unit unit)
        {
            if (unit == null)
                return;

            _idToUnitDic.Add(unit.Id, unit);
            _colliderGroupIdToUnitDic.Add(unit.ColliderGroupId, unit);
            _unitList.Add(unit);
        }

        private void RemoveUsingUnit(Unit unit)
        {
            if (unit == null)
                return;

            _idToUnitDic.Remove(unit.Id);
            _colliderGroupIdToUnitDic.Remove(unit.ColliderGroupId);
            _unitList.Remove(unit);
        }

        private bool TryGetFromPool<T>(string avatarPath, out T unit, out UnitAvatarSetting avatarSetting) where T : Unit, new()
        {
            unit = null;
            avatarSetting = null;
            if (_pathToAvatarPool.TryGetValue(avatarPath, out var avatarPool) && avatarPool.Count > 0)
            {
                avatarSetting = avatarPool.Dequeue();
            }
            else
            {
                var avatarAsset = AssetSystem.LoadAsset<GameObject>(avatarPath);
                if (avatarAsset == null)
                {
                    Log.LogError($"UnitManager.TryGetFromPool 找不到AvatarAsset Path:{avatarPath}");
                    return false;
                }
                var avatarObject = ObjectUtility.InstantiateWithoutClone(avatarAsset);
                avatarSetting = avatarObject.GetComponent<UnitAvatarSetting>();
                if (avatarSetting == null)
                {
                    Object.Destroy(avatarObject);
                    Log.LogError($"UnitManager.TryGetFromPool Avatar上不存在UnitAvatarRootSetting的Component Path:{avatarPath}");
                    return false;
                }
            }

            var unitTypeName = typeof(T).Name;
            if (_nameToUnitPool.TryGetValue(unitTypeName, out var unitPool) && unitPool.Count > 0)
            {
                unit = unitPool.Dequeue() as T;
            }
            else
            {
                var unitRootSetting = ObjectUtility.InstantiateWithoutClone(_unitRootTemplate, _unitRootTrans);
                unit = new T();
                unit.Init(unitRootSetting);
            }

            return true;
        }

        private void ReturnToPool(Unit unit)
        {
            if (unit == null)
                return;

            if (!_pathToAvatarPool.TryGetValue(unit.AvatarPath, out var avatarPool))
            {
                avatarPool = new Queue<UnitAvatarSetting>();
                _pathToAvatarPool.Add(unit.AvatarPath, avatarPool);
            }
            avatarPool.Enqueue(unit.UnitAvatarSetting);

            var typeName = unit.GetType().Name;
            if (!_nameToUnitPool.TryGetValue(typeName, out var unitPool))
            {
                unitPool = new Queue<Unit>();
                _nameToUnitPool.Add(typeName, unitPool);
            }
            unitPool.Enqueue(unit);
        }
    }
}
