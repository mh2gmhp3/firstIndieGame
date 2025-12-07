using GameMainModule;
using GameSystem;
using System;
using System.Collections.Generic;
using UnitModule.Movement;
using UnityEngine;
using Random = System.Random;

namespace UnitModule
{
    public class TestEnemySpawnData
    {
        public int SettingId;

        //TODO 要在表格資料內
        public int Hp;
        public string ModelName;
    }

    public class EnemyManager : IUpdateTarget
    {
        private class Spawner
        {
            public int Id;

            public Vector3 Position;
            public float Radius;
            public List<TestEnemySpawnData> EnemySpawnDataList = new List<TestEnemySpawnData>();

            public List<int> CurEnemyIdList = new List<int>();
            public int MaxEnemyCount = 3;

            public bool NeedSpawn()
            {
                return CurEnemyIdList.Count == 0;
            }
        }

        private int _nextSpawnerId = 0;
        private List<Spawner> _spawnerList = new List<Spawner>();
        private Dictionary<int, Spawner> _idToSpawnerDic = new Dictionary<int, Spawner>();

        private List<int> _allEnemyIdList = new List<int>();
        private Dictionary<int, GameThreeDimensionalEnemyController> _idToEnemyControllerUnitDic = new Dictionary<int, GameThreeDimensionalEnemyController>();
        private List<GameThreeDimensionalEnemyController> _allUnitControllerList = new List<GameThreeDimensionalEnemyController>();
        private List<int> _deadEnemyIdList = new List<int>();

        private GameMainSystem _mainSystem;

        public EnemyManager(GameMainSystem mainSystem)
        {
            _mainSystem = mainSystem;
        }

        private int AllocSpawnerId()
        {
            return ++_nextSpawnerId;
        }

        public void AddSpawnPoint(Vector3 position, float radius, List<TestEnemySpawnData> spawnDataList)
        {
            var spawner = new Spawner
            {
                Id = AllocSpawnerId(),
                Position = position,
                Radius = radius,
            };
            spawner.EnemySpawnDataList.AddRange(spawnDataList);
            _spawnerList.Add(spawner);
            _idToSpawnerDic.Add(spawner.Id, spawner);
        }

        private void Spawn()
        {
            for (int i = 0; i < _spawnerList.Count; ++i)
            {
                Spawn(_spawnerList[i]);
            }
        }

        private void Spawn(Spawner spawner)
        {
            if (spawner == null)
                return;

            if (!spawner.NeedSpawn())
                return;

            var needSpawnCount = spawner.MaxEnemyCount - spawner.CurEnemyIdList.Count;
            if (needSpawnCount <= 0)
                return;

            var random = new Random();
            for (int i = 0; i < needSpawnCount; i++)
            {
                double angle = random.NextDouble() * 2 * Math.PI;
                double distance = Math.Sqrt(random.NextDouble()) * spawner.Radius;
                float x = (float)(distance * Math.Cos(angle));
                float z = (float)(distance * Math.Sin(angle));

                var spawnData = spawner.EnemySpawnDataList[random.Next(spawner.EnemySpawnDataList.Count)];

                if (!AddEnemy(spawner.Id, spawnData, spawner.Position + new Vector3(x, 0, z), out var enemyUnit))
                    continue;

                spawner.CurEnemyIdList.Add(enemyUnit.Id);
                _allEnemyIdList.Add(enemyUnit.Id);
                var controller = new GameThreeDimensionalEnemyController(this);
                controller.Init(enemyUnit);
                _idToEnemyControllerUnitDic.Add(enemyUnit.Id, controller);
                _allUnitControllerList.Add(controller);
            }
        }

