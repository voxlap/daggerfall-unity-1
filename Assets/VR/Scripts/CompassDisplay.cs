using DaggerfallWorkshop.Game.UserInterface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;
using Valve.VR.InteractionSystem;

[RequireComponent(typeof(UserInterfaceRenderTarget))]
public class CompassDisplay : MonoBehaviour {

    private SteamVR_TrackedObject trackedObj;
    private int counter;

    [Tooltip("The under-controller UI will display when the bottom of the controller is facing the user. " +
        "In order to accomplish this, a raycast is shot out towards the camera. This variable needs to be set " +
        "to the layer that the VR camera is on.")]
    public string collisionLayerName = "UI";

    private UserInterfaceRenderTarget ui;
    private HUDCompass compass;
    private RawImage ri;

    private void Awake()
    {
        trackedObj = GetComponent<SteamVR_TrackedObject>();
    }

    void Start ()
    {
        counter = 0;
        ui = GetComponent<UserInterfaceRenderTarget>();
        ui.CustomHeight = 69;
        ui.CustomHeight = 17;

        compass = new HUDCompass();
        ui.ParentPanel.Components.Add(compass);

        ri = gameObject.GetComponent<RawImage>();
	}
	
	void Update ()
    {
        compass.Scale = ui.ParentPanel.LocalScale;

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
            Player.instance.rightHand.TriggerHapticPulse(2000);
            
            ri.enabled = true;
            Debug.Log("Activating compass");
        }
        else
        {
            //Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.down) * 1000, Color.white);
            ri.enabled = true;
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
