using GameMainModule.Attack;
using Logging;
using System.Collections;
using System.Collections.Generic;
using UnitModule.Movement;
using UnityEngine;
using UnityEngine.UIElements;
using static UnitModule.UnitAvatarManager;

namespace UnitModule
{
    public class EnemyData
    {
        public int SpawnerId;

        public int SettingId;

        public int MaxHp;
        public int CurHp;

        public void Init(int spawnerId, TestEnemySpawnData testEnemySpawnData)
        {
            SpawnerId = spawnerId;

            SettingId = testEnemySpawnData.SettingId;
            MaxHp = testEnemySpawnData.Hp;
            CurHp = testEnemySpawnData.Hp;
        }

        public void Clear()
        {
            SettingId = 0;
            MaxHp = 0;
            CurHp = 0;
        }
    }

    public class EnemyUnit : Unit
    {
        public override int UnitType => (int)UnitDefine.UnitType.Enemy;
        public override Vector3 Position
        {
            get => _unitMovementSetting.RootTransform.position;
        }

        public EnemyData Data { get; } = new EnemyData();

        public bool HaveAvatar { get; private set; } = false;
        private GameUnitMovementSetting _unitMovementSetting;
        private EnemyAttackRefSetting _attackRefSetting;

        public GameUnitMovementSetting UnitMovementSetting => _unitMovementSetting;
        public EnemyAttackRefSetting AttackRefSetting => _attackRefSetting;

        public void SetAvatarInsInfo(UnitAvatarInstance avatarInstance)
        {
            if (avatarInstance == null)
            {
                Log.LogError("CharacterUnit.SetAvatarInsInfo Error, avatarInstance is null");
                return;
            }

            HaveAvatar = true;
            _unitMovementSetting = new GameUnitMovementSetting(avatarInstance);
            _attackRefSetting = new EnemyAttackRefSetting(_unitMovementSetting);
        }

        protected override void DoInit()
        {

        }

        protected override void DoReset()
        {
            Data.Clear();
            HaveAvatar = false;
            _unitMovementSetting = null;
            _attackRefSetting = null;
        }
    }
}
