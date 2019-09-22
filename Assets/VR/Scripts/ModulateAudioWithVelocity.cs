using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(AudioSource))]
public class ModulateAudioWithVelocity : MonoBehaviour
{
    public float minVelocity = 3f;
    public float maxVelocity = 30f;
    [Range(0, 1)]
    public float minVolume = .05f;
    [Range(0, 1)]
    public float maxVolume = 1f;

    private Rigidbody rb;
    private AudioSource aud;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        aud = GetComponent<AudioSource>();
    }

    private void FixedUpdate()
    {
        float velocity = rb.velocity.magnitude;
        if (velocity < minVelocity)
            aud.volume = 0;
        else
        {
            aud.volume = Mathf.Lerp(minVolume, maxVolume, Mathf.InverseLerp(minVelocity, maxVelocity, velocity));
        }
    }
}
