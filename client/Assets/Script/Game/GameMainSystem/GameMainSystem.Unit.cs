using AssetModule;
using System.Collections.Generic;
using UnitModule;
using UnityEngine;

namespace GameMainModule
{
    public partial class GameMainSystem
    {
        private const string UnitRootPath = "Prototype/TestObject/UnitRoot";
        private const string AvatarPath = "Prototype/TestObject/Avatar";

        private UnitManager _unitManager = new UnitManager();
        private UnitAvatarManager _unitAvatarManager = new UnitAvatarManager();
        private UnitColliderManager _unitColliderManager = new UnitColliderManager();

        private EnemyManager _enemyManager;

        public UnitManager UnitManager => _unitManager;
        public UnitAvatarManager UnitAvatarManager => _unitAvatarManager;
        public UnitColliderManager UnitColliderManager => _unitColliderManager;

        public EnemyManager EnemyManager => _enemyManager;

        public void InitUnitManager()
        {
            var unitRootAssets = AssetSystem.LoadAsset<GameObject>(UnitRootPath);
            var unitRoot = unitRootAssets.GetComponent<UnitSetting>();
            _unitAvatarManager.Init(unitRoot, _transform, AvatarPath);
            _enemyManager = new EnemyManager(this);
            RegisterUpdateTarget(_enemyManager);
        }

        #region UnitCollider

        public static bool TryGetUnitIdByColliderGroupId(int groupId, out int unitId)
        {
            return _instance.InternalTryGetUnitIdByColliderGroupId(groupId, out unitId);
        }

        public bool InternalTryGetUnitIdByColliderGroupId(int groupId, out int unitId)
        {
            return _unitColliderManager.TryGetUnitIdByColliderGroupId(groupId, out unitId);
        }

        #endregion

        #region Unit

        public static bool TryGetUnit(int id, out Unit unit)
        {
            return _instance.InternalTryGetUnit(id, out unit);
        }

        private bool InternalTryGetUnit(int id, out Unit unit)
        {
            return _unitManager.TryGetUnit(id, out unit);
        }

        #endregion

        #region Character

        public static bool AddCharacterUnit(string avatarName, out CharacterUnit unit)
        {
            return _instance.InternalAddCharacterUnit(avatarName, out unit);
        }

        private bool InternalAddCharacterUnit(string avatarName, out CharacterUnit unit)
        {
            unit = _unitManager.AddUnit<CharacterUnit>();
            //Character 會永遠顯示 直接註冊所有要使用的資料
            if (!_unitAvatarManager.RegisterAvatar(unit.Id, avatarName, out var avatarInstance))
            {
                _unitManager.RemoveUnit(unit.Id);
                return false;
            }
            unit.SetAvatarInsInfo(avatarInstance);
            _unitColliderManager.RegisterCollider(unit.Id, avatarInstance.UnitAvatarSetting.UnitColliderList, out _);

            return true;
        }

        public static Vector3 GetCharacterPosition()
        {
            return _instance._characterController.Unit.Position;
        }

        #endregion

        #region Npc

        public static bool AddNpcUnit(string avatarName, out NpcUnit unit)
        {
            return _instance.InternalAddNpcUnit(avatarName, out unit);
        }

        private bool InternalAddNpcUnit(string avatarName, out NpcUnit unit)
        {
            unit = _unitManager.AddUnit<NpcUnit>();
            //等系統完全建立起來後在處理分離行為
            if (!_unitAvatarManager.RegisterAvatar(unit.Id, avatarName, out var avatarInstance))
            {
                _unitManager.RemoveUnit(unit.Id);
                return false;
            }
            _unitColliderManager.RegisterCollider(unit.Id, avatarInstance.UnitAvatarSetting.UnitColliderList, out _);

            return true;
        }

        #endregion

        #region Enemy

        public static void AddEnemySpawnPoint(Vector3 position, float radius, List<TestEnemySpawnData> spawnDataList)
        {
            _instance.InternalAddEnemySpawnPoint(position, radius, spawnDataList);
        }

        private void InternalAddEnemySpawnPoint(Vector3 position, float radius, List<TestEnemySpawnData> spawnDataList)
        {
            _enemyManager.AddSpawnPoint(position, radius, spawnDataList);
        }

        public static void TestCauseToEnemyDamage(int id, int damage)
        {
            _instance.InternalTestCauseToEnemyDamage(id, damage);
        }

        private void InternalTestCauseToEnemyDamage(int id, int damage)
        {
            _enemyManager.TestCauseDamage(id, damage);
        }

        public static bool TryGetNearEnemyUnit(Vector3 position, float radius, out int id)
        {
            return _instance.InternalTryGetNearEnemyUnit(position, radius, out id);
        }

        private bool InternalTryGetNearEnemyUnit(Vector3 position, float radius, out int id)
        {
            return _enemyManager.TryGetNearUnit(position, radius, out id);
        }

        #endregion
    }
}
