using AssetModule;
using System.Collections.Generic;
using UnityEngine;

namespace GameMainModule.Attack
{
    public class AttackClipRuntimeTrack : ITrackRuntimeAsset
    {
        private AttackClipTrack _editData;
        private string _name;
        private AnimationClip _clip;

        public int Id => _editData.Id;
        public string Name => _name;
        public AnimationClip Clip => _clip;

        public AttackClipRuntimeTrack(AttackClipTrack editData)
        {
            _editData = editData;
            _clip = AssetSystem.LoadAsset(editData.Clip);
            if (_clip != null)
                _name = _clip.name;
        }

        public float StartTime(float speedRate)
        {
            return _editData.StartTime / speedRate;
        }

        public float Duration(float speedRate)
        {
            if (_clip == null)
                return 0;

            return _clip.length / speedRate;
        }
    }

    public class AttackCastRuntimeTrack : ITrackRuntimeAsset
    {
        private AttackCastTrack _editData;

        public int Id => _editData.Id;

        public int CastId => _editData.CastId;
        public int CastParam => _editData.CastParam;
        public Vector3 CastDirection => _editData.CastDirection;
        public Vector3 CastRotation => _editData.CastRotation;

        public AttackCastRuntimeTrack(AttackCastTrack editData)
        {
            _editData = editData;
        }

        public float StartTime(float speedRate)
        {
            return _editData.StartTime / speedRate;
        }

        public float Duration(float speedRate)
        {
            return _editData.Duration / speedRate;
        }
    }

    public class AttackBehaviorTimelineRuntimeAsset : ITimelineRuntimeAsset
    {
        private AttackBehaviorTimelineAsset _editData;
        private List<ITrackRuntimeAsset> _trackAssetList = new List<ITrackRuntimeAsset>();
        private List<AttackClipRuntimeTrack> _attackClipTrackList = new List<AttackClipRuntimeTrack>();
        private float _totalDuration = 0f;

        public List<ITrackRuntimeAsset> TrackAssetList => _trackAssetList;
        public List<AttackClipRuntimeTrack> AttackClipTrackList => _attackClipTrackList;

        public int Id => _editData.Id;

        public AttackBehaviorTimelineRuntimeAsset(AttackBehaviorTimelineAsset editData)
        {
            _editData = editData;

            for (int i = 0; i < editData.AttackClipTrackList.Count; i++)
            {
                var attackClipTrack = new AttackClipRuntimeTrack(editData.AttackClipTrackList[i]);
                _trackAssetList.Add(attackClipTrack);
                _attackClipTrackList.Add(attackClipTrack);
            }
            for (int i = 0; i < editData.AttackCastTrackList.Count; i++)
            {
                _trackAssetList.Add(new AttackCastRuntimeTrack(editData.AttackCastTrackList[i]));
            }
            _trackAssetList.Sort(
                (l, r) =>
                {
                    if (l == null)
                        return -1;
                    if (r == null)
                        return 1;
                    return l.StartTime(1).CompareTo(r.StartTime(1));
                });
            float totalDuration = 0;
            for (int i = 0; i < _trackAssetList.Count; i++)
            {
                var trackAssetList = _trackAssetList[i];
                var time = trackAssetList.StartTime(1) + trackAssetList.Duration(1);
                if (time > totalDuration)
                    totalDuration = time;
            }
            _totalDuration = totalDuration;
        }

        public float TotalDuration(float speedRate)
        {
            return _totalDuration / speedRate;
        }
    }
}
