using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Copy form Unity Document https://docs.unity3d.com/ScriptReference/AnimatorOverrideController.html
// 1. 使用Animator的runtimeAnimatorController建立AnimatorOverrideController
// 2. 使用AnimatorOverrideController.GetOverrides獲取所有Clip
// 3. 設定完所有需要覆蓋的Clip後使用ApplyOverrides設定新的覆蓋Clip
public class AnimationClipOverrides : List<KeyValuePair<AnimationClip, AnimationClip>>
{
    public AnimationClipOverrides(int capacity) : base(capacity) { }

    public AnimationClip this[string name]
    {
        get { return this.Find(x => x.Key.name.Equals(name)).Value; }
        set
        {
            int index = this.FindIndex(x => x.Key.name.Equals(name));
            if (index != -1)
                this[index] = new KeyValuePair<AnimationClip, AnimationClip>(this[index].Key, value);
        }
    }
}
