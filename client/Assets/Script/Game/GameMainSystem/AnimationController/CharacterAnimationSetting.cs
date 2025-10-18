using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameMainModule.Animation
{
    [CreateAssetMenu(fileName = "CharacterAnimationSetting", menuName = "GameMainModule/Animation/CharacterAnimationSetting")]
    public class CharacterAnimationSetting : ScriptableObject
    {
        public AnimationClip Idle;

        public AnimationClip Walk;
        public AnimationClip Walk_Left;
        public AnimationClip Walk_Right;

        public AnimationClip Run;
        public AnimationClip Run_Left;
        public AnimationClip Run_Right;

        public AnimationClip Jump_Start;
        public AnimationClip Jump_Continue;

        public AnimationClip Fall_Continue;
        public AnimationClip Fall_Landing;
    }
}
