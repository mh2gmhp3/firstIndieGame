using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameMainModule.Animation
{
    public class AnimatorController
    {
        private Animator _animator;
        private AnimatorTransitionSetting _transitionSetting;
        private AnimatorOverrideController _animatorOverrideController;

        private float _defaultNormalizedTransitionDuration;

        public AnimatorController()
        {

        }

        public void Init(Animator animator, float defaultNormalizedTransitionDuration = 0.1f)
        {
            Init(animator, null, defaultNormalizedTransitionDuration);
        }

        public void Init(Animator animator, AnimatorTransitionSetting transitionSetting, float defaultNormalizedTransitionDuration = 0.1f)
        {
            _animator = animator;
            _transitionSetting = transitionSetting;
            _animatorOverrideController = new AnimatorOverrideController(_animator.runtimeAnimatorController);
            _animator.runtimeAnimatorController = _animatorOverrideController;
            _defaultNormalizedTransitionDuration = defaultNormalizedTransitionDuration;
        }

        public void CrossFade(string stateName)
        {
            if (_transitionSetting == null || !_transitionSetting.TryGet(stateName, out var transitionSettingData))
            {
                var currentStateInfo = _animator.GetCurrentAnimatorStateInfo(0);
                var replay = currentStateInfo.IsName(stateName);
                if (replay)
                    _animator.CrossFade(stateName, _defaultNormalizedTransitionDuration, 0, 0);
                else
                    _animator.CrossFade(stateName, _defaultNormalizedTransitionDuration, 0);
            }
            else
            {
                _animator.CrossFade(
                    stateName,
                    transitionSettingData.NormalizedTransitionDuration,
                    transitionSettingData.Layer,
                    transitionSettingData.NormalizedTimeOffset,
                    transitionSettingData.NormalizedTransitionTime);
            }
        }
    }
}
