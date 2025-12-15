using CollisionModule;
using GameMainModule.VFX;
using GameSystem;
using Logging;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace GameMainModule.Attack
{
    public class AttackCastTriggerInfo : ICollisionAreaTriggerInfo
    {
        public int CasterUnitId;
        public int OnHitCastId;

        public void Update(CastAttackInfo attackInfo)
        {
            CasterUnitId = attackInfo.CasterUnitId;
        }
    }

    public struct CastAttackInfo
    {
        public float SpeedRate;
        public int CasterUnitId;
        public CastTransformInfo TransformInfo;
    }

    public struct CastTransformInfo
    {
        public Vector3 WorldPoint;
        public Vector3 Direction;
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
            private AttackCastTriggerInfo _attackCastTriggerInfo = new AttackCastTriggerInfo();

            private AttackCastTimelineRuntimeAsset _asset;

            private CastAttackInfo _attackInfo;

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
                _asset = asset;
                _timelinePlayController.Play(asset, castAttackInfo.SpeedRate);
                _attackInfo = castAttackInfo;
                _attackCastTriggerInfo.Update(_attackInfo);
            }

            public void OnPlayTrack(ITrackRuntimeAsset trackAsset, float speedRate)
            {
                if (trackAsset is AttackEffectRuntimeTrack effectRuntimeTrack)
                {
                    VFXManager.GetVFX(effectRuntimeTrack.EffectObject, (vfxO) =>
                    {
                        _vfxObject = vfxO;
                        _vfxObject.Transform.position = _attackInfo.TransformInfo.WorldPoint;
                        _vfxObject.Transform.rotation = Quaternion.LookRotation(_attackInfo.TransformInfo.Direction) * Quaternion.Euler(_attackInfo.TransformInfo.CastRotation);
                        _vfxObject.Play();
                    });
                }
                else if (trackAsset is AttackCollisionRuntimeTrack collisionRuntimeTrack)
                {
                    _attackCastTriggerInfo.OnHitCastId = collisionRuntimeTrack.OnHitCastId;
                    CollisionAreaManager.CreateCollisionArea(
                        GameMainSystem.GetCollisionAreaSetupData(
                            _attackInfo.TransformInfo.WorldPoint,
                            _attackInfo.TransformInfo.Direction,
                            collisionRuntimeTrack,
                            _attackCastTriggerInfo,
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
                _asset = null;
                _attackInfo = default;

                VFXManager.RecycleVFX(_vfxObject);
                _vfxObject = null;
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
