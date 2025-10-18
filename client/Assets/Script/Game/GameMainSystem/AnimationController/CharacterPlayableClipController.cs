using AnimationModule;
using System.Collections.Generic;
using UnityEngine;

namespace GameMainModule.Animation
{
    public class CharacterPlayableClipController
    {
        public static class PlayableDefine
        {
            public const int Output_MixerId = 1;

            public const int Normal_MixerId = 2;
            public const int Normal_InputIndex = 0;

            public const int Idle_InputIndex = 0;

            public const int Walk_MixerId = 4;
            public const int Walk_MixerInputIndex = 1;
            public const int Walk_InputIndex = 0;
            public const int Walk_Left_InputIndex = 1;
            public const int Walk_Right_InputIndex = 2;

            public const int Run_MixerId = 5;
            public const int Run_MixerInputIndex = 2;
            public const int Run_InputIndex = 0;
            public const int Run_Left_InputIndex = 1;
            public const int Run_Right_InputIndex = 2;

            public const int Jump_Start_InputIndex = 3;
            public const int Jump_Continue_InputIndex = 4;

            public const int Fall_Continue_InputIndex = 5;
            public const int Fall_Landing_InputIndex = 6;

            public const int Attack_MixerId = 3;
            public const int Attack_InputIndex = 1;
        }

        private PlayableClipController _controller;
        private Dictionary<string, int> _attackNameToInputIndex = new Dictionary<string, int>();
        public CharacterPlayableClipController()
        {
            _controller = new PlayableClipController();
        }

        public void Init(string name, Animator animator, CharacterAnimationSetting setting)
        {
            _controller.Init(name, animator);
            //Output
            _controller.AddMixer(PlayableDefine.Output_MixerId);

            //Static Base
            _controller.AddMixer(
                PlayableDefine.Normal_MixerId,
                connectId: PlayableDefine.Output_MixerId,
                connectInputIndex: PlayableDefine.Normal_InputIndex);
            _controller.AddMixer(
                PlayableDefine.Attack_MixerId,
                connectId: PlayableDefine.Output_MixerId,
                connectInputIndex: PlayableDefine.Attack_InputIndex);

            //Normal
            //Idle
            _controller.AddMixerClip(PlayableDefine.Normal_MixerId, PlayableDefine.Idle_InputIndex, setting.Idle);
            //Walk Mixer
            _controller.AddMixer(
                PlayableDefine.Walk_MixerId,
                connectId: PlayableDefine.Normal_MixerId,
                connectInputIndex: PlayableDefine.Walk_MixerInputIndex);
            _controller.AddMixerClip(PlayableDefine.Walk_MixerId, PlayableDefine.Walk_InputIndex, setting.Walk);
            _controller.AddMixerClip(PlayableDefine.Walk_MixerId, PlayableDefine.Walk_Left_InputIndex, setting.Walk_Left);
            _controller.AddMixerClip(PlayableDefine.Walk_MixerId, PlayableDefine.Walk_Right_InputIndex, setting.Walk_Right);
            //Run Mixer
            _controller.AddMixer(
                PlayableDefine.Run_MixerId,
                connectId: PlayableDefine.Normal_MixerId,
                connectInputIndex: PlayableDefine.Run_MixerInputIndex);
            _controller.AddMixerClip(PlayableDefine.Run_MixerId, PlayableDefine.Run_InputIndex, setting.Run);
            _controller.AddMixerClip(PlayableDefine.Run_MixerId, PlayableDefine.Run_Left_InputIndex, setting.Run_Left);
            _controller.AddMixerClip(PlayableDefine.Run_MixerId, PlayableDefine.Run_Right_InputIndex, setting.Run_Right);
            //Jump Start
            _controller.AddMixerClip(PlayableDefine.Normal_MixerId, PlayableDefine.Jump_Start_InputIndex, setting.Jump_Start);
            //Jump Continue
            _controller.AddMixerClip(PlayableDefine.Normal_MixerId, PlayableDefine.Jump_Continue_InputIndex, setting.Jump_Continue);
            //Fall Continue
            _controller.AddMixerClip(PlayableDefine.Normal_MixerId, PlayableDefine.Fall_Continue_InputIndex, setting.Fall_Continue);
            //Fall Landing
            _controller.AddMixerClip(PlayableDefine.Normal_MixerId, PlayableDefine.Fall_Landing_InputIndex, setting.Fall_Landing);
        }

        public void Update()
        {
            _controller.Update();
        }

