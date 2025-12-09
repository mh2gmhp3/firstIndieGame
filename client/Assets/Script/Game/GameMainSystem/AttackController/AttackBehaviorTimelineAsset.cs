using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Utility;
using static CollisionModule.CollisionAreaDefine;

namespace GameMainModule.Attack
{
    [Serializable]
    public class AttackClipTrack
    {
        public int Id;
        public float StartTime;

        public AnimationClipIndirectField Clip = new AnimationClipIndirectField();
    }

    [Serializable]
    public class AttackEffectTrack
    {
        public int Id;
        public float StartTime;
        public float Duration;

        public Vector3 Position;
        public Vector3 Rotation;
    }

    [Serializable]
    public class AttackCollisionTrack
    {
        public int Id;
        public float StartTime;
        public float Duration;

        public AreaType CollisionAreaType;

        public Vector3 Position;
        public Vector3 Rotation;
    }

    [Serializable]
    public class AttackBehaviorTimelineAsset
    {
        public int Id;

        public List<AttackClipTrack> AttackClipTrackList = new List<AttackClipTrack>();
        public List<AttackEffectTrack> AttackEffectTrackList = new List<AttackEffectTrack>();
        [FormerlySerializedAs("AttackCollisonTrackList")]
        public List<AttackCollisionTrack> AttackCollisionTrackList = new List<AttackCollisionTrack>();
    }
}
