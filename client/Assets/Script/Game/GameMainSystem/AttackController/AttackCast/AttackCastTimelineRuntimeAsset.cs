using System.Collections.Generic;
using static CollisionModule.CollisionAreaDefine;

namespace GameMainModule.Attack
{
    public class AttackEffectRuntimeTrack : ITrackRuntimeAsset
    {
        private AttackEffectTrack _editData;

        public int Id => _editData.Id;

        public AttackEffectRuntimeTrack(AttackEffectTrack editData)
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

    public class AttackCollisionRuntimeTrack : ITrackRuntimeAsset
    {
        private AttackCollisionTrack _editData;

        public int Id => _editData.Id;

        public AreaType CollisionAreaType => _editData.CollisionAreaType;

        public AttackCollisionRuntimeTrack(AttackCollisionTrack editData)
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

    public class AttackCastTimelineRuntimeAsset : ITimelineRuntimeAsset
    {
        private AttackCastTimelineAsset _editData;
        private List<ITrackRuntimeAsset> _trackAssetList = new List<ITrackRuntimeAsset>();
        private float _totalDuration = 0f;

        public List<ITrackRuntimeAsset> TrackAssetList => _trackAssetList;

        public int Id => _editData.Id;

        public AttackCastTimelineRuntimeAsset(AttackCastTimelineAsset editData)
        {
            _editData = editData;

            for (int i = 0; i < editData.AttackEffectTrackList.Count; i++)
            {
                _trackAssetList.Add(new AttackEffectRuntimeTrack(editData.AttackEffectTrackList[i]));
            }
            for (int i = 0; i < editData.AttackCollisionTrackList.Count; i++)
            {
                _trackAssetList.Add(new AttackCollisionRuntimeTrack(editData.AttackCollisionTrackList[i]));
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