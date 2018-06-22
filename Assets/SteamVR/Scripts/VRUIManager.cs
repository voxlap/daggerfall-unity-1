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
    private GameObject floatingUI;
    private GameObject eyesCamera;
    private Camera actualCamera;

    [Tooltip("The name assigned by SteamVR when it creates the camera object for the player's eyes")]
    public String CameraEyeName = "Camera (eye)";

    [Tooltip("The name of the layer that the FloatingUI is on.")]
    public String UI_LAYER_MASK_NAME = "UI";

    void Start()
    {
        if (FloatingUIPrefab) {
            floatingUI = Instantiate(FloatingUIPrefab);
        } else {
            Debug.LogError("The VR UI Manager was unable to create the floating UI! The VR UI will be very broken.");
            return;
        }

        eyesCamera = GameObject.Find(CameraEyeName);
        if (!eyesCamera) {
            Debug.LogError("The VR UI Manager was unable to find the Camera (eyes) component with name " + CameraEyeName + ". Improper setting in the injected VRUI Manager prefab? The VR UI will be very broken.");
            return;
        }

        actualCamera = eyesCamera.GetComponent<Camera>();
        actualCamera.backgroundColor = new Color(.1f, .1f, .1f);
        stickFloatingUIInFrontOfPlayer();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        floatingUI.SetActive(false);
	}
	
    private bool stuckUI = false;
    private bool lastPauseState = false;
    private int skipFrame = 0;
    private int cachedMask = 0;
	void Update () {
        if (skipFrame++ < 30) return;
        skipFrame = 0;

        bool currentPauseState = InputManager.Instance.IsPaused;
        if (lastPauseState != currentPauseState) {
            if (!lastPauseState && currentPauseState) {
                floatingUI.SetActive(true);
                cachedMask = actualCamera.cullingMask;
                actualCamera.cullingMask = (1 << LayerMask.NameToLayer(UI_LAYER_MASK_NAME));
                stickFloatingUIInFrontOfPlayer();
            } else if (lastPauseState && !currentPauseState) {
                floatingUI.SetActive(false);
                actualCamera.cullingMask = cachedMask;
            }
            lastPauseState = currentPauseState;
        }
	}

    void stickFloatingUIInFrontOfPlayer() {
        if (!floatingUI || !eyesCamera) return;

        floatingUI.transform.position = eyesCamera.transform.position + (eyesCamera.transform.forward * 3f);
        floatingUI.transform.LookAt(eyesCamera.transform);
        floatingUI.transform.Rotate(Vector3.up, 180);
    }
}
