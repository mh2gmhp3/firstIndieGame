using CollisionModule;
using FormModule;
using FormModule.Game.Table;
using GameMainModule.Animation;
using System.Collections.Generic;
using UnityEngine;

namespace GameMainModule.Attack
{
    public class EnemyAttackController : ITimelineListener
    {
        [SerializeField]
        private List<AttackBehaviorTimelineRuntimeAsset> _behaviorList =
            new List<AttackBehaviorTimelineRuntimeAsset>();

        private int _curBehaviorIndex = -1;

        private int _id;
        private EnemyAttackRefSetting _attackRefSetting;
        private CharacterPlayableClipController _playableClipController;
        private TimelinePlayController _attackBehaviorTimelinePlayController = new TimelinePlayController();

        private float _delayStartTime;
        private bool _delayStart = false;

        public bool IsEnd
        {
            get
            {
                if (_delayStart)
                    return false;

                return _attackBehaviorTimelinePlayController.IsEnd;
            }
        }

        public void Init(int id, EnemyAttackRefSetting attackRefSetting, CharacterPlayableClipController clipController)
        {
            _id = id;
            _attackRefSetting = attackRefSetting;
            _playableClipController = clipController;
            _attackBehaviorTimelinePlayController.AddListener(this);

            //TODO 先直接設定
            var behaviorRowList = new List<AttackBehaviorSettingRow>();
            FormSystem.Table.AttackBehaviorSettingTable.GetTypeRowList(100, behaviorRowList);
            var nameToClip = new Dictionary<string, AnimationClip>();
            for (int i = 0; i < behaviorRowList.Count; i++)
            {
                if (!GameMainSystem.AttackBehaviorAssetSetting.TryGetRuntimeAsset(behaviorRowList[i].AssetSettingId, out var assetSetting))
                    continue;
                _behaviorList.Add(assetSetting);
                for (int j = 0; j < assetSetting.AttackClipTrackList.Count; j++)
                {
                    var clip = assetSetting.AttackClipTrackList[j].Clip;
                    if (clip == null)
                        continue;
                    if (nameToClip.ContainsKey(clip.name))
                        continue;
                    nameToClip.Add(clip.name, clip);
                }
            }
            _playableClipController.SetAttackClip(nameToClip);
        }

        public void Clear()
        {
            _id = 0;
            _behaviorList.Clear();
            _curBehaviorIndex = -1;
            _attackRefSetting = null;
            _playableClipController = null;
            _attackBehaviorTimelinePlayController.End();
        }

        public void RandomAttack()
        {
            var index = Random.Range(0, _behaviorList.Count);
            _curBehaviorIndex = index;
            _delayStartTime = Time.time;
            _delayStart = true;
        }

        public void DoUpdate()
        {
            if (_delayStart)
            {
                //TODO 先設定0.5秒間格
                if ((Time.time - _delayStartTime) < 0.5f)
                    return;

                _attackBehaviorTimelinePlayController.End();
                _attackBehaviorTimelinePlayController.Play(_behaviorList[_curBehaviorIndex], 1f);

                _delayStart = false;
            }

            _attackBehaviorTimelinePlayController.Evaluate();
        }

        #region ITimeline

        public void OnPlayAsset()
        {

        }

        public void OnEndAsset()
        {

        }

        public void OnPlayTrack(ITrackRuntimeAsset trackAsset, float speedRate)
        {
            if (trackAsset is AttackClipRuntimeTrack attackTrack)
            {
                _playableClipController.Attack(attackTrack.Name);
            }
            else if (trackAsset is AttackCastRuntimeTrack attackCast)
            {
                if (_attackRefSetting.TryGetAttackCastPoint(0, out var worldPoint, out var direction))
                {
                    AttackCastManager.CastAttack(attackCast.CastId,
                        new CastAttackInfo()
                        {
                            SpeedRate = speedRate,
                            CasterUnitId = _id,
                            TransformInfo = new CastTransformInfo()
                            {
                                WorldPoint = worldPoint,
                                Direction = direction,
                                CastDirection = attackCast.CastDirection,
                                CastRotation = attackCast.CastRotation,
                            }
                        });
                }
            }
        }

        public void OnEndTrack(ITrackRuntimeAsset trackAsset, float speedRate)
        {

        }

        #endregion
    }
}
