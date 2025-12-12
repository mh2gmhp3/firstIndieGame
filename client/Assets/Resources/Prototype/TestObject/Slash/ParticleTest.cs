using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class ParticleTest : MonoBehaviour
{
    public ParticleSystem ParticleSystem;
    public bool TestEmit;
    public int EmitCount;
    public float Duration;
    public float SpeedRate;
    public Vector3 Position;
    public Vector3 Rotation;

    // Update is called once per frame
    void Update()
    {
        if (TestEmit)
        {
            Emit();
            TestEmit = false;
        }
    }

    private void Emit()
    {
        if (ParticleSystem == null)
            return;
        ParticleSystem.Play();
    }
}