        public void SetAttackClip(Dictionary<string, AnimationClip> attackClipDic)
        {
            //清空
            _controller.RemoveMixerAllClip(PlayableDefine.Attack_MixerId);
            _attackNameToInputIndex.Clear();
            //加入新的
            int index = 0;
            foreach (var attackClip in attackClipDic)
            {
                _controller.AddMixerClip(PlayableDefine.Attack_MixerId, index, attackClip.Value);
                _attackNameToInputIndex.Add(attackClip.Key, index);
                index++;
            }
        }

        public void Idle()
        {
            ChangeToNormalImmediate();
            ClearNormalInputImmediate();
            _controller.SetMixerWeight(PlayableDefine.Normal_MixerId, PlayableDefine.Idle_InputIndex, 1, 0.2f);
        }

        public void Walk(float direction)
        {
            ChangeToNormalImmediate();
            ClearNormalInputImmediate();
            //Mixer
            _controller.SetMixerWeight(PlayableDefine.Normal_MixerId, PlayableDefine.Walk_MixerInputIndex, 1, 0.2f);
            //Clip 先直走
            _controller.SetMixerWeight(PlayableDefine.Walk_MixerId, PlayableDefine.Walk_InputIndex, 1);
        }

        public void Run(float direction)
        {
            ChangeToNormalImmediate();
            ClearNormalInputImmediate();
            //Mixer
            _controller.SetMixerWeight(PlayableDefine.Normal_MixerId, PlayableDefine.Run_MixerInputIndex, 1, 0.2f);
            //Clip 先直走
            _controller.SetMixerWeight(PlayableDefine.Run_MixerId, PlayableDefine.Run_InputIndex, 1);
        }

        public void Jump()
        {
            ChangeToNormalImmediate();
            ClearNormalInputImmediate();
            _controller.SetMixerWeight(PlayableDefine.Normal_MixerId, PlayableDefine.Jump_Continue_InputIndex, 1, 0.2f);
        }

        public void Fall()
        {
            ChangeToNormalImmediate();
            ClearNormalInputImmediate();
            _controller.SetMixerWeight(PlayableDefine.Normal_MixerId, PlayableDefine.Fall_Continue_InputIndex, 1, 0.2f);
        }

        public void Landing()
        {
            ChangeToNormalImmediate();
            ClearNormalInputImmediate();
            _controller.Play(PlayableDefine.Normal_MixerId, PlayableDefine.Fall_Landing_InputIndex);
        }

        public void Attack(string attackName, float speed = 1f)
        {
            if (!_attackNameToInputIndex.TryGetValue(attackName, out var index))
                return;
            ChangeToAttackImmediate();
            _controller.Play(PlayableDefine.Attack_MixerId, index, speed);
        }

        private void ChangeToNormalImmediate()
        {
            _controller.SetMixerWeight(PlayableDefine.Output_MixerId, PlayableDefine.Normal_InputIndex, 1);
            _controller.SetMixerWeight(PlayableDefine.Output_MixerId, PlayableDefine.Attack_InputIndex, 0);
        }

        private void ChangeToAttackImmediate()
        {
            _controller.SetMixerWeight(PlayableDefine.Output_MixerId, PlayableDefine.Normal_InputIndex, 0);
            _controller.SetMixerWeight(PlayableDefine.Output_MixerId, PlayableDefine.Attack_InputIndex, 1);
        }

        private void ClearNormalInputImmediate()
        {
            _controller.SetMixerWeight(PlayableDefine.Normal_MixerId, PlayableDefine.Idle_InputIndex, 0, 0.2f);
            _controller.SetMixerWeight(PlayableDefine.Normal_MixerId, PlayableDefine.Walk_MixerInputIndex, 0, 0.2f);
            _controller.SetMixerWeight(PlayableDefine.Normal_MixerId, PlayableDefine.Run_MixerInputIndex, 0, 0.2f);
            _controller.SetMixerWeight(PlayableDefine.Normal_MixerId, PlayableDefine.Jump_Start_InputIndex, 0, 0.2f);
            _controller.SetMixerWeight(PlayableDefine.Normal_MixerId, PlayableDefine.Jump_Continue_InputIndex, 0, 0.2f);
            _controller.SetMixerWeight(PlayableDefine.Normal_MixerId, PlayableDefine.Fall_Continue_InputIndex, 0, 0.2f);
            _controller.SetMixerWeight(PlayableDefine.Normal_MixerId, PlayableDefine.Fall_Landing_InputIndex, 0, 0.2f);
        }
    }
}
