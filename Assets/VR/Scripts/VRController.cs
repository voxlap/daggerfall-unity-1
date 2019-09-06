using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using System;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;
using Valve.VR.InteractionSystem;
using DaggerfallWorkshop.Game.UserInterface;

public class VRController : MonoBehaviour
{
    [Tooltip("The laser prefab is drawn when the controller is pointed at certain objects, such as distant UI targets.")]
    public GameObject LaserPrefab;
    [Tooltip("The transform which determines the laser's position and forward")]
    public Transform laserPositionTF;

    public Hand VRHand { get { return hand; } }

    // References
    private Hand hand;
    private PlayerGroundMotor groundMotor;
    private PlayerMotor playerMoter { get { return GameManager.Instance.PlayerMotor; } }
    private AcrobatMotor acrobatMotor { get { return GameManager.Instance.AcrobatMotor; } }
    private Camera mainCamera { get { return GameManager.Instance.MainCamera; } }
    private VRUIManager vrUIManager { get { return VRUIManager.Instance; } }

    // UI targetting
    private GameObject laser;
    private bool laserOn = false;
    private InputManager inputManager;
    private bool wasUIPressed = false;

    private bool _init = false;

    void Start() {
        // Obtain references
        if(!hand){
            Debug.LogError("A critical error occurred in the VR Controller while trying to get the Hand component reference. " +
                "This script should be attached to an object that has the Hand script, under the Player instance, from SteamVR's InteractionSystem.");
            return;
        }

        // Initiate controller-specific elements
        laser = Instantiate(LaserPrefab);
        inputManager = InputManager.Instance;

        // initiate SteamVR action listeners
        VRInputActions.SelectWorldObjectAction.AddOnStateDownListener(SelectWorldObject_onStateDown, hand.handType);

        _init = true;
	}

    void Awake() {
        hand = GetComponent<Hand>();
        groundMotor = playerMoter.GetComponent<PlayerGroundMotor>();
    }

    private void OnDisable()
    {
        if (wasUIPressed)
            SetMousePressed(false);
    }

    void LateUpdate() {
        if (!_init) return;

        switch (hand.handType)
        {
            case SteamVR_Input_Sources.Any:
                HandleLeftController();
                HandleRightController();
                break;
            case SteamVR_Input_Sources.LeftHand:
                HandleLeftController();
                break;
            case SteamVR_Input_Sources.RightHand:
                HandleRightController();
                break;
            default:
                break;
        }
        HandleLaser();
        //set custom mouse state for ui mouse down events
        SetMousePressed(VRInputActions.InteractUIAction.GetState(hand.handType));
    }

    private void TrySelectObject()
    {
        DaggerfallUI.Instance.CustomMousePosition = Vector2.zero;
        GameManager.Instance.PlayerActivate.rayEmitter = laserPositionTF.gameObject;
        InputManager.Instance.AddAction(InputManager.Actions.ActivateCenterObject);
    }

    private void HandleRightController()
    {
        if(!inputManager.IsPaused && VRInputActions.RotateAction.active )
        {
            RotatePlayerWithJoystick();
        }
    }
    private void HandleLeftController()
    {
        if (!inputManager.IsPaused && VRInputActions.WalkAction.active)
        {
            WalkWithJoystick();
        }
    }

    private void RotatePlayerWithJoystick()
    {
        float curRotation = VRInputActions.RotateAction.GetAxis(hand.handType).x;
        playerMoter.transform.Rotate(Vector3.up, 135f * curRotation * Time.deltaTime);
    }
    private void WalkWithJoystick()
    {
        Vector2 touchpad = VRInputActions.WalkAction.GetAxis(hand.handType);
        Vector3 moveDir = Vector3.zero;
        if (touchpad.y > 0.15f || touchpad.y < -0.15f)
        {
            Vector3 camForward = mainCamera.transform.forward;
            camForward.y = 0;
            moveDir += camForward.normalized * touchpad.y;
        }
        else
            touchpad.y = 0;

        if (touchpad.x > 0.15f || touchpad.x < -0.15f)
        {
            Vector3 camRight = mainCamera.transform.right;
            camRight.y = 0;
            moveDir += camRight.normalized * touchpad.x;
        }
        else
            touchpad.x = 0;

        if (moveDir != Vector3.zero)
        {
            moveDir = Quaternion.Inverse(playerMoter.transform.rotation) * moveDir;
            inputManager.ApplyHorizontalForce(moveDir.x);
            inputManager.ApplyVerticalForce(moveDir.z);
        }
    }

