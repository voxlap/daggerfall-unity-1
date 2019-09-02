using DaggerfallWorkshop.Game.UserInterface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;
using Valve.VR.InteractionSystem;
using DaggerfallWorkshop.Game;

/**
 * Controls displaying and hiding the UI that attaches underneath a VR controller
 *
 * A ray is cast out from the underside of the controller. When it collides with a collider in the proper layer, the UI appears
 **/
public class UnderHandUIController : MonoBehaviour
{
    [Tooltip("The Canvas that owns the actual UI to display or hide..")]
    public Canvas UIOwner;
    [Tooltip("The center of the UI, from which a dot product is calculated to see if it should be hidden or shown")]
    public Transform uiCenterTF;
    [Tooltip("The maximum view angle (in degrees) for displaying the UI")]
    public float maxAngle = 30f;

    private SteamVR_TrackedObject trackedObj;
    private int counter;

    private Camera mainCamera { get { return GameManager.Instance.MainCamera; } }

    

    private void Awake()
    {
    }

    void Start ()
    {
        counter = 0;
        //UIQuad = Instantiate(UIQuadPrefab);

	}
	
	void Update ()
    {
        counter++;
        if (counter < 10) return;
        counter = 0;

        bool canSee = Vector3.Dot(uiCenterTF.forward, (mainCamera.transform.position - uiCenterTF.position).normalized) < -Mathf.Cos(maxAngle * Mathf.Deg2Rad);
        UIOwner.enabled = canSee;
	}
}
