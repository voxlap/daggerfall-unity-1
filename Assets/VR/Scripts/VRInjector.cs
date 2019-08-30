using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRInjector : MonoBehaviour {
    public Valve.VR.InteractionSystem.Player VRPlayerPrefab;
    public GameObject UnderControllerUIPrefabLeft;
    public GameObject UnderControllerUIPrefabRight;
    public GameObject VRUIManagerPrefab;
    public GameObject OverControllerUIPrefab;
    
    [Tooltip("The name in Daggerfall Unity for the PlayerAdvanced GameObject")]
    public string playerAdvancedName = "PlayerAdvanced";

    // TODO: Change this value based on user's height?
    [Tooltip("Changes the player's height to this value. Currently this is determined by testing, but maybe it should be scaled based on " +
        "the VR user's actual height.")]
    public float defaultCharacterControllerHeight = 0.8f;

    public static bool IsVRDevicePresent { get { return UnityEngine.XR.XRDevice.isPresent; } }

    private GameObject player;
    private PlayerMouseLook playerMouseLook;
    private Valve.VR.InteractionSystem.Player vrPlayer;
    private GameObject controllerLeft;
    private GameObject controllerRight;
    private GameObject controllerLeftVirtual; // these GameObjects own the controller{Left,Right} objects, facilitating VR emulation
    private GameObject controllerRightVirtual;// by letting you move the GOs independently of the real tracking
    private Camera oldCamera;
    private Camera eyesCamera;
    private GameObject vruiManager;
    private GameObject playerObject { get { return GameManager.Instance.PlayerObject; } }

    private void Start() {
        StartCoroutine(Setup());
    }

    private IEnumerator Setup() {
        yield return new WaitForSeconds(1); // the game starts paused. When unpaused, one second after start up it'll inject

        if (!VRPlayerPrefab || !UnderControllerUIPrefabLeft || !UnderControllerUIPrefabRight || !VRUIManagerPrefab || !OverControllerUIPrefab) { 
            Debug.LogError("Attempted to inject VR, but one or more of the default prefabs aren't set! SteamVRPrefab, CameraRigPrefabLeft/Right, UnderControllerUIPrefab, VRUIManagerPrefab, or OverControllerUIPrefab. This error is non-recoverable for VR support.");
            yield return 0;
        }

        player = GameObject.Find(playerAdvancedName);
        playerMouseLook = player.GetComponentInChildren<PlayerMouseLook>();
        if (!player || !playerMouseLook) {
            Debug.LogError("Attempted to inject VR but I wasn't able to find either the PlayerAdvanced or the PlayerMouseLook! This error is non-recoverable for VR support.");
            yield return 0;
        }

        try {
            oldCamera = GameManager.Instance.MainCamera;
            oldCamera.enabled = false;
            oldCamera.tag = "Untagged";
            oldCamera.GetComponent<AudioListener>().enabled = false;
            Destroy(oldCamera.GetComponent<UnityEngine.PostProcessing.PostProcessingBehaviour>());
        }
        catch (Exception) {
            Debug.LogError("Unable to get the original camera and/or the old AudioListenerer to disable it for VR! If you continue, VR support will most likely be broken.");
        }
        
        vrPlayer = Instantiate(VRPlayerPrefab);
        vrPlayer.transform.SetParent(player.transform);
        vrPlayer.transform.localPosition = Vector3.zero;
        vrPlayer.transform.localRotation = Quaternion.identity;

        controllerRight = vrPlayer.rightHand.gameObject;
        controllerLeft = vrPlayer.leftHand.gameObject;

        if (controllerLeft && controllerRight) {
            GameObject controller = GameObject.Instantiate(UnderControllerUIPrefabLeft);
            controller.transform.SetParent(controllerLeft.transform);
            //controller.transform.parent = controllerLeft.transform;
            controller.transform.localPosition = new Vector3(0, 0, 0);
            controller.GetComponent<UnderHandUIController>().myController = controllerLeft;

            controller = GameObject.Instantiate(UnderControllerUIPrefabRight);
            controller.transform.SetParent(controllerRight.transform);
            //controller.transform.parent = controllerRight.transform;
            controller.transform.localPosition = new Vector3(0, 0, 0);
            controller.GetComponent<UnderHandUIController>().myController = controllerRight;

            controller = GameObject.Instantiate(OverControllerUIPrefab);
            controller.transform.SetParent(controllerLeft.transform);
            //controller.transform.parent = controllerLeft.transform;
            controller.transform.localPosition = new Vector3(0, 0, 0);

            controller = GameObject.Instantiate(OverControllerUIPrefab);
            controller.transform.parent = controllerRight.transform;
            controller.transform.localPosition = new Vector3(0, 0, 0);

        }
        else {
            Debug.LogError("Unable to get the two VR controller objects! If you continue the UI for VR controllers will be broken.");
        }
        Transform eyesTF = vrPlayer.hmdTransforms[IsVRDevicePresent ? 0 : 1];
        if (eyesTF && oldCamera) {
            //set up VR camera, and preexisting scripts to reference it
            eyesCamera = eyesTF.GetComponent<Camera>();
            GameManager.Instance.MainCamera = eyesCamera;
            GameManager.Instance.PlayerActivate.rayEmitter = eyesCamera.gameObject;
            SphereCollider uiHeadCollider = eyesCamera.gameObject.AddComponent<SphereCollider>();
            uiHeadCollider.isTrigger = true;
            uiHeadCollider.radius = 0.3f;
            eyesCamera.transform.localPosition = Vector3.up * defaultCharacterControllerHeight;
            oldCamera.transform.SetParent(eyesTF, false);
            oldCamera.transform.localPosition = Vector3.zero;
            oldCamera.transform.localRotation = Quaternion.identity;

            //If VR isn't possible, then make sure the fallback camera is being rotated by the old camera's mouselook
            if (!IsVRDevicePresent) {
                oldCamera.transform.SetParent(eyesTF.parent, true);
                eyesTF.SetParent(oldCamera.transform, true);
            }
        }
        else {
            Debug.LogError("Unable to get Camera object from newly spawned VR Player! If you continue, the VR UI and sprite rotation will be broken.");
        }

        vruiManager = GameObject.Instantiate(VRUIManagerPrefab);
        
        //set player height to that defined in the injector
        GameManager.Instance.PlayerController.height = defaultCharacterControllerHeight;
    }
	
	void Update() {
		
	}
}