    private void ActivateUI()
    {
        RaycastHit hit;
        if (Physics.Raycast(laserPositionTF.position, laserPositionTF.forward, out hit, 100, LayerMask.GetMask("UI")))
        {
            ShowLaser(hit.distance);

            FloatingUITest floatingUI;
            if (floatingUI = hit.transform.GetComponent<FloatingUITest>())
            {
                Vector3 localPoint = hit.transform.GetComponent<RectTransform>().InverseTransformPoint(hit.point);
                floatingUI.HandlePointer(localPoint);
            }
        }
        else
            ShowLaser(100);
    }
    private bool HintsRaycast(out RaycastHit hit)
    {
        return Physics.Raycast(laserPositionTF.position, laserPositionTF.forward, out hit, 75, ~(LayerMask.GetMask("UI") | LayerMask.GetMask("Player")));
    }
    private void ActivateHints()
    {
        RaycastHit hit;
        if (HintsRaycast(out hit))
        {
            Debug.DrawRay(laserPositionTF.position, laserPositionTF.forward * hit.distance, Color.yellow);
            MeshRenderer mr = hit.collider.transform.GetComponent<MeshRenderer>();
            if (mr && mr.transform.tag != "StaticGeometry")
            {
                Vector3 newScale = mr.bounds.size;
                BillboardRotationCorrector billboardCorrector;
                if (billboardCorrector = mr.GetComponent<BillboardRotationCorrector>())
                { // billboards' mesh sizes are all screwy if not rotated to zero
                    mr.transform.rotation = Quaternion.identity;
                    newScale = mr.bounds.size;
                    newScale.z = .1f;
                    billboardCorrector.CorrectRotation();
                }
                newScale *= 1.05f;
                vrUIManager.repositionHint(mr.bounds.center, newScale, mr.transform.rotation);

                ShowLaser(hit.distance);
            }
            else
            {
                vrUIManager.HideHint();
                ShowLaser(75);
            }
        }
        else
            vrUIManager.HideHint();
    }


    private void HandleLaser()
    {
        //set activity of laser and position of UI pointer
        if (VRInputActions.GrabGripAction.GetState(hand.handType) && !VRInputActions.GrabPinchAction.GetState(hand.handType))
        {
            laser.SetActive(true);
            if (inputManager.IsPaused)
            {
                ActivateUI();
            }
            else
            {
                ActivateHints();
            }
        }
        else
            laser.SetActive(false);
    }
    private void ShowLaser(float distance)
    {
        laser.SetActive(true);
        laser.transform.position = laserPositionTF.position + laserPositionTF.forward * (distance / 2f);
        laser.transform.rotation = laserPositionTF.rotation;
        laser.transform.localScale = new Vector3(laser.transform.localScale.x, laser.transform.localScale.y, distance);
    }
    private void SetMousePressed(bool isPressed)
    {
        DaggerfallInput.SetMouseButton(0, isPressed);
        wasUIPressed = isPressed;
    }
    private void TryGrabVREquipment()
    {
        RaycastHit hit;
        VREquipment equipment;
        if (HintsRaycast(out hit) && (equipment = hit.transform.GetComponent<VREquipment>()) != null)
        {
            equipment.ForceAttachToHand(hand.handType);
        }
    }

    // SteamVR actions
    private void SelectWorldObject_onStateDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        Debug.Log("SteamVR input action detected: SelectWorldObject from " + fromSource + " on hand " + hand.handType);
        if (!inputManager.IsPaused)
        {
            TrySelectObject();
            TryGrabVREquipment();
        }
    }
}