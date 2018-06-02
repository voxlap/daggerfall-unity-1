using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRInjector : MonoBehaviour {

    public GameObject SteamVRPrefab;
    public GameObject CameraRigPrefab;
    public GameObject UnderControllerUIPrefab;

    [Tooltip("This name must match the name in the left controller prefab")]
    public String controllerLeftName = "Controller (left)";

    [Tooltip("This name must match the name in the right controller prefab")]
    public String controllerRightName = "Controller (right)";
    
    [Tooltip("The name assigned by SteamVR when it creates the camera object for the player's eyes")]
    public String cameraEyeName = "Camera (eye)";

    private GameObject player;
    private PlayerMouseLook playerMouseLook;
    private GameObject steamVR;
    private GameObject cameraRig;
    private GameObject controllerLeft;
    private GameObject controllerRight;
    private GameObject oldCamera;
    private GameObject eyesCamera;

    private void Start()
    {
        StartCoroutine(Setup());
    }

    private IEnumerator Setup()
    {
        yield return new WaitForSeconds(1); // the game starts paused. When unpaused, one second after start up it'll inject

        if (!SteamVRPrefab || !CameraRigPrefab)
        {
            Debug.LogError("Attempted to inject VR but either SteamVRPrefab or CameraRigPrefab wasn't set!");
            yield return 0;
        }

        player = GameObject.Find("PlayerAdvanced");
        playerMouseLook = player.GetComponentInChildren<PlayerMouseLook>();
        if (!player || !playerMouseLook)
        {
            Debug.LogError("Attempted to inject VR but I wasn't able to find either the PlayerAdvanced or the PlayerMouseLook!");
            yield return 0;
        }

        try
        {
            GameObject oldCamera = player.transform.Find("SmoothFollower").gameObject.transform.Find("Camera").gameObject;
            oldCamera.GetComponent<Camera>().enabled = false;
            oldCamera.GetComponent<AudioListener>().enabled = false;
        }
        catch (Exception e)
        {
            Debug.LogError("Unable to get the original camera and/or the old AudioListenerer to disable it for VR!");
        }


        steamVR = GameObject.Instantiate(SteamVRPrefab);
        cameraRig = GameObject.Instantiate(CameraRigPrefab);
        cameraRig.transform.SetParent(player.transform);
        cameraRig.transform.position = player.transform.position;
        playerMouseLook.enabled = false;

        controllerRight = cameraRig.transform.Find(controllerRightName).gameObject;
        controllerLeft = cameraRig.transform.Find(controllerLeftName).gameObject;

        if (controllerLeft && controllerRight)
        {
            GameObject.Instantiate(UnderControllerUIPrefab).transform.parent = controllerLeft.transform;
            GameObject.Instantiate(UnderControllerUIPrefab).transform.parent = controllerRight.transform;
        }
        else
        {
            Debug.LogError("Unable to get the two VR controller objects!");
        }

        eyesCamera = GameObject.Find(cameraEyeName);
        if (eyesCamera)
        {
            SphereCollider uiHeadCollider = eyesCamera.AddComponent<SphereCollider>();
            uiHeadCollider.isTrigger = true;
            uiHeadCollider.radius = 0.2f;
        }
        else
        {
            Debug.LogError("Unable to get the newly created 'Camera (eyes)' object!");
        }

	}
	
	// Update is called once per frame
	void Update()
    {
		
	}
}
