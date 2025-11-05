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
        private int _nextUnitId = 0;

        //Using Unit
        private Dictionary<int, Unit> _idToUsingUnitDic = new Dictionary<int, Unit>();
        private List<Unit> _unitList = new List<Unit>();

        //Pool Unit
        private Dictionary<string, Queue<Unit>> _nameToUnitPool = new Dictionary<string, Queue<Unit>>();

        private int AllocUnitId()
        {
            return ++_nextUnitId;
        }

        public T AddUnit<T>() where T : Unit, new()
        {
            var unit = GetFromPool<T>();
            SetupUnit(unit);
            AddUsingUnit(unit);
            return unit;
        }

        public void RemoveUnit(int id)
        {
            if (!_idToUsingUnitDic.TryGetValue(id, out var unit))
            {
                Log.LogError($"UnitManager.RemoveUnit 不存在此的Unit Id:{id}");
                return;
            }

            ResetUnit(unit);
            RemoveUsingUnit(unit);
            ReturnToPool(unit);
        }

        public bool TryGetUnit<T>(int id, out T result) where T : Unit
        {
            result = null;
            if (!_idToUsingUnitDic.TryGetValue(id, out var unit))
                return false;

            result = unit as T;
            return result != null;
        }

        private void SetupUnit(Unit unit)
        {
            if (unit == null)
                return;

            var unitId = AllocUnitId();
            unit.Init(unitId);
        }

        private void ResetUnit(Unit unit)
        {
            if (unit == null)
                return;

            unit.Reset();
        }

        private void AddUsingUnit(Unit unit)
        {
            if (unit == null)
                return;

            _idToUsingUnitDic.Add(unit.Id, unit);
            _unitList.Add(unit);
        }

        private void RemoveUsingUnit(Unit unit)
        {
            if (unit == null)
                return;

            _idToUsingUnitDic.Remove(unit.Id);
            _unitList.Remove(unit);
        }

        private T GetFromPool<T>() where T : Unit, new()
        {
            var unitTypeName = typeof(T).Name;
            if (_nameToUnitPool.TryGetValue(unitTypeName, out var unitPool) && unitPool.Count > 0)
                return unitPool.Dequeue() as T;
            else
                return new T();
        }

        private void ReturnToPool(Unit unit)
        {
            if (unit == null)
                return;

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
