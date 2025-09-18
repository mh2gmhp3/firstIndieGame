using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnitModule.Movement
{
    [Serializable]
    public class StateMovementSetting
    {
        public int State;

        public float SpeedFactor = 1f;
    }

    [CreateAssetMenu(fileName = "MovementSetting", menuName = "UnitModule/Movement/MovementSetting")]
    public class MovementSetting : ScriptableObject
    {
        public float FixGroundVelocity = 10;

        public float SlopeAngle = 60f;

        public float RotateDurationTimePreAngle = 10f;

        public float LandStiffTime = 0.5f;

        public AnimationCurve JumpVelocityCurve = new AnimationCurve(new Keyframe(0, 10), new Keyframe(0.3f, 5));

        public AnimationCurve FallGravityCurve = new AnimationCurve(new Keyframe(0, 5), new Keyframe(0.5f, 15));
    }
}
