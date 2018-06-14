using DaggerfallWorkshop.Game;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/**
 * This component disables the default UI and replaces it with VR logic
 **/
public class VRUIManager : MonoBehaviour {
    public GameObject FloatingUIPrefab;
    private GameObject floatingUI;
    private GameObject eyesCamera;

    [Tooltip("The name assigned by SteamVR when it creates the camera object for the player's eyes")]
    public String CameraEyeName = "Camera (eye)";

    void Start()
    {
        if (FloatingUIPrefab) {
            floatingUI = Instantiate(FloatingUIPrefab);
        } else {
            Debug.Log("The VR UI Manager was unable to create the floating UI! The VR UI will be very broken.");
            return;
        }

        eyesCamera = GameObject.Find(CameraEyeName);
        if (!eyesCamera) {
            Debug.Log("The VR UI Manager was unable to find the Camera (eyes) component with name " + CameraEyeName + ". Improper setting in the injected VRUI Manager prefab? The VR UI will be very broken.");
            return;
        }

        stickFloatingUIInFrontOfPlayer();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
	}
	
	void Update () {
		
	}

    void stickFloatingUIInFrontOfPlayer() {
        if (!floatingUI || !eyesCamera) return;

        floatingUI.gameObject.transform.position = eyesCamera.transform.position;
        floatingUI.gameObject.transform.LookAt(eyesCamera.transform);
        floatingUI.transform.Translate(0, 1f, 2f);
    }
}
