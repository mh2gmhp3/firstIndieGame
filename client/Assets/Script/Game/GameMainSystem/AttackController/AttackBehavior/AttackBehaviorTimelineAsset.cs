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

    /// <summary>
    /// 移動控制
    /// </summary>
    [Serializable]
    public class AttackMovementTrack
    {
        public int Id;
        public float StartTime;
        public float Duration;

        public AnimationCurve ControlSpeedRateCurve = new AnimationCurve(new Keyframe(0, 1), new Keyframe(1, 1));
    }

    /// <summary>
    /// 動畫位移
    /// </summary>
    [Serializable]
    public class AttackMotionTrack
    {
        public int Id;
        public float StartTime;
        public float Duration;

        public Vector3 StartPosition;
        public Vector3 EndPosition;
        public AnimationCurve Curve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
    }

    [Serializable]
    public class AttackBehaviorTimelineAsset
    {
        public int Id;

        public List<AttackClipTrack> AttackClipTrackList = new List<AttackClipTrack>();
        public List<AttackCastTrack> AttackCastTrackList = new List<AttackCastTrack>();
        public List<AttackMovementTrack> AttackMovementTrackList = new List<AttackMovementTrack>();
        public List<AttackMotionTrack> AttackMotionTrackList = new List<AttackMotionTrack>();
    }
}
