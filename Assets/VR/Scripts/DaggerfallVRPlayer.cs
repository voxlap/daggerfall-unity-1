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
    
    private void Update()
    {
        //playerController.height = mainCamera.transform.localPosition.y;
        //if (GameManager.Instance.PlayerMotor.IsGrounded)
        //{
        //    RaycastHit hit;
        //    if (Physics.Raycast(playerController.transform.position, Vector3.down, out hit, playerController.height, positionMask))
        //        transform.position = hit.point;
        //}
        transform.position = playerController.transform.position + Vector3.down * (playerController.height / 2f);
        transform.rotation = playerObject.transform.rotation;
    }
}
