using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using System;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class VRController : MonoBehaviour  {
    
    [Tooltip("The laser prefab is drawn when the controller is pointed at certain objects, such as distant UI targets.")]
    public GameObject LaserPrefab;

    public float FORWARD_SENSITIVITY = 0.05f;
    public float STRAFE_SENSITIVITY = 0.05f;

    // References
    private Hand hand;
    private PlayerGroundMotor groundMotor;
    private PlayerMotor playerMoter { get { return GameManager.Instance.PlayerMotor; } }
    private AcrobatMotor acrobatMotor { get { return GameManager.Instance.AcrobatMotor; } }
    private Camera mainCamera { get { return GameManager.Instance.MainCamera; } }
    private VRUIManager vrUIManager { get { return VRUIManager.Instance; } }

    // UI targetting
    private GameObject laser;
    private Transform laserTransform;
    private Vector3 hitPoint;
    private bool laserOn = false;
    private InputManager inputManager;
    
    private bool _init = false;

    void Start() {
        // Obtain references
        if(!hand){
            Debug.LogError("A critical error occurred in the VR Controller while trying to get the Hand component reference. " +
                "This script should be attached to an object that has the Hand script, under the Player instance, from SteamVR's InteractionSystem.");
            return;
        }

        //SpawnGlove();

        // Initiate controller-specific elements
        laser = Instantiate(LaserPrefab);
        laserTransform = laser.transform;
        inputManager = InputManager.Instance;

        // initiate SteamVR actions
        VRInputActions.SelectWorldObjectAction.AddOnStateDownListener(SelectWorldObject_onStateDown, hand.handType);

        _init = true;
	}

    void Awake() {
        hand = GetComponent<Hand>();
        groundMotor = playerMoter.GetComponent<PlayerGroundMotor>();
    }

    private void ShowLaser(RaycastHit hit) {
        laser.SetActive(true);
        laserTransform.position = Vector3.Lerp(hand.transform.position, hitPoint, 0.5f);
        laserTransform.LookAt(hitPoint);
        laserTransform.localScale = new Vector3(laserTransform.localScale.x, laserTransform.localScale.y, hit.distance);
    }

    void LateUpdate() {
        if (!_init) return;

        switch (hand.handType)
        {
            case SteamVR_Input_Sources.Any:
                handleLeftController();
                handleRightController();
                break;
            case SteamVR_Input_Sources.LeftHand:
                handleLeftController();
                break;
            case SteamVR_Input_Sources.RightHand:
                handleRightController();
                break;
            default:
                break;
        }

        //UpdateGloveAnim();
    }

    private void tryAction() {
        GameManager.Instance.PlayerActivate.rayEmitter = hand.gameObject;
        InputManager.Instance.AddAction(InputManager.Actions.ActivateCenterObject);
    }

    private void handleHitDoor(GameObject hitDoor) {
        MeshRenderer mr = hitDoor.GetComponent<MeshRenderer>(); 
        if (mr) {
            mr.enabled = !mr.enabled;
        }
    }

    private void handleRightController()
    {
        RaycastHit hit;

        // Touchpad press for rotate
        if (inputManager.IsPaused)
        {
            handleUIInput();
        }
        else
        {
            laser.SetActive(false);

            //activate hints
            if (Physics.Raycast(hand.transform.position, hand.transform.forward, out hit, 75) && hit.transform.tag != "StaticGeometry")
            {
                Debug.DrawRay(hand.transform.position, hand.transform.forward * hit.distance, Color.yellow);
                MeshRenderer mr = hit.transform.GetComponent<MeshRenderer>();
                if (mr)
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
                }
                else
                    vrUIManager.HideHint();
            }
            else
                vrUIManager.HideHint();
            //rotate
            float curRotation = VRInputActions.RotateAction.GetAxis(hand.handType).x;
            playerMoter.transform.Rotate(Vector3.up, 135f * curRotation * Time.deltaTime);
        }
    }

    private void handleLeftController()
    {
        // Touchpad drag for slide
        if (!inputManager.IsPaused && VRInputActions.WalkAction.active)
        {
            Vector2 touchpad = VRInputActions.WalkAction.GetAxis(hand.handType);
            Vector3 moveDir = Vector3.zero;
            if (touchpad.y > 0.15f || touchpad.y < -0.15f)
            {
                Vector3 camForward = mainCamera.transform.forward;
                camForward.y = 0;
                moveDir += camForward.normalized * touchpad.y;
                //player.transform.position -= player.transform.forward * Time.deltaTime * (touchpad.y * FORWARD_SENSITIVITY);

                //Vector2 pos = player.transform.position;
                //pos.y = Terrain.activeTerrain.SampleHeight(player.transform.position);
                //player.transform.position = pos;
            }
            else
                touchpad.y = 0;

            if (touchpad.x > 0.15f || touchpad.x < -0.15f)
            {
                Vector3 camRight = mainCamera.transform.right;
                camRight.y = 0;
                moveDir += camRight.normalized * touchpad.x;
                //player.transform.position -= player.transform.right * Time.deltaTime * (touchpad.x * STRAFE_SENSITIVITY);
            }
            else
                touchpad.x = 0;

            if(moveDir != Vector3.zero)
            {
                moveDir = Quaternion.Inverse(playerMoter.transform.rotation) * moveDir;
                inputManager.ApplyHorizontalForce(moveDir.x);
                inputManager.ApplyVerticalForce(moveDir.z);
            }
        }
    }

    private void handleUIInput()
    {
        RaycastHit hit;
        if (Physics.Raycast(hand.transform.position, hand.transform.forward, out hit, 100, LayerMask.GetMask(VRUIManager.UI_LAYER_NAME)))
        {
            ShowLaser(hit);
            hitPoint = hit.point;
            FloatingUITest floatingUI;
            Debug.Log("Hit " + hit.transform.name);
            if (floatingUI = hit.transform.GetComponent<FloatingUITest>())
            {
                //Debug.Log("World position: " + hit.point.ToString());
                //Vector3 localPoint = hit.transform.InverseTransformPoint(hit.point);
                Vector3 localPoint = hit.transform.GetComponent<RectTransform>().InverseTransformPoint(hit.point);
                //Debug.Log("Inverse Transform Point: " + localPoint);
                floatingUI.HandlePointer(localPoint);
            }
        }
    }

    // SteamVR actions

    private void SelectWorldObject_onStateDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        Debug.Log("SteamVR input action detected: SelectWorldObject from " + fromSource + " on hand " + hand.handType);
        if (inputManager.IsPaused)
            handleUIInput();
        tryAction();
    }


    // possibly obsolete
    /*

    //public GameObject LeftGlovePrefab;
    //public GameObject RightGlovePrefab;
    //private GameObject gloveModel;
    //public AnimatorController GloveAnimControllerPrefab;
    //private AnimatorController gloveAnimController;
    //private Animator gloveAnimator;
     
    private void SpawnGlove()
    {
        if (LeftGlovePrefab && RightGlovePrefab && GloveAnimControllerPrefab)
        {
            if (hand.handType == SteamVR_Input_Sources.LeftHand)
            {
                gloveModel = Instantiate(LeftGlovePrefab);
            }
            else
            {
                gloveModel = Instantiate(RightGlovePrefab);
            }
            gloveModel.transform.SetParent(transform, false);
            gloveModel.transform.localPosition = new Vector3(0, 0, -0.05f);
            gloveModel.transform.rotation = Quaternion.identity;
            gloveModel.transform.Rotate(0, 90f, 0, Space.Self);
            gloveModel.transform.localScale = new Vector3(1.75f, 1.75f, 1.75f);

            gloveAnimController = Instantiate(GloveAnimControllerPrefab);
            try
            {
                gloveAnimator = gloveModel.GetComponent<Animator>();
                gloveAnimator.runtimeAnimatorController = gloveAnimController;
            }
            catch (Exception e)
            {
                Debug.LogError("An error occurred while trying to set up the glove animation controller!");
            }

        }
        else
        {
            Debug.LogError("The VR UI Manager didn't find a left or right glove prefab, or it was missing the glove animation controller. The glove models will be missing.");
        }
    }
    
    private void UpdateGloveAnim()
    {
        if (VRInputActions.GrabGripAction.GetState(hand.handType))
        {
            //Debug.Log(gameObject.name + " grabgrip");
            gloveModel.GetComponent<Animator>().SetBool("isPointing", true);
        }
        else
        {
            gloveModel.GetComponent<Animator>().SetBool("isPointing", false);
        }
    }
    */
}