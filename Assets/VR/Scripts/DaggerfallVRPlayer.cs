// Project:         Daggerfall Tools For Unity
// Copyright:       Copyright (C) 2009-2019 Daggerfall Workshop
// Web Site:        http://www.dfworkshop.net
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/Interkarma/daggerfall-unity
// Original Author: Vincent Wing (vincentwing00@gmail.com)
// Contributors:    
// 
// Notes:
//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Serialization;

public class DaggerfallVRPlayer : MonoBehaviour
{
    public static Player SteamVRPlayer { get { return Player.instance; } }
    
    public Transform HeadTF { get { return SteamVRPlayer.hmdTransforms[VRInjector.IsVRDevicePresent ? 0 : 1]; } }
    public Hand RightHand { get { return SteamVRPlayer.rightHand; } }
    public Hand LeftHand { get { return SteamVRPlayer.leftHand; } }

    [Tooltip("The layermask used for the ray that goes downward from the player controller. Sets the VR Player's position based on what it hits.")]
    public LayerMask positionMask = -1;

    private bool wasPaused = false;
    private Vector3 offsetPosition = Vector3.zero;
    private Quaternion offsetRotation;

    private GameObject playerObject { get { return GameManager.Instance.PlayerObject; } }
    private Camera mainCamera { get { return GameManager.Instance.MainCamera; } }
    private CharacterController playerController { get { return GameManager.Instance.PlayerController; } }
    private PlayerMotor playerMotor { get { return GameManager.Instance.PlayerMotor; } }
    private Transform trackingOriginTransform { get { return Player.instance.trackingOriginTransform; } }

    #region Singleton

    public static DaggerfallVRPlayer Instance { get; private set; }
    private void SetupSingleton()
    {
        if (!Instance)
            Instance = this;
        else
        {
            Debug.LogError("Second DaggerfallVRPlayer singleton has been spawned in the scene. This obviously shouldn't happen.");
        }
    }

    #endregion

    private void Awake()
    {
        SetupSingleton();
    }

    private void Start()
    {
        VRInputActions.OpenMenuAction.onStateDown += OpenMenu_onStateDown;
        VRInputActions.ResetPosition.onStateDown += ResetPosition_onStateDown;
        SaveLoadManager.OnStartLoad += SaveLoadManager_OnStartLoad;
        SaveLoadManager.OnLoad += SaveLoadManager_OnLoad;
        
    }

    private void OnDestroy()
    {
        VRInputActions.OpenMenuAction.onStateDown -= OpenMenu_onStateDown;
        VRInputActions.ResetPosition.onStateDown -= ResetPosition_onStateDown;
        SaveLoadManager.OnStartLoad -= SaveLoadManager_OnStartLoad;
        SaveLoadManager.OnLoad -= SaveLoadManager_OnLoad;
    }

    private void Update()
    {
        UpdatePositionAndRotation();
        UpdateTagsOnPause();
    }

    public void FadeIn(float duration = 0, float delay = 0)
    {
        if (delay > 0)
            StartCoroutine(FadeInCoroutine(duration, delay));
        else
        {
            SteamVR_Fade.Start(Color.clear, duration);
        }
    }
    public void FadeOut(float duration = 0, float delay = 0)
    {
        if (delay > 0)
            StartCoroutine(FadeOutCoroutine(duration, delay));
        else
        {
            SteamVR_Fade.Start(Color.black, duration);
        }
    }

    private IEnumerator FadeInCoroutine(float duration, float delay)
    {
        yield return new WaitForSeconds(delay);
        FadeIn(duration, 0);
    }
    private IEnumerator FadeOutCoroutine(float duration, float delay)
    {
        yield return new WaitForSeconds(delay);
        FadeOut(duration, 0);
    }

    public void ResetPlayerPosition()
    {
        //get the new offset position
        Vector3 newOffsetPosition = -transform.InverseTransformPoint(HeadTF.position);
        newOffsetPosition.y = 0;
        offsetPosition = newOffsetPosition;
    }

    /// <summary>
    /// Add rotation to player so that the VR camera stays in the same position.
    /// </summary>
    /// <param name="yaw"></param>
    public void Rotate(float yaw)
    {
        trackingOriginTransform.RotateAround(mainCamera.transform.position, Vector3.up, yaw);
        playerObject.transform.rotation = trackingOriginTransform.rotation;
    }
    /// <summary>
    /// Set rotation of player so that the VR camera stays in the same position.
    /// </summary>
    /// <param name="yaw"></param>
    public void SetRotation(float yaw)
    {
        Rotate(yaw - trackingOriginTransform.eulerAngles.y);
    }

    private void UpdatePositionAndRotation()
    {
        transform.position = playerObject.transform.position + Vector3.down * (playerController.height / 2f) + offsetPosition;
        //transform.rotation = playerObject.transform.rotation;
        playerObject.transform.rotation = trackingOriginTransform.rotation;
    }

    private void UpdateTagsOnPause()
    {
        if (!wasPaused && InputManager.Instance.IsPaused)
            SetLayerRecursively(transform, LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("VREquipment"));
        wasPaused = InputManager.Instance.IsPaused;
    }

    private void OpenMenu_onStateDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        Debug.Log("SteamVR input action detected: OpenMenu");
        if (VRUIManager.Instance.IsOpen)
            VRUIManager.Instance.CloseAllDaggerfallWindows();
        else
            InputManager.Instance.AddAction(InputManager.Actions.CharacterSheet);
    }

    private void ResetPosition_onStateDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        Debug.Log("SteamVR input action detected: ResetPosition");
        ResetPlayerPosition();
    }

    private void SaveLoadManager_OnLoad(SaveData_v1 saveData)
    {
        //fade from black
        FadeIn(0f, 2f);
        //rotate player to the saved rotation
        SetRotation(saveData.playerData.playerPosition.yaw);
    }

    private void SaveLoadManager_OnStartLoad(SaveData_v1 saveData)
    {
        FadeOut();
    }

    //TODO: This would go well in a general utilities class.
    /// <summary>
    /// Sets the layer of a transform and all of its children (and subchildren) to be the given layer.
    /// </summary>
    public static void SetLayerRecursively(Transform tf, int layer, int ignoreLayer = -1)
    {
        if(tf.gameObject.layer != ignoreLayer)
            tf.gameObject.layer = layer;
        for (int i = 0; i < tf.childCount; ++i)
        {
            SetLayerRecursively(tf.GetChild(i), layer, ignoreLayer);
        }
    }

    /* possibly obsolete
    

    /// <summary>
    /// control player height based on head tf position.This seemed like a good idea, would work well
    /// for crushing the player accurately and possibly other gameplay mechanics, but it leads to the
    /// view being jankily stuttered up and down whenever the player moves their head vertically.
    /// 
    /// TODO: make this work
    /// </summary>
    private void UpdatePlayerHeight()
    {
        playerController.height = mainCamera.transform.localPosition.y;
        if (GameManager.Instance.PlayerMotor.IsGrounded)
        {
            RaycastHit hit;
            if (Physics.Raycast(playerController.transform.position, Vector3.down, out hit, playerController.height, positionMask))
                transform.position = hit.point;
        }
    }

    */
}
