using System;
using UnityEngine;

namespace GameMainModule.Animation
{
    [Serializable]
    public class CharacterAnimationClipSetting
    {
        public AnimationClip Clip;
        public float FadeDuration;
    }

    [CreateAssetMenu(fileName = "CharacterAnimationSetting", menuName = "GameMainModule/Animation/CharacterAnimationSetting")]
    public class CharacterAnimationSetting : ScriptableObject
    {
        public CharacterAnimationClipSetting Idle;

        public CharacterAnimationClipSetting Walk;
        public CharacterAnimationClipSetting Walk_Left;
        public CharacterAnimationClipSetting Walk_Right;

        public CharacterAnimationClipSetting Run;
        public CharacterAnimationClipSetting Run_Left;
        public CharacterAnimationClipSetting Run_Right;

        public CharacterAnimationClipSetting Jump_Start;
        public CharacterAnimationClipSetting Jump_Continue;

        public CharacterAnimationClipSetting Fall_Continue;
        public CharacterAnimationClipSetting Fall_Landing;
    }
}
