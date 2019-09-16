using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRInjector : MonoBehaviour {
    public DaggerfallVRPlayer VRPlayerPrefab;
    public GameObject UnderControllerUIPrefabLeft;
    public GameObject UnderControllerUIPrefabRight;
    public GameObject VRUIManagerPrefab;
    public GameObject OverControllerUIPrefab;
    
    // TODO: Change this value based on user's height?
    [Tooltip("Changes the player's height to this value. Currently this is determined by testing, but maybe it should be scaled based on " +
        "the VR user's actual height.")]
    public float defaultCharacterControllerHeight = 0.8f;

    private DaggerfallVRPlayer vrPlayer;
    private GameObject controllerLeft;
    private GameObject controllerRight;
    private GameObject controllerLeftVirtual; // these GameObjects own the controller{Left,Right} objects, facilitating VR emulation
    private GameObject controllerRightVirtual;// by letting you move the GOs independently of the real tracking
    private Camera oldCamera;
    private Camera headCamera;
    private GameObject vruiManager;
    private GameObject playerObject { get { return GameManager.Instance.PlayerObject; } }
    private PlayerMouseLook playerMouseLook { get { return GameManager.Instance.PlayerMouseLook; } }

    public static bool IsVRDevicePresent { get { return UnityEngine.XR.XRDevice.isPresent; } }
    public bool IsInitialized { get; private set; }
    public static event Action OnInitialized;

    #region Singleton

    public static VRInjector Instance { get; private set; }
    private void SetupSingleton()
    {
        if (!Instance)
            Instance = this;
        else
        {
            Debug.LogError("Second VRInjector singleton has been spawned in the scene. This obviously shouldn't happen.");
        }
    }

    #endregion

    private void Awake()
    {
        SetupSingleton();
        DaggerfallBillboard.OnCreated += DaggerfallBillboard_OnCreated;
    }

    private void Start() {
        StartCoroutine(Setup());
    }

    private void OnDestroy()
    {
        DaggerfallBillboard.OnCreated -= DaggerfallBillboard_OnCreated;
    }

    private IEnumerator Setup() {
        // the game starts paused. When unpaused, it'll inject
        while (GameManager.IsGamePaused)
            yield return new WaitForEndOfFrame();

        if (!IsVRDevicePresent)
        {
            Debug.LogError("There is no VR Device detected. VR obviously won't work, but we will try our best to inject everything using the fallback objects. Why do that and not just" +
                " stop things here and now?\nBecause sometimes it's nice to develop for VR without having a VR device attached.");
        }
        //else
        //    FindObjectOfType<UnityEngine.EventSystems.BaseInputModule>().gameObject.SetActive(false);

        if (!VRPlayerPrefab || !UnderControllerUIPrefabLeft || !UnderControllerUIPrefabRight || !VRUIManagerPrefab || !OverControllerUIPrefab) { 
            Debug.LogError("Attempted to inject VR, but one or more of the default prefabs aren't set! SteamVRPrefab, CameraRigPrefabLeft/Right, UnderControllerUIPrefab, VRUIManagerPrefab, or OverControllerUIPrefab. This error is non-recoverable for VR support.");
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

        //vrPlayer.transform.SetParent(playerObject.transform);
        //vrPlayer.transform.localPosition = Vector3.zero;
        //vrPlayer.transform.localRotation = Quaternion.identity;

        controllerRight = vrPlayer.RightHand.gameObject;
        controllerLeft = vrPlayer.LeftHand.gameObject;

        if (controllerLeft && controllerRight) {
            Instantiate(UnderControllerUIPrefabLeft, controllerLeft.transform);
            Instantiate(UnderControllerUIPrefabRight, controllerRight.transform);

            Instantiate(OverControllerUIPrefab, controllerLeft.transform);
            Instantiate(OverControllerUIPrefab, controllerRight.transform);

        }
        else {
            Debug.LogError("Unable to get the two VR controller objects! If you continue the UI for VR controllers will be broken.");
        }
        Transform headTF = vrPlayer.HeadTF;
        if (headTF && oldCamera) {
            //set up VR camera, and preexisting scripts to reference it
            headCamera = headTF.GetComponent<Camera>();
            GameManager.Instance.MainCamera = headCamera;
            GameManager.Instance.PlayerActivate.rayEmitter = headCamera.gameObject;
            SphereCollider uiHeadCollider = headCamera.gameObject.AddComponent<SphereCollider>();
            uiHeadCollider.isTrigger = true;
            uiHeadCollider.radius = 0.3f;
            //modify player pos so that it's zero'd at current camera position. TODO: make player move to where the head is, always?
            Vector3 modifiedPlayerPos = -headTF.localPosition;
            modifiedPlayerPos.y = 0;
            vrPlayer.transform.localPosition = modifiedPlayerPos;
            //set old camera transform as child of head camera
            oldCamera.transform.SetParent(headTF, false);
            oldCamera.transform.localPosition = Vector3.zero;
            oldCamera.transform.localRotation = Quaternion.identity;

            // If VR isn't possible, then make sure the fallback camera is being rotated by the old camera's mouselook
            if (!IsVRDevicePresent)
            {
                oldCamera.transform.SetParent(headTF.parent, true);
                headTF.SetParent(oldCamera.transform, true);
            }
            else //otherwise disable mouse look.
                playerMouseLook.enabled = false;
        }
        else {
            Debug.LogError("Unable to get Camera object from newly spawned VR Player! If you continue, the VR UI and sprite rotation will be broken.");
        }

        //set up UI
        vruiManager = GameObject.Instantiate(VRUIManagerPrefab);
        BillboardRotationCorrection();

        //spawn equipment
        VREquipmentManager.Instance.Init();

        //Make sure the head position is on top of the character controller
        yield return null;
        vrPlayer.ResetPlayerPosition();

        //done. Set initialized true and trigger event.
        IsInitialized = true;
        if (OnInitialized != null)
            OnInitialized();
    }
    private void BillboardRotationCorrection()
    {
        DaggerfallBillboard[] billboards = GameObject.FindObjectsOfType<DaggerfallBillboard>();
        for (int i = 0; i < billboards.Length; ++i)
            if (!billboards[i].GetComponent<BillboardRotationCorrector>())
                billboards[i].gameObject.AddComponent<BillboardRotationCorrector>();
    }
    private void DaggerfallBillboard_OnCreated(DaggerfallBillboard createdBillboard)
    {
        createdBillboard.gameObject.AddComponent<BillboardRotationCorrector>();
    }
}
