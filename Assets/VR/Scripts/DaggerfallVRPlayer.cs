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

public class DaggerfallVRPlayer : MonoBehaviour
{
    public static Player SteamVRPlayer { get { return Player.instance; } }
    
    public Transform HeadTF { get { return SteamVRPlayer.hmdTransforms[VRInjector.IsVRDevicePresent ? 0 : 1]; } }
    public Hand RightHand { get { return SteamVRPlayer.rightHand; } }
    public Hand LeftHand { get { return SteamVRPlayer.leftHand; } }

    [Tooltip("The layermask used for the ray that goes downward from the player controller. Sets the VR Player's position based on what it hits.")]
    public LayerMask positionMask = -1;

    private GameObject playerObject { get { return GameManager.Instance.PlayerObject; } }
    private Camera mainCamera { get { return GameManager.Instance.MainCamera; } }
    private CharacterController playerController { get { return GameManager.Instance.PlayerController; } }

    #region Singleton

    public static DaggerfallVRPlayer Instance { get; private set; }
    private void Awake()
    {
        if (!Instance)
            Instance = this;
        else
        {
            Debug.LogError("Second DaggerfallVRPlayer singleton has been spawned in the scene. This obviously shouldn't happen.");
        }
    }

    #endregion

    private void Start()
    {
        VRInputActions.OpenMenuAction.onStateDown += OpenMenu_onStateDown;
    }

    private void OnDestroy()
    {
        VRInputActions.OpenMenuAction.onStateDown -= OpenMenu_onStateDown;
    }

    private void Update()
    {
        transform.position = playerController.transform.position + Vector3.down * (playerController.height / 2f);
        transform.rotation = playerObject.transform.rotation;

        if (Input.GetKeyDown(KeyCode.R))
            ResetPlayerPosition();    
        //UpdatePlayerHeight();
    }

    public void ResetPlayerPosition()
    {
        Vector3 headPos = HeadTF.localPosition;
        Vector3 headParentPos = HeadTF.parent.localPosition;
        headParentPos = new Vector3(-headPos.x, headParentPos.y, -headPos.z);
        HeadTF.parent.localPosition = headParentPos;
    }


    private void OpenMenu_onStateDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        Debug.Log("SteamVR input action detected: OpenMenu");
        InputManager.Instance.AddAction(InputManager.Actions.CharacterSheet);
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
