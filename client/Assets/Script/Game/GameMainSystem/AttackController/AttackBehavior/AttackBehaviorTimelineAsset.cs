using System;
using System.Collections.Generic;
using UnityEngine;
using Utility;

namespace GameMainModule.Attack
{
    [Serializable]
    public class AttackClipTrack
    {
        public int Id;
        public float StartTime;

        public AnimationClipIndirectField Clip = new AnimationClipIndirectField();
    }

    /// <summary>
    /// 釋放攻擊
    /// </summary>
    [Serializable]
    public class AttackCastTrack
    {
        public int Id;
        public float StartTime;
        public float Duration;

        public int CastId;
        public int CastParam;
        public Vector3 CastDirection = Vector3.forward;
        public Vector3 CastRotation;
    }

    [Serializable]
    public class AttackBehaviorTimelineAsset
    {
        public int Id;

        public List<AttackClipTrack> AttackClipTrackList = new List<AttackClipTrack>();
        public List<AttackCastTrack> AttackCastTrackList = new List<AttackCastTrack>();
    }
}
