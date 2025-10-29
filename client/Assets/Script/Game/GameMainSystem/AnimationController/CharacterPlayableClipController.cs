using AnimationModule;
using System.Collections.Generic;
using UnityEngine;

namespace GameMainModule.Animation
{
    public class CharacterPlayableClipController
    {
        #region Mixer Define

        public const int Output_MixerId = 1;

        #region Normal

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

        #endregion

        #region Attack

        public const int Attack_MixerId = 3;
        public const int Attack_InputIndex = 1;

        #endregion

        #endregion

        private PlayableClipController _controller;
        private Dictionary<string, int> _attackNameToInputIndex = new Dictionary<string, int>();

        private CharacterAnimationSetting _setting;

        private float _evaluateTime = 0;
        private float _evaluateUpdateInterval = 0.1f;

        public CharacterPlayableClipController()
        {
            _controller = new PlayableClipController();
        }

        public void Init(string name, Animator animator, CharacterAnimationSetting setting)
        {
            _setting = setting;

            _controller.Init(name, animator, UnityEngine.Playables.DirectorUpdateMode.Manual);
            //Output
            _controller.AddMixer(Output_MixerId);

            //Static Base
            _controller.AddMixer(
                Normal_MixerId,
                connectId: Output_MixerId,
                connectInputIndex: Normal_InputIndex);
            _controller.AddMixer(
                Attack_MixerId,
                connectId: Output_MixerId,
                connectInputIndex: Attack_InputIndex);

            //Normal
            //Idle
            _controller.AddMixerClip(Normal_MixerId, Idle_InputIndex, _setting.Idle.Clip);
            //Walk Mixer
            _controller.AddMixer(
                Walk_MixerId,
                connectId: Normal_MixerId,
                connectInputIndex: Walk_MixerInputIndex);
            _controller.AddMixerClip(Walk_MixerId, Walk_InputIndex, _setting.Walk.Clip);
            _controller.AddMixerClip(Walk_MixerId, Walk_Left_InputIndex, _setting.Walk_Left.Clip);
            _controller.AddMixerClip(Walk_MixerId, Walk_Right_InputIndex, _setting.Walk_Right.Clip);
            //Run Mixer
            _controller.AddMixer(
                Run_MixerId,
                connectId: Normal_MixerId,
                connectInputIndex: Run_MixerInputIndex);
            _controller.AddMixerClip(Run_MixerId, Run_InputIndex, _setting.Run.Clip);
            _controller.AddMixerClip(Run_MixerId, Run_Left_InputIndex, _setting.Run_Left.Clip);
            _controller.AddMixerClip(Run_MixerId, Run_Right_InputIndex, _setting.Run_Right.Clip);
            //Jump Start
            _controller.AddMixerClip(Normal_MixerId, Jump_Start_InputIndex, _setting.Jump_Start.Clip);
            //Jump Continue
            _controller.AddMixerClip(Normal_MixerId, Jump_Continue_InputIndex, _setting.Jump_Continue.Clip);
            //Fall Continue
            _controller.AddMixerClip(Normal_MixerId, Fall_Continue_InputIndex, _setting.Fall_Continue.Clip);
            //Fall Landing
            _controller.AddMixerClip(Normal_MixerId, Fall_Landing_InputIndex, _setting.Fall_Landing.Clip);
        }

        public void Update()
        {
            _controller.Update();
            _evaluateTime += Time.deltaTime;
            if (_evaluateTime >= _evaluateUpdateInterval)
            {
                _controller.Evaluate(_evaluateTime);
                _evaluateTime = 0;
            }
        }

        public void SetAttackClip(Dictionary<string, AnimationClip> attackClipDic)
        {
            //清空
            _controller.RemoveMixerAllClip(Attack_MixerId);
            _attackNameToInputIndex.Clear();
            //加入新的
            int index = 0;
            foreach (var attackClip in attackClipDic)
            {
                _controller.AddMixerClip(Attack_MixerId, index, attackClip.Value);
                _attackNameToInputIndex.Add(attackClip.Key, index);
                index++;
            }
        }

        public void Idle()
        {
            ChangeToNormalImmediate();
            ClearNormalInputImmediate();
            _controller.SetMixerWeight(Normal_MixerId, Idle_InputIndex, 1, _setting.Idle.FadeDuration);
        }

        public void Walk(float direction)
        {
            ChangeToNormalImmediate();
            ClearNormalInputImmediate();
            //Mixer
            _controller.SetMixerWeight(Normal_MixerId, Walk_MixerInputIndex, 1, _setting.Walk.FadeDuration);
            //Clip 先直走
            _controller.SetMixerWeight(Walk_MixerId, Walk_InputIndex, 1);
        }

        public void Run(float direction)
        {
            ChangeToNormalImmediate();
            ClearNormalInputImmediate();
            //Mixer
            _controller.SetMixerWeight(Normal_MixerId, Run_MixerInputIndex, 1, _setting.Run.FadeDuration);
            //Clip 先直走
            _controller.SetMixerWeight(Run_MixerId, Run_InputIndex, 1);
        }

        public void Jump()
        {
            ChangeToNormalImmediate();
            ClearNormalInputImmediate();
            _controller.SetMixerWeight(Normal_MixerId, Jump_Continue_InputIndex, 1, _setting.Jump_Continue.FadeDuration);
        }

        public void Fall()
        {
            ChangeToNormalImmediate();
            ClearNormalInputImmediate();
            _controller.SetMixerWeight(Normal_MixerId, Fall_Continue_InputIndex, 1, _setting.Fall_Continue.FadeDuration);
        }

        public void Landing()
        {
            ChangeToNormalImmediate();
            ClearNormalInputImmediate();
            _controller.Play(Normal_MixerId, Fall_Landing_InputIndex);
        }

        public void Attack(string attackName, float speed = 1f)
        {
            if (!_attackNameToInputIndex.TryGetValue(attackName, out var index))
                return;
            ChangeToAttackImmediate();
            _controller.Play(Attack_MixerId, index, speed);
        }

        private void ChangeToNormalImmediate()
        {
            _controller.SetMixerWeight(Output_MixerId, Normal_InputIndex, 1);
            _controller.SetMixerWeight(Output_MixerId, Attack_InputIndex, 0);
        }

        private void ChangeToAttackImmediate()
        {
            _controller.SetMixerWeight(Output_MixerId, Normal_InputIndex, 0);
            _controller.SetMixerWeight(Output_MixerId, Attack_InputIndex, 1);
        }

        private void ClearNormalInputImmediate()
        {
            _controller.SetMixerWeight(Normal_MixerId, Idle_InputIndex, 0, _setting.Idle.FadeDuration);
            _controller.SetMixerWeight(Normal_MixerId, Walk_MixerInputIndex, 0, _setting.Walk.FadeDuration);
            _controller.SetMixerWeight(Normal_MixerId, Run_MixerInputIndex, 0, _setting.Run.FadeDuration);
            _controller.SetMixerWeight(Normal_MixerId, Jump_Start_InputIndex, 0, _setting.Jump_Start.FadeDuration);
            _controller.SetMixerWeight(Normal_MixerId, Jump_Continue_InputIndex, 0, _setting.Jump_Continue.FadeDuration);
            _controller.SetMixerWeight(Normal_MixerId, Fall_Continue_InputIndex, 0, _setting.Fall_Continue.FadeDuration);
            _controller.SetMixerWeight(Normal_MixerId, Fall_Landing_InputIndex, 0, _setting.Fall_Landing.FadeDuration);
        }
    }
}
