using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class VRWeaponPiece : MonoBehaviour
{
    public VRWeapon Weapon { get { return myWeapon; } }
    [Tooltip("Determines things like whether to make wood sound or metal sound on hit")]
    public bool isMetal = true;
    private VRWeapon myWeapon;
    private Rigidbody rb;
    private bool isInitialized = false;
    private Vector3 lastVelocity;
    private Vector3 lastAngularVelocity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Init(VRWeapon callback)
    {
        myWeapon = callback;
        isInitialized = true;
    }

    private void FixedUpdate()
    {
        lastVelocity = rb.velocity;
        lastAngularVelocity = rb.angularVelocity;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isInitialized && collision.transform != myWeapon.transform)
            myWeapon.TryToHitThing(collision, rb, lastVelocity, lastAngularVelocity);

        float volume = VREquipment.GetVolumeForHit(rb.velocity.sqrMagnitude);
        if(volume > 0)
        {
            if (isMetal)
                VREquipmentManager.Instance.PlayRandomMetalHitSound(collision.contacts[0].point, volume);
            else
                VREquipmentManager.Instance.PlayRandomWoodHitSound(collision.contacts[0].point, volume);
        }
    }
}
