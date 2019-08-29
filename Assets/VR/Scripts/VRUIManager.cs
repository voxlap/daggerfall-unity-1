using DaggerfallWorkshop.Game;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/** * This component disables the default UI and replaces it with VR logic
 **/
public class VRUIManager : MonoBehaviour {
    public GameObject FloatingUIPrefab;
    public GameObject floatingUI;
    public GameObject FollowingUIPrefab;

    private GameObject eyesCamera;
    private Camera actualCamera;
    private GameObject hint;
    private GameObject followingUI;

    [Tooltip("The name assigned by SteamVR when it creates the camera object for the player's eyes")]
    public String CameraEyeName = "Camera (eye)";

    [Tooltip("The name of the layer that the FloatingUI is on.")]
    public String UI_LAYER_MASK_NAME = "UI";

    [Tooltip("The VR Hint is an object that'll be positioned over an object that's being pointed at, to indicate to the user that it can be interacted with")]
    public GameObject HintPrefab;

    [Tooltip("The name in Daggerfall Unity for the PlayerAdvanced GameObject")]
    public String playerAdvancedName = "PlayerAdvanced";

    [Tooltip("This will bet set by the VR Injector")]
    public GameObject playerAdvanced;

    // Used for enabling/disabling the floating UI
    private int cachedMask = 0; 
    private int lastWindowCount = -1;

    void Start()
    {
        if (FloatingUIPrefab) {
            floatingUI = Instantiate(FloatingUIPrefab);
        } else {
            Debug.LogError("The VR UI Manager was unable to create the floating UI! The VR UI will be very broken.");
            return;
        }

        if (FollowingUIPrefab) {
            followingUI = Instantiate(FollowingUIPrefab);
        } else {
            Debug.LogError("The VR UI Manager was unable to create the Following UI! The VR UI will be somewhat broken.");
            return;
        }

        eyesCamera = GameObject.Find(CameraEyeName);
        if (!eyesCamera) {
            Debug.LogError("The VR UI Manager was unable to find the Camera (eyes) component with name " + CameraEyeName + ". Improper setting in the injected VRUI Manager prefab? The VR UI will be very broken.");
            return;
        }

        if (HintPrefab) {
            hint = Instantiate(HintPrefab);
        } else {
            Debug.LogError("The VR UI Manager didn't find a Hint prefab set. Hinting will be broken, but this isn't a huge deal.");
        }


        actualCamera = eyesCamera.GetComponent<Camera>();
        actualCamera.backgroundColor = new Color(.1f, .1f, .1f);
        cachedMask = actualCamera.cullingMask;
        stickFloatingUIInFrontOfPlayer();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        floatingUI.SetActive(false);

        followingUI.GetComponent<FollowingUI>().whatToFollow = eyesCamera;
	}

    public void repositionHint(Vector3 position, Vector3 width_height_depth, Quaternion rotation) {
        if (hint) {
            hint.transform.rotation = rotation;
            //hint.transform.Rotate(0, rotation.eulerAngles.y, 0);
            hint.transform.localScale = width_height_depth;
            hint.transform.position = position;
        }
    }
	
    private bool stuckUI = false;
    private bool lastPauseState = false;
    private int skipFrame = 0;
	void Update () {
        if (skipFrame++ < 30) return;
        skipFrame = 0;

        int currentWindowCount = DaggerfallUI.Instance.UserInterfaceManager.WindowCount;
        if (currentWindowCount > 0 && currentWindowCount > lastWindowCount) {
            // Window count increased--display the UI!
            floatingUI.SetActive(true);
            cachedMask = actualCamera.cullingMask;
            actualCamera.cullingMask = (1 << LayerMask.NameToLayer(UI_LAYER_MASK_NAME));
            stickFloatingUIInFrontOfPlayer();
        } else if (currentWindowCount <= 0 && currentWindowCount < lastWindowCount) { 
            // Window count decreased--disable the UI
            //floatingUI.SetActive(false);
            actualCamera.cullingMask = cachedMask;
        }
        lastWindowCount = currentWindowCount;

            /*
        bool currentPauseState = InputManager.Instance.IsPaused;
        if (lastPauseState != currentPauseState) {
            if (!lastPauseState && currentPauseState) {
                floatingUI.SetActive(true);
                cachedMask = actualCamera.cullingMask;
                actualCamera.cullingMask = (1 << LayerMask.NameToLayer(UI_LAYER_MASK_NAME));
                stickFloatingUIInFrontOfPlayer();
            } else if (lastPauseState && !currentPauseState) {
                //floatingUI.SetActive(false);
                actualCamera.cullingMask = cachedMask;
            }
            lastPauseState = currentPauseState;
        }
        */
	}

    void stickFloatingUIInFrontOfPlayer() {
        if (!floatingUI || !eyesCamera) return;

        floatingUI.transform.position = eyesCamera.transform.position + (eyesCamera.transform.forward * 3f);
        floatingUI.transform.LookAt(eyesCamera.transform);
        floatingUI.transform.Rotate(Vector3.up, 180);
    }
}
