using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleTargetsOnHitWithFlail : MonoBehaviour
{
    public GameObject targets;
    public ResetRBWhenOutOfBounds[] rbResetters = new ResetRBWhenOutOfBounds[0];

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.gameObject.name == "SpikeBall")
        {
            targets.SetActive(!targets.activeSelf);
            if(targets.activeSelf)
            {
                for (int i = 0; i < rbResetters.Length; ++i)
                    rbResetters[i].ResetRB();
            }
        }
    }
}
