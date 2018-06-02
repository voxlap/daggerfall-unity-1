using DaggerfallWorkshop.Game.UserInterface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CompassDisplay : MonoBehaviour {

    private SteamVR_TrackedObject trackedObj;
    private int counter;
    public GameObject compassQuad;

    [Tooltip("The under-controller UI will display when the bottom of the controller is facing the user. " +
        "In order to accomplish this, a raycast is shot out towards the camera. This variable needs to be set " +
        "to the layer that the VR camera is on.")]
    public string collisionLayerName = "UI";

    private SteamVR_Controller.Device Controller
    {
        get { return SteamVR_Controller.Input((int)trackedObj.index); } 
    }


    private void Awake()
    {
        trackedObj = GetComponent<SteamVR_TrackedObject>();
    }

    void Start ()
    {
        counter = 0;
	}
	
	void Update ()
    {
        counter++;
        if (counter < 10) return;
        counter = 0;

        int layerMask = LayerMask.NameToLayer(collisionLayerName);
        if (layerMask == -1)
        {
            Debug.LogError("Unable to figure out the index of the UI layer for the controller VR UI!");
            return;
        }
        layerMask = 1 << layerMask;
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), out hit, Mathf.Infinity, layerMask))
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.down) * hit.distance, Color.yellow);
            Debug.Log("Hit!");
            Controller.TriggerHapticPulse(2000);
            if (compassQuad)
            {
                RawImage ri = compassQuad.GetComponent<RawImage>();
                if (ri)
                {
                    Debug.Log("Activating compass");
                    ri.enabled = true;
                }
            }
        }
        else
        {
            Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.down) * 1000, Color.white);
            if (compassQuad)
            {
                RawImage ri = compassQuad.GetComponent<RawImage>();
                if (ri)
                {
                    ri.enabled = false;
                }
                
            }
        }
        /*
        Quaternion rot = Controller.transform.rot;
        // Call me insane, but assigning Controller.transform.rot.z to a float results in 0.
        if (Mathf.Abs(rot.z) >= 0.5 && Mathf.Abs(rot.z) <= 0.7) 
        {
            if (compassQuad)
            {
                RawImage ri = compassQuad.GetComponent<RawImage>();
                if (ri)
                {
                    Debug.Log("Activating compass");
                    ri.enabled = true;
                }
            }
        }
        else
        {
            if (compassQuad)
            {
                RawImage ri = compassQuad.GetComponent<RawImage>();
                if (ri)
                {
                    ri.enabled = false;
                }
                
            }
        }
        */
		
	}
}
