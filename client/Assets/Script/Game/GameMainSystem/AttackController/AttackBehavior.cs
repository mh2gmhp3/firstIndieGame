using CollisionModule;
using Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using UnitModule.Movement;
using UnityEngine;

namespace GameMainModule.Attack
{
    /// <summary>
    /// 假的角色攻擊觸發資料
    /// </summary>
    public class FakeCharacterTriggerInfo : ICollisionAreaTriggerInfo
    {
        public bool ToCharacter = false;
        public int Attack = 20;
    }

    [Serializable]
    public class AttackBehavior
    {
        [SerializeField]
        private string _name;

        [SerializeField]
        private float _lockNextBehaviorTime = 0.1f;
        [SerializeField]
        private float _behaviorTime = 0.5f;

        [SerializeField]
        private float _elapsedTime = 0;

        private IAttackRefSetting _attackRefSetting;
        private AttackBehaviorAssetSettingData _setting;
        private FakeCharacterTriggerInfo _fakeCharacterTriggerInfo = new FakeCharacterTriggerInfo();

        private int _collisionAreaId = 0;

        public string Name => _name;

        public bool CanNextBehavior => _elapsedTime >= _lockNextBehaviorTime;

        public bool IsEnd => _elapsedTime > _behaviorTime;

        public AttackBehavior()
        {

        }

        public AttackBehavior(string name, float lockNextBehaviorTime, float behaviorTime)
        {
            _name = name;
            _lockNextBehaviorTime = lockNextBehaviorTime;
            _behaviorTime = behaviorTime;
        }

        /// <summary>
        /// TODO 要加入玩家資料相關內容
        /// </summary>
        /// <param name="setting"></param>
        public AttackBehavior(IAttackRefSetting attackRefSetting, AttackBehaviorAssetSettingData setting, float baseBehaviorTime, bool toCharacter = false)
        {
            _name = setting.AnimationStateName;
            _attackRefSetting = attackRefSetting;
            _setting = setting;
            _behaviorTime = baseBehaviorTime;
            _fakeCharacterTriggerInfo.ToCharacter = toCharacter;
        }

        public void OnStart()
        {
            Reset();

            //每次開始要跟玩家資料和設定重新計算總時間 TODO 先評估是否要影響到動作速度
            _lockNextBehaviorTime = _setting.LockNextTime;
        }

        public void OnUpdate()
        {
            CheckCollision();
            _elapsedTime += Time.deltaTime;
        }

        public void OnEnd()
        {
            //Area自己執行完畢會直接消失 這邊也順便處理如果有外部狀態導致Behavior中指要中斷Area
            CollisionAreaManager.RemoveCollisionArea(_collisionAreaId);
        }

        public void Reset()
        {
            _elapsedTime = 0;
            _collisionAreaId = 0;
        }

        private void CheckCollision()
        {
            if (_collisionAreaId != 0)
                return;

            if (_elapsedTime < _setting.CollisionAreaStartTime)
                return;

            _collisionAreaId = CollisionAreaManager.CreateCollisionArea(GetCollisionAreaSetupData(
                _attackRefSetting,
                _setting,
                _fakeCharacterTriggerInfo));
        }

        private static ICollisionAreaSetupData GetCollisionAreaSetupData(
            IAttackRefSetting attackRefSetting,
            AttackBehaviorAssetSettingData attackBehaviorAssetSettingData,
            ICollisionAreaTriggerInfo triggerInfo)
        {
            switch (attackBehaviorAssetSettingData.CollisionAreaType)
            {
                case CollisionAreaDefine.AreaType.Test:
                    return new TestCollisionAreaSetupData(1); ;
                case CollisionAreaDefine.AreaType.Quad:
                    if (!attackRefSetting.TryGetAttackCastPoint(
                        attackBehaviorAssetSettingData.AttackRefId,
                        out var worldPoint,
                        out var direction))
                        return new TestCollisionAreaSetupData(1); ;
                    return new QuadCollisionAreaSetupData(
                        worldPoint,
                        direction,
                        attackBehaviorAssetSettingData.CollisionAreaDuration,
                        5f,
                        5f,
                        triggerInfo);
            }

            return new TestCollisionAreaSetupData(1);
        }
    }
}
