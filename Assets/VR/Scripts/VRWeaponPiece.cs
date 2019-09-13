using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class VRWeaponPiece : MonoBehaviour
{
    private VRWeapon myWeapon;
    private bool isInitialized = false;
    public void Init(VRWeapon callback)
    {
        myWeapon = callback;
        isInitialized = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isInitialized)
            myWeapon.TryToHitThing(collision, transform);
    }
}