        private bool AddEnemy(int spawnerId, TestEnemySpawnData spawnData, Vector3 position, out EnemyUnit enemyUnit)
        {
            enemyUnit = _mainSystem.UnitManager.AddUnit<EnemyUnit>();
            //先直接註冊Avatar
            if (!_mainSystem.UnitAvatarManager.RegisterAvatar(enemyUnit.Id, spawnData.ModelName, out var unitAvatarInstance))
            {
                _mainSystem.UnitManager.RemoveUnit(enemyUnit.Id);
                return false;
            }
            _mainSystem.UnitColliderManager.RegisterCollider(enemyUnit.Id, unitAvatarInstance.UnitAvatarSetting.UnitColliderList, out _);
            unitAvatarInstance.UnitSetting.RootTransform.position = position;
            enemyUnit.SetAvatarInsInfo(unitAvatarInstance);
            enemyUnit.Data.Init(spawnerId, spawnData);
            return true;
        }

        public void TestCauseDamage(int id, int damage)
        {
            if (!_idToEnemyControllerUnitDic.TryGetValue(id, out var controller))
                return;

            controller.Data.CurHp -= damage;
            controller.Data.CurHp = Math.Clamp(controller.Data.CurHp, 0, controller.Data.MaxHp);
            if (controller.Data.CurHp == 0)
            {
                _deadEnemyIdList.Add(id);
            }
        }

        private void UpdateEnemyController()
        {
            for (int i = 0; i < _allUnitControllerList.Count; i++)
            {
                _allUnitControllerList[i].DoUpdate();
            }
        }

        private void FixedUpdateEnemyController()
        {
            for (int i = 0; i < _allUnitControllerList.Count; i++)
            {
                _allUnitControllerList[i].DoFixedUpdate();
            }
        }

        private void DrawGizmosEnemyController()
        {
            for (int i = 0; i < _allUnitControllerList.Count; i++)
            {
                _allUnitControllerList[i].DoDrawGizmos();
            }
        }

        private void DeadCheck()
        {
            if (_deadEnemyIdList.Count == 0)
                return;

            for (int i = 0; i < _deadEnemyIdList.Count; i++)
            {
                var id = _deadEnemyIdList[i];
                if (!_idToEnemyControllerUnitDic.TryGetValue(id, out var controller))
                    continue;
                if (!_idToSpawnerDic.TryGetValue(controller.Data.SpawnerId, out var spawner))
                    continue;

                //移除這個系統裡的
                spawner.CurEnemyIdList.Remove(id);
                _allEnemyIdList.Remove(id);
                _idToEnemyControllerUnitDic.Remove(id);
                _allUnitControllerList.Remove(controller);

                //移除中央管理的
                _mainSystem.UnitColliderManager.UnRegisterCollider(id);
                _mainSystem.UnitAvatarManager.UnregisterAvatar(id);
                _mainSystem.UnitManager.RemoveUnit(id);
            }

            _deadEnemyIdList.Clear();
        }

        public void AddDead(int id)
        {
            if (!_idToEnemyControllerUnitDic.ContainsKey(id))
                return;

            _deadEnemyIdList.Add(id);
        }

        public bool TryGetNearUnit(Vector3 position, float radius, out int id)
        {
            //TODO 之後要考慮Chunk來做搜尋
            id = 0;
            var nearDistanceSqrt = float.MaxValue;
            var maxDistanceSqrt = radius * radius;
            for (int i = 0; i < _allUnitControllerList.Count; i++)
            {
                var unitController = _allUnitControllerList[i];
                var unitPosition = unitController.Unit.Position;
                var unitDistanceSqrt = (position - unitPosition).sqrMagnitude;
                if (unitDistanceSqrt > maxDistanceSqrt)
                    continue;
                if (nearDistanceSqrt > unitDistanceSqrt)
                {
                    nearDistanceSqrt = unitDistanceSqrt;
                    id = unitController.Unit.Id;
                }
            }

            return id > 0;
        }

        #region IUpdateTarget

        void IUpdateTarget.DoUpdate()
        {
            Spawn();
            UpdateEnemyController();
            DeadCheck();
        }

        void IUpdateTarget.DoFixedUpdate()
        {
            FixedUpdateEnemyController();
        }

        void IUpdateTarget.DoLateUpdate()
        {

        }

        void IUpdateTarget.DoOnGUI()
        {

        }

        void IUpdateTarget.DoDrawGizmos()
        {
            DrawGizmosEnemyController();
        }

        #endregion
    }
}
