using System;
using System.Collections.Generic;
using UnityEngine;
using static CollisionModule.CollisionAreaDefine;

namespace GameMainModule.Attack
{
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
    public class AttackCastTimelineAsset
    {
        public int Id;

        public List<AttackEffectTrack> AttackEffectTrackList = new List<AttackEffectTrack>();
        public List<AttackCollisionTrack> AttackCollisionTrackList = new List<AttackCollisionTrack>();
    }
}