using System.Collections.Generic;
using UnityEngine;

namespace GameMainModule
{
    public interface ITimelineListener
    {
        public void OnPlayTrack(ITrackRuntimeAsset trackAsset, float speedRate);
        public void OnEndTrack(ITrackRuntimeAsset trackAsset, float speedRate);
    }

    public interface ITrackRuntimeAsset
    {
        public int Id { get; }
        public float StartTime(float speedRate);
        public float Duration(float speedRate);
    }

    public interface ITimelineRuntimeAsset
    {
        public List<ITrackRuntimeAsset> TrackAssetList { get; }
        public float TotalDuration(float speedRate);
    }

    public class TimelinePlayController
    {
        public enum State
        {
            WaitPlay,
            Playing,
            End,
        }

        private struct PlayTrackInfo
        {
            public int Id;
            public float StartTime;
            public float EndTime;
        }

        private float _lastTime;
        private float _elapsedTime;

        private ITimelineRuntimeAsset _asset;
        private float _speedRate;

        private float _duration;

        private State _state = State.End;

        private List<PlayTrackInfo> _waitPlayTrackInfo = new List<PlayTrackInfo>();
        private List<PlayTrackInfo> _playingTrackInfo = new List<PlayTrackInfo>();

        private List<ITimelineListener> _listenerList = new List<ITimelineListener>();

        public bool IsEnd => _state == State.End;

        public bool CanNext => _duration - _elapsedTime < 0.2f || IsEnd;

        public void AddListener(ITimelineListener listener)
        {
            if (listener == null)
                return;

            if (_listenerList.Contains(listener))
                return;

            _listenerList.Add(listener);
        }

        public void RemoveListener(ITimelineListener listener)
        {
            if (listener == null)
                return;

            _listenerList.Remove(listener);
        }

        public void Play(ITimelineRuntimeAsset asset, float speedRate)
        {
            if (asset == null || speedRate <= 0)
                return;

            _state = State.WaitPlay;
            _asset = asset;
            _speedRate = speedRate;
            _duration = _asset.TotalDuration(speedRate);
        }

        public void Evaluate()
        {
            if (_state == State.End)
                return;

            if (_state == State.WaitPlay)
            {
                _lastTime = Time.time;
                _elapsedTime = 0;
                _state = State.Playing;

                for (int i = 0; i < _asset.TrackAssetList.Count; i++)
                {
                    var trackAsset = _asset.TrackAssetList[i];
                    _waitPlayTrackInfo.Add(new PlayTrackInfo()
                    {
                        Id = trackAsset.Id,
                        StartTime = trackAsset.StartTime(_speedRate),
                        EndTime = trackAsset.StartTime(_speedRate) + trackAsset.Duration(_speedRate)
                    });
                }
            }

            _elapsedTime += Time.time - _lastTime;
            _lastTime = Time.time;
            for (int i = _waitPlayTrackInfo.Count - 1; i >= 0; i--)
            {
                var playTrackInfo = _waitPlayTrackInfo[i];
                if (_elapsedTime >= playTrackInfo.StartTime)
                {
                    _playingTrackInfo.Add(playTrackInfo);
                    StartTrack(playTrackInfo.Id);
                    _waitPlayTrackInfo.RemoveAt(i);
                }
            }

            for (int i = _playingTrackInfo.Count - 1; i >= 0; i--)
            {
                var playTrackInfo = _playingTrackInfo[i];
                if (_elapsedTime >= playTrackInfo.EndTime)
                {
                    EndTrack(playTrackInfo.Id);
                    _playingTrackInfo.RemoveAt(i);
                }
            }

            if (_waitPlayTrackInfo.Count == 0 && _playingTrackInfo.Count == 0 && _elapsedTime >= _duration)
                DoEnd();
        }

        public void End()
        {
            DoEnd();
        }

        public void Clear()
        {
            ClearPlayState();
            _asset = null;
            _speedRate = 0;
        }

        private void StartTrack(int id)
        {
            var track = GetTrack(id);
            if (track == null)
                return;

            for (int i = 0; i < _listenerList.Count; i++)
            {
                var listener = _listenerList[i];
                listener.OnPlayTrack(track, _speedRate);
            }
        }

        private void EndTrack(int id)
        {
            var track = GetTrack(id);
            if (track == null)
                return;

            for (int i = 0; i < _listenerList.Count; i++)
            {
                var listener = _listenerList[i];
                listener.OnEndTrack(track, _speedRate);
            }
        }

        private void DoEnd()
        {
            ClearPlayState();
        }

        private void ClearPlayState()
        {
            _lastTime = 0;
            _elapsedTime = 0;
            _waitPlayTrackInfo.Clear();
            _playingTrackInfo.Clear();
            _state = State.End;
        }

        private ITrackRuntimeAsset GetTrack(int id)
        {
            var trackAssetList = _asset.TrackAssetList;
            for (int i = 0; i < trackAssetList.Count; i++)
            {
                if (trackAssetList[i].Id == id)
                    return trackAssetList[i];
            }
            return null;
        }
    }
}
