using System;
using UnityEngine;

namespace GameMainModule.Animation
{
    [Serializable]
    public class CharacterAnimationClipSetting
    {
        public AnimationClip Clip;
        //不區分To跟Out避免設定時間 有Out小於To導致有一瞬間變成預設的狀態
        public float FadeDuration;
    }

    [CreateAssetMenu(fileName = "CharacterAnimationSetting", menuName = "GameMainModule/Animation/CharacterAnimationSetting")]
    public class CharacterAnimationSetting : ScriptableObject
    {
        public CharacterAnimationClipSetting Idle;

        public CharacterAnimationClipSetting Walk;
        public CharacterAnimationClipSetting Walk_Left;
        public CharacterAnimationClipSetting Walk_Right;

        public CharacterAnimationClipSetting Trot;
        public CharacterAnimationClipSetting Trot_Left;
        public CharacterAnimationClipSetting Trot_Right;

        public CharacterAnimationClipSetting Run;
        public CharacterAnimationClipSetting Run_Left;
        public CharacterAnimationClipSetting Run_Right;

        public CharacterAnimationClipSetting Jump_Start;
        public CharacterAnimationClipSetting Jump_Continue;

        public CharacterAnimationClipSetting Falling;
        public CharacterAnimationClipSetting Landing;

        public CharacterAnimationClipSetting Dash;

        public float NormalAndAttackMixDuration = 0.2f;
    }
}
