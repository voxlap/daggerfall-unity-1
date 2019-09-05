using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ResetRBWhenOutOfBounds : MonoBehaviour
{
    public float boundedRadius = 30f;
    private Rigidbody rb;
    private Vector3 startPos;
    private Quaternion startRot;
    private bool hasStarted = false;
    private IEnumerator Start()
    {
        startPos = transform.position;
        startRot = transform.rotation;
        rb = GetComponent<Rigidbody>();
        hasStarted = true;
        while (true)
        {
            yield return new WaitForSeconds(1f);
            if(Vector3.Distance(transform.position, startPos) > boundedRadius)
            {
                ResetRB();
            }
        }
    }
    public void ResetRB()
    {
        if (!hasStarted)
            return;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.position = startPos;
        transform.rotation = startRot;
    }
}
