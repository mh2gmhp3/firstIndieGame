using Extension;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace AnimationModule
{
    public class PlayableClipController
    {
        private struct PlayableFadeTo
        {
            public int MixerId;
            public int MixInputIndex;
            public float FromWeight;
            public float ToWeight;
            public float Duration;
            public float StartTime;
        }

        private PlayableGraph _graph;
        private AnimationPlayableOutput _output;

        private Dictionary<int, AnimationMixerPlayable> _idToAnimationMixerPlayable = new Dictionary<int, AnimationMixerPlayable>();

        private List<PlayableFadeTo> _fadeToList = new List<PlayableFadeTo>();
        private List<int> _processedFadeToIndexList = new List<int>();

        public void Init(string name, Animator animator, DirectorUpdateMode updateMode = DirectorUpdateMode.GameTime)
        {
            _graph = PlayableGraph.Create(name);
            _output = AnimationPlayableOutput.Create(_graph, "AnimationOutput", animator);
            _graph.SetTimeUpdateMode(updateMode);
            _graph.Play();
        }

        public void Update()
        {
            _processedFadeToIndexList.Clear();
            for (int i = 0; i < _fadeToList.Count; i++)
            {
                var fadeTo = _fadeToList[i];
                if (!_idToAnimationMixerPlayable.TryGetValue(fadeTo.MixerId, out var mixer))
                {
                    //找不到直接移除
                    _processedFadeToIndexList.Add(i);
                    continue;
                }
                var elapsed = Time.time - fadeTo.StartTime;
                if (elapsed >= fadeTo.Duration)
                {
                    //時間超過
                    mixer.SetInputWeight(fadeTo.MixInputIndex, fadeTo.ToWeight);
                    _processedFadeToIndexList.Add(i);
                }
                else
                {
                    mixer.SetInputWeight(
                        fadeTo.MixInputIndex,
                        Mathf.Lerp(fadeTo.FromWeight, fadeTo.ToWeight,Mathf.Clamp01(elapsed / fadeTo.Duration)));
                }
            }

            for (int i = _processedFadeToIndexList.Count - 1; i >= 0; i--)
            {
                _fadeToList.RemoveAt(_processedFadeToIndexList[i]);
            }
        }

        public void Evaluate(float deltaTime = 0f)
        {
            _graph.Evaluate(deltaTime);
        }

        public void AddMixer(int mixerId, int inputCount = 0, int connectId = 0, int connectInputIndex = 0)
        {
            if (_idToAnimationMixerPlayable.ContainsKey(mixerId))
                return;

            var mixerPlayable = AnimationMixerPlayable.Create(_graph, inputCount);
            _idToAnimationMixerPlayable[mixerId] = mixerPlayable;
            if (connectId == 0)
            {
                _output.SetSourcePlayable(mixerPlayable);
            }
            else
            {
                if (!_idToAnimationMixerPlayable.TryGetValue(connectId, out var connectTarget))
                    return;

                if (connectTarget.GetInputCount() < connectInputIndex + 1)
                    connectTarget.SetInputCount(connectInputIndex + 1);
                _graph.Connect(mixerPlayable, 0, connectTarget, connectInputIndex);
            }
        }

        public void AddMixerClip(int mixerId, int inputIndex, AnimationClip clip)
        {
            if (clip == null)
                return;

            if (!_idToAnimationMixerPlayable.TryGetValue(mixerId, out var mixer))
                return;

            if (inputIndex + 1 > mixer.GetInputCount())
            {
                mixer.SetInputCount(inputIndex + 1);
            }

            var clipPlayable = AnimationClipPlayable.Create(_graph, clip);
            _graph.Connect(clipPlayable, 0, mixer, inputIndex);
        }

        public void RemoveMixerAllClip(int mixerId)
        {
            if (!_idToAnimationMixerPlayable.TryGetValue(mixerId, out var mixer))
                return;

            var mixerCount = mixer.GetInputCount();
            for (int i = 0; i < mixerCount; i++)
            {
                var playable = mixer.GetInput(i);
                if (!playable.IsValid())
                    return;
                //斷連線
                _graph.Disconnect(mixer, i);
                //刪掉
                playable.Destroy();
            }
            //清空輸入數量
            mixer.SetInputCount(0);
        }

        public void SetMixerWeight(int mixerId, int inputIndex, float weight, float duration = 0f)
        {
            if (!_idToAnimationMixerPlayable.TryGetValue(mixerId, out var mixer))
                return;

            //原本有再跑的直接快進到結束
            RemoveFadeToIfExist(mixerId, inputIndex);
            ////相等 或 立即
            if (duration == 0f)
            {
                //Immediate
                mixer.SetInputWeight(inputIndex, weight);
            }
            else
            {
                var oriWeight = mixer.GetInputWeight(inputIndex);
                if (oriWeight.ApproximateEqual(weight))
                {
                    mixer.SetInputWeight(inputIndex, weight);
                    return;
                }
                _fadeToList.Add(new PlayableFadeTo()
                {
                    MixerId = mixerId,
                    MixInputIndex = inputIndex,
                    FromWeight = oriWeight,
                    ToWeight = weight,
                    Duration = duration,
                    StartTime = Time.time
                });
            }
        }

        public void Play(int mixerId, int inputIndex, float speed = 1)
        {
            if (!_idToAnimationMixerPlayable.TryGetValue(mixerId, out var mixer))
                return;
            for (int i = 0; i < mixer.GetInputCount(); i++)
            {
                RemoveFadeToIfExist(mixerId, i);
                mixer.SetInputWeight(i, 0);
            }
            mixer.SetInputWeight(inputIndex, 1);
            var playable = mixer.GetInput(inputIndex);
            playable.SetTime(0);
            playable.SetSpeed(speed);
        }

        /// <summary>
        /// 移除存在的FadeTo
        /// </summary>
        /// <param name="mixerId"></param>
        /// <param name="inputIndex"></param>
        private void RemoveFadeToIfExist(int mixerId, int inputIndex)
        {
            _processedFadeToIndexList.Clear();
            for (int i = 0; i < _fadeToList.Count; i++)
            {
                var fadeTo = _fadeToList[i];
                if (fadeTo.MixerId == mixerId && fadeTo.MixInputIndex == inputIndex)
                {
                    _processedFadeToIndexList.Add(i);
                }
            }
            for (int i = _processedFadeToIndexList.Count - 1; i >= 0; i--)
            {
                _fadeToList.RemoveAt(_processedFadeToIndexList[i]);
            }
        }
    }
}
