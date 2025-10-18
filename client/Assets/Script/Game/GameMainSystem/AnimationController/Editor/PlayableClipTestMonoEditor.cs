using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PlayableClipTestMono))]
public class PlayableClipTestMonoEditor : Editor
{
    private PlayableClipTestMono _target;
    private PlayableClipTestMono Target
    {
        get
        {
            if (_target == null)
            {
                _target = target as PlayableClipTestMono;
            }
            return _target;
        }
    }
    public override void OnInspectorGUI()
    {
        if (Application.isPlaying)
        {
            EditorGUILayout.BeginVertical();
            if (GUILayout.Button("Idle"))
            {
                Target.PlayableClipController.Idle();
            }
            if (GUILayout.Button("Walk"))
            {
                Target.PlayableClipController.Walk(1);
            }
            if (GUILayout.Button("Run"))
            {
                Target.PlayableClipController.Run(1);
            }
            if (GUILayout.Button("Jump"))
            {
                Target.PlayableClipController.Jump();
            }
            if (GUILayout.Button("Fall"))
            {
                Target.PlayableClipController.Fall();
            }
            if (GUILayout.Button("Landing"))
            {
                Target.PlayableClipController.Landing();
            }
            EditorGUILayout.EndVertical();
        }
        base.OnInspectorGUI();
    }
}