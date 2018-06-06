using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRInjector : MonoBehaviour {

    public GameObject SteamVRPrefab;
    public GameObject CameraRigPrefab;
    public GameObject UnderControllerUIPrefabLeft;
    public GameObject UnderControllerUIPrefabRight;
    public GameObject VRUIManagerPrefab;

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
    private GameObject vruiManager;

    private void Start()
    {
        StartCoroutine(Setup());
    }

    private IEnumerator Setup()
    {
        yield return new WaitForSeconds(1); // the game starts paused. When unpaused, one second after start up it'll inject

        if (!SteamVRPrefab || !CameraRigPrefab || !UnderControllerUIPrefabLeft || !UnderControllerUIPrefabRight || !VRUIManagerPrefab)
        {
            Debug.LogError("Attempted to inject VR, but one or more of the default prefabs aren't set! SteamVRPrefab, CameraRigPrefabLeft/Right, UnderControllerUIPrefab or VRUIManagerPrefab. This error is non-recoverable for VR support.");
            yield return 0;
        }

        player = GameObject.Find("PlayerAdvanced");
        playerMouseLook = player.GetComponentInChildren<PlayerMouseLook>();
        if (!player || !playerMouseLook)
        {
            Debug.LogError("Attempted to inject VR but I wasn't able to find either the PlayerAdvanced or the PlayerMouseLook! This error is non-recoverable for VR support.");
            yield return 0;
        }

        try
        {
            GameObject oldCamera = player.transform.Find("SmoothFollower").gameObject.transform.Find("Camera").gameObject;
            oldCamera.GetComponent<Camera>().enabled = false;
            oldCamera.GetComponent<AudioListener>().enabled = false;
        }
        catch (Exception)
        {
            Debug.LogError("Unable to get the original camera and/or the old AudioListenerer to disable it for VR! If you continue, VR support will most likely be broken.");
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
            GameObject controller = GameObject.Instantiate(UnderControllerUIPrefabLeft);
            controller.transform.parent = controllerLeft.transform;
            controller.transform.localPosition = new Vector3(0, 0, 0);
            controller.GetComponent<UnderHandUIController>().myController = controllerLeft;

            controller = GameObject.Instantiate(UnderControllerUIPrefabRight);
            controller.transform.parent = controllerRight.transform;
            controller.transform.localPosition = new Vector3(0, 0, 0);
            controller.GetComponent<UnderHandUIController>().myController = controllerRight;

        }
        else
        {
            Debug.LogError("Unable to get the two VR controller objects! If you continue the UI for VR controllers will be broken.");
        }

        eyesCamera = GameObject.Find(cameraEyeName);
        if (eyesCamera)
        {
            SphereCollider uiHeadCollider = eyesCamera.AddComponent<SphereCollider>();
            uiHeadCollider.isTrigger = true;
            uiHeadCollider.radius = 0.3f;
        }
        else
        {
            Debug.LogError("Unable to get the newly created 'Camera (eyes)' object! If you continue, the UI for VR controllers will be broken.");
        }

        vruiManager = GameObject.Instantiate(VRUIManagerPrefab);
	}
	
	// Update is called once per frame
	void Update()
    {
		
	}
}
