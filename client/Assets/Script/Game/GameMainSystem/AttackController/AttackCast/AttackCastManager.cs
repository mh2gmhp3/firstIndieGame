using CollisionModule;
using GameMainModule.VFX;
using GameSystem;
using Logging;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace GameMainModule.Attack
{
    public struct CastAttackInfo
    {
        public float SpeedRate;
        public FakeCharacterTriggerInfo TriggerInfo;
        public CastTransformInfo TransformInfo;
    }

    public struct CastTransformInfo
    {
        public IAttackRefSetting AttackRef;
        public Vector3 CastDirection;
        public Vector3 CastRotation;

        public bool FollowTarget;
        public Transform FollowTrans;
    }

    public class AttackCastManager : IUpdateTarget
    {
        private class CastAttackProcesser : ITimelineListener
        {
            private TimelinePlayController _timelinePlayController = new TimelinePlayController();

            private FakeCharacterTriggerInfo _triggerInfo;

            private AttackCastTimelineRuntimeAsset _asset;

            private IAttackRefSetting _attackRef;
            private Vector3 _castDirection;
            private Vector3 _castRotation;

            private VFXObject _vfxObject = null;

            public bool IsEnd
            {
                get
                {
                    return _timelinePlayController.IsEnd;
                }
            }

            public CastAttackProcesser()
            {
                _timelinePlayController.AddListener(this);
            }

            public void Play(AttackCastTimelineRuntimeAsset asset, CastAttackInfo castAttackInfo)
            {
                _timelinePlayController.Play(asset, castAttackInfo.SpeedRate);
                _triggerInfo = castAttackInfo.TriggerInfo;
                _attackRef = castAttackInfo.TransformInfo.AttackRef;
                _castDirection = castAttackInfo.TransformInfo.CastDirection;
                _castRotation = castAttackInfo.TransformInfo.CastRotation;
            }

            public void OnPlayTrack(ITrackRuntimeAsset trackAsset, float speedRate)
            {
                if (trackAsset is AttackEffectRuntimeTrack effectRuntimeTrack)
                {
                    VFXManager.GetVFX(effectRuntimeTrack.EffectObject, (vfxO) =>
                    {
                        _vfxObject = vfxO;
                        if (_attackRef.TryGetAttackCastPoint(0, out var worldPoint, out var direction ))
                        {
                            _vfxObject.Transform.position = worldPoint;
                            _vfxObject.Transform.rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(_castRotation);
                        }
                    });
                }
                else if (trackAsset is AttackCollisionRuntimeTrack collisionRuntimeTrack)
                {
                    CollisionAreaManager.CreateCollisionArea(
                        GameMainSystem.GetCollisionAreaSetupData(
                            _attackRef,
                            collisionRuntimeTrack,
                            _triggerInfo,
                            speedRate));
                }
            }

            public void OnEndTrack(ITrackRuntimeAsset trackAsset, float speedRate)
            {

            }

            public void Update()
            {
                _timelinePlayController.Evaluate();
            }

            public void Clear()
            {
                _timelinePlayController.Clear();
                _triggerInfo = null;
                _asset = null;
                _attackRef = null;
                _castDirection = Vector3.zero;
                _castRotation = Vector3.zero;

                VFXManager.RecycleVFX(_vfxObject);
            }
        }

        private static AttackCastManager _instance = null;

        private AttackCastAssetSetting _assetSetting;

        private List<CastAttackProcesser> _castingAttackList = new List<CastAttackProcesser>();
        private ObjectPool<CastAttackProcesser> _pool;

        private VFXManager _fxManager = new VFXManager();

        public AttackCastManager(AttackCastAssetSetting assetSetting)
        {
            _instance = this;
            _assetSetting = assetSetting;
            _pool = new ObjectPool<CastAttackProcesser>(GetCastAttackProcesser, actionOnRelease: OnCastAttackProcesserRelease);
        }

        public static void CastAttack(int id, CastAttackInfo info)
        {
            if (_instance == null)
                return;

            _instance.InternalCastAttack(id, info);
        }

        private void InternalCastAttack(int id, CastAttackInfo info)
        {
            if (_assetSetting == null)
            {
                Log.LogError("AttackCastManager.InternalTriggerAttack _assetSetting is null");
                return;
            }

            if (!_assetSetting.TryGetRuntimeAsset(id, out var timelineRuntimeAsset))
            {
                Log.LogWarning($"AttackCastManager.InternalTriggerAttack timelineRuntimeAsset not found Id:{id}");
                return;
            }

            var castProcesser = _pool.Get();
            castProcesser.Play(timelineRuntimeAsset, info);
            _castingAttackList.Add(castProcesser);
        }

        #region Pool

        private CastAttackProcesser GetCastAttackProcesser()
        {
            return new CastAttackProcesser();
        }

        private void OnCastAttackProcesserRelease(CastAttackProcesser castAttackProcesser)
        {
            castAttackProcesser.Clear();
        }

        #endregion

        #region IUpdateTarget

        public void DoUpdate()
        {
            for (int i = _castingAttackList.Count - 1; i >= 0; i--)
            {
                var castingAttack = _castingAttackList[i];
                castingAttack.Update();
                if (castingAttack.IsEnd)
                {
                    _pool.Release(castingAttack);
                    _castingAttackList.RemoveAt(i);
                }
            }
        }

        public void DoFixedUpdate()
        {

        }

        public void DoLateUpdate()
        {

        }

        public void DoOnGUI()
        {

        }

        public void DoDrawGizmos()
        {

        }

        #endregion
    }
}
