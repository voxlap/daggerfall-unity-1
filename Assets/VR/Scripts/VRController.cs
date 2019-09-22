using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using System;
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
    private InputManager inputManager;
    private bool wasUIPressed = false;

    //Highlighting
    private VREquipment lastHighlightedVREquipment;
    private cakeslice.Outline lastHighlightedDaggerfallInteractable;

    //laser physics masks
    private LayerMask equipmentMask;
    private LayerMask uiMask;
    private LayerMask playerMask;
    private LayerMask hintsMask;
    private int equipmentLayer;

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
        VRInputActions.InteractUIAction.AddOnChangeListener(InteractUIAction_onChange, hand.handType);

        //set layermasks
        uiMask = LayerMask.GetMask("UI");
        playerMask = LayerMask.GetMask("Player");
        equipmentLayer = LayerMask.NameToLayer("VREquipment");
        equipmentMask = LayerMask.GetMask("VREquipment");
        hintsMask = ~(uiMask | playerMask);

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

        HandleControllerInput();

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

    private void HandleControllerInput()
    {
        if(!inputManager.IsPaused)
        {
            if(!VRInputActions.ActivateWalk.active || VRInputActions.ActivateWalk.GetState(hand.handType))
                WalkWithJoystick();
            if(!VRInputActions.ActivateRotate.active || VRInputActions.ActivateRotate.GetState(hand.handType))
            {
                switch (VRSettingsMenu.Instance.TurningMode)
                {
                    case VRTurningMode.Smooth:
                        RotatePlayerWithJoystick();
                        break;
                    case VRTurningMode.Snap:
                        if (!VRInputActions.ActivateRotate.active && VRInputActions.RotateAction.GetLastAxis(hand.handType).x == 0 && VRInputActions.RotateAction.GetAxis(hand.handType).x != 0
                            || VRInputActions.ActivateRotate.GetStateDown(hand.handType))
                            RotatePlayerWithJoystick();
                        break;
                    default:
                        break;
                }
            }
        }
    }
    private void RotatePlayerWithJoystick()
    {
        //get current rotation speed from joystick X position
        float curRotation = VRInputActions.RotateAction.GetAxis(hand.handType).x;
        Transform playerTF = Player.instance.trackingOriginTransform;
        //rotate player
        if (VRSettingsMenu.Instance.TurningMode == VRTurningMode.Smooth)
            DaggerfallVRPlayer.Instance.Rotate(VRSettingsMenu.Instance.SmoothTurningSpeed * curRotation * Time.deltaTime);
        else if (VRSettingsMenu.Instance.TurningMode == VRTurningMode.Snap)
        {
            //snap to nearest grid rotation at given angle
            int rotationAmount = (int)VRSettingsMenu.Instance.SnapTurningSpeed;
            float nearestRotation = (Mathf.RoundToInt(playerTF.eulerAngles.y) / rotationAmount) * rotationAmount;
            nearestRotation += rotationAmount * (curRotation > 0 ? 1f : -1f);
            if (nearestRotation < 0)
                nearestRotation = 360 + nearestRotation;
            nearestRotation %= 360;
            DaggerfallVRPlayer.Instance.SetRotation(nearestRotation);
        }

        //set mouselook yaw so that save games save your current rotation.
        GameManager.Instance.PlayerMouseLook.Yaw = playerTF.eulerAngles.y;
    }
    private void WalkWithJoystick()
    {
        Vector2 touchpad = VRInputActions.WalkAction.GetAxis(hand.handType);
        Vector3 moveDir = Vector3.zero;
        Transform tf = VRSettingsMenu.Instance.MovementMode == VRMovementMode.Controller ? transform : mainCamera.transform;

        if (touchpad.y > 0.15f || touchpad.y < -0.15f)
        {
            Vector3 controllerForward = tf.forward;
            controllerForward.y = 0;
            moveDir += controllerForward.normalized * touchpad.y;
        }
        else
            touchpad.y = 0;

        if (touchpad.x > 0.15f || touchpad.x < -0.15f)
        {
            Vector3 controllerRight = tf.right;
            controllerRight.y = 0;
            moveDir += controllerRight.normalized * touchpad.x;
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

    public bool LaserRaycast(out RaycastHit hit, int layerMask)
    {
        return Physics.Raycast(laserPositionTF.position, laserPositionTF.forward, out hit, 75, layerMask);
    }
    private void ActivateUI(bool doClick = false, bool doUnclick = false)
    {
        RaycastHit hit;
        if (LaserRaycast(out hit, uiMask))
        {
            ShowLaser(hit.distance);

            FloatingUI floatingUI;
            if (floatingUI = hit.transform.GetComponent<FloatingUI>())
            {
                Vector3 localPoint = hit.transform.GetComponent<RectTransform>().InverseTransformPoint(hit.point);
                if (doClick)
                    floatingUI.HandleClick(localPoint);
                else if (doUnclick)
                    floatingUI.HandleUnclick(localPoint);
                else
                    floatingUI.HandlePointer(localPoint);
            }
        }
        else
            ShowLaser(75);
    }
    /// <summary>
    /// Activates a hint that the daggerfall item you're pointing at with a laser can be activated
    /// </summary>
    /// <param name="hit">The raycast hit result from the laser raycast, using hintsMask which excludes Player and UI layers</param>
    /// <returns>true if a hint was activated, false otherwise</returns>
    private bool ActivateDaggerfallInteractableHints(out RaycastHit hit)
    {
        if (LaserRaycast(out hit, hintsMask))
        {
            ShowLaser(hit.distance);
            Debug.DrawRay(laserPositionTF.position, laserPositionTF.forward * hit.distance, Color.yellow);
            MeshRenderer mr = hit.collider.transform.GetComponent<MeshRenderer>();

            if (mr && mr.transform.tag != "StaticGeometry" && mr.gameObject.layer != equipmentLayer)
            {
                if (lastHighlightedDaggerfallInteractable)
                    lastHighlightedDaggerfallInteractable.enabled = false;

                cakeslice.Outline outline = mr.GetComponent<cakeslice.Outline>();
                if (outline)
                    outline.enabled = true;
                else
                {
                    outline = mr.gameObject.AddComponent<cakeslice.Outline>();
                    outline.color = 1;
                }
                lastHighlightedDaggerfallInteractable = outline;
                return true;
            }
        }
        else
            ShowLaser(75);

        if (lastHighlightedDaggerfallInteractable)
            lastHighlightedDaggerfallInteractable.enabled = false;
        return false;
    }

    /// <summary>
    /// set activity of laser and position of UI pointer
    /// </summary>
    private void HandleLaser()
    {
        //we only want to show laser if you grip the controller and you're not holding anything
        if (VRInputActions.GrabGripAction.GetState(hand.handType) && hand.AttachedObjects.Count == 0)
        {
            laser.SetActive(true);
            if (inputManager.IsPaused)
            {
                //activate UI with laser
                ActivateUI();
                //no highlighting of weapons when paused
                DeactivateHighlightOnVREquipment();
            }
            else
            {
                //try activating hints out in the daggerfall world
                RaycastHit hit;
                bool didActivateHint = ActivateDaggerfallInteractableHints(out hit);

                //if no hints were activated, perhaps a VR weapon was blocking the cast? Highlight it if so.
                if (!didActivateHint && hit.transform != null && hit.transform.gameObject.layer == equipmentLayer)
                {
                    //the only things that should be on this layer are VREquipment and VRWeaponPieces
                    VREquipment equipment = hit.transform.GetComponent<VREquipment>();
                    if (!equipment)
                    {
                        try
                        {
                            equipment = hit.transform.GetComponent<VRWeaponPiece>().Weapon;
                        }
                        catch
                        {
                            Debug.LogError(hit.transform.name + " is on the VREquipment layer but does not have a VREquipment (or adjacent) script. Unable to highlight.");
                            return;
                        }
                    }
                    //Highlight equipment. Handle last highlighted equipment.
                    if (equipment != lastHighlightedVREquipment)
                    {
                        DeactivateHighlightOnVREquipment();
                        equipment.Highlight();
                    }
                    lastHighlightedVREquipment = equipment;
                }
                //No vr weapon was highlighted this frame, so unhighlight any weapon we were previously highlighting
                else
                    DeactivateHighlightOnVREquipment();
            }
        }
        else
        {
            laser.SetActive(false);
            if (lastHighlightedDaggerfallInteractable)
                lastHighlightedDaggerfallInteractable.enabled = false;
            DeactivateHighlightOnVREquipment();
        }
    }
    private void DeactivateHighlightOnVREquipment()
    {
        if (lastHighlightedVREquipment != null)
        {
            lastHighlightedVREquipment.UnHighlight();
            lastHighlightedVREquipment = null;
        }
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
        if (LaserRaycast(out hit, LayerMask.GetMask("VREquipment")) && hit.rigidbody != null)
        {
            equipment = hit.rigidbody.GetComponent<VREquipment>();
            if(equipment)
                equipment.ForceAttachToHand(hand.handType);
            else //Maybe it's a spike ball
            {
                ConfigurableJoint joint = hit.rigidbody.GetComponent<ConfigurableJoint>();
                if (joint && (equipment = joint.connectedBody.GetComponent<VREquipment>()))
                {
                    equipment.ForceAttachToHand(hand.handType);
                }
            }
        }
    }

    // SteamVR actions
    private void SelectWorldObject_onStateDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        Debug.Log("SteamVR input action detected: SelectWorldObject on hand " + hand.handType);
        if (!inputManager.IsPaused)
        {
            TrySelectObject();
            TryGrabVREquipment();
        }
    }

    private void InteractUIAction_onChange(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource, bool newState)
    {
        if(laser.gameObject.activeSelf)
            ActivateUI(newState, !newState);
    }
}