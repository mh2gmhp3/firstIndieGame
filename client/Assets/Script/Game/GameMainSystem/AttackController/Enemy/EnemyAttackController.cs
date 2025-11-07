using FormModule;
using FormModule.Game.Table;
using GameMainModule.Animation;
using System.Collections.Generic;
using UnityEngine;

namespace GameMainModule.Attack
{
    public class EnemyAttackController
    {
        [SerializeField]
        private List<AttackBehavior> _behaviorList =
            new List<AttackBehavior>();

        private AttackBehavior _curBehavior;

        private EnemyAttackRefSetting _attackRefSetting;
        private CharacterPlayableClipController _playableClipController;

        private float _delayStartTime;
        private bool _delayStart = false;

        public bool IsEnd
        {
            get
            {
                if (_curBehavior == null)
                    return true;

                if (_delayStart)
                    return false;

                return _curBehavior.IsEnd;
            }
        }

        public void Init(EnemyAttackRefSetting attackRefSetting, CharacterPlayableClipController clipController)
        {
            _attackRefSetting = attackRefSetting;
            _playableClipController = clipController;

            //TODO 先直接設定
            var behaviorRowList = new List<AttackBehaviorSettingRow>();
            if (GameMainSystem.AttackBehaviorAssetSetting.TryGetAnimationNameToClipDic(100, out var result))
            {
                _playableClipController.SetAttackClip(result);
            }
            FormSystem.Table.AttackBehaviorSettingTable.GetTypeRowList(100, behaviorRowList);
            for (int i = 0; i < behaviorRowList.Count; i++)
            {
                if (!GameMainSystem.AttackBehaviorAssetSetting.TryGetSetting(behaviorRowList[i].AssetSettingId, out var assetSetting))
                    continue;
                var baseTime = GameMainSystem.AttackBehaviorAssetSetting.GetBehaviorBaseTime(
                    behaviorRowList[i].WeaponType,
                    assetSetting.AnimationStateName);
                _behaviorList.Add(new AttackBehavior(_attackRefSetting, assetSetting, baseTime + 0.2f, true));
            }
        }

        public void Clear()
        {
            _behaviorList.Clear();
            _curBehavior = null;
            _attackRefSetting = null;
            _playableClipController = null;
        }

        public void RandomAttack()
        {
            var index = Random.Range(0, _behaviorList.Count);
            _curBehavior = _behaviorList[index];

            _delayStartTime = Time.time;
            _delayStart = true;
        }

        public void DoUpdate()
        {
            if (_curBehavior == null)
                return;

            if (_delayStart)
            {
                //TODO 先設定0.5秒間格
                if ((Time.time - _delayStartTime) < 0.5f)
                    return;

                _curBehavior.OnStart();
                _playableClipController.Attack(_curBehavior.Name);

                _delayStart = false;
            }

            _curBehavior.OnUpdate();
            if (_curBehavior.IsEnd)
            {
                _curBehavior.OnEnd();
            }
        }
    }
}
