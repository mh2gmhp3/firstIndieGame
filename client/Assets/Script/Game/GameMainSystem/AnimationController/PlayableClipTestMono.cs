using AnimationModule;
using GameMainModule.Animation;
using UnityEngine;

public class PlayableClipTestMono : MonoBehaviour
{
    public CharacterAnimationSetting AnimationSetting;
    public CharacterPlayableClipController PlayableClipController = new CharacterPlayableClipController();

    public AnimationClip Clip1;
    public AnimationClip Clip2;
    public AnimationClip Clip3;
    public bool Attack;
    [Range(0, 10)]
    public float AttackSpeed;
    public Animator Animator;
    private bool _attacking = false;

    [Range(0, 1)]
    public float Weight = 1.0f;

    public PlayableClipController Controller = new PlayableClipController();

    public void Start()
    {
        PlayableClipController.Init("PlayMono", Animator, AnimationSetting);
    }

    public void Update()
    {
        PlayableClipController.Update();
    }
}
