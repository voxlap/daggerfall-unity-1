using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using System;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class VRController : MonoBehaviour  {
    

    [Tooltip("This should be set to the name of the GameObject that contains the VRUIManager script. '(Clone)' will be added if necessary")]
    public string VR_UI_MANAGER_NAME = "VRUIManager";
    
    [Tooltip("The laser prefab is drawn when the controller is pointed at certain objects, such as distant UI targets.")]
    public GameObject LaserPrefab;

    public GameObject LeftGlovePrefab;
    public GameObject RightGlovePrefab;
    private GameObject gloveModel;
    public AnimatorController GloveAnimControllerPrefab;
    private AnimatorController gloveAnimController;
    private Animator gloveAnimator;

    public float FORWARD_SENSITIVITY = 0.05f;
    public float STRAFE_SENSITIVITY = 0.05f;

    // References
    private VRUIManager vrUIManager;
    private Hand hand;
    private CharacterController player;
    private Camera eyesCamera;
    private FloatingUITest floatingUI;

    // UI targetting
    private GameObject laser;
    private Transform laserTransform;
    private Vector3 hitPoint;
    private bool laserOn = false;
    private InputManager inputManager;

    // SteamVR Input
    private SteamVR_Action_Boolean pointing = SteamVR_Input.GetBooleanAction("Point");
    private SteamVR_Action_Boolean openMenu = SteamVR_Input.GetBooleanAction("OpenMenu");
    private SteamVR_Action_Single rotate = SteamVR_Input.GetSingleAction("Rotate");
    private SteamVR_Action_Vector2 walk = SteamVR_Input.GetVector2Action("Walk");
    private SteamVR_Action_Boolean selectWorldObject = SteamVR_Input.GetBooleanAction("SelectWorldObject");

    public bool IsPointing
    {
        get { return pointing.GetState(hand.handType); }
    }

    private bool _init = false;


    void Start() {
        // Obtain references
        if(!hand){
            Debug.LogError("A critical error occurred in the VR Controller while trying to get the Hand component reference. " +
                "This script should be attached to an object that has the Hand script, under the Player instance, from SteamVR's InteractionSystem.");
            return;
        }
        GameObject go = GameObject.Find(VR_UI_MANAGER_NAME);
        if (!go) {
            go = GameObject.Find(VR_UI_MANAGER_NAME + "(Clone)");
        }
        if (go) {
            vrUIManager = go.GetComponent<VRUIManager>();
        } else {
            Debug.LogError("A critical error occurred in the VR Controller while trying to get the VR UI Manager GameObject in the " + hand.handType + 
                " VR Controller. Incorrect VR_UI_MANAGER name? The VR UI is going to be broken.");
            return;
        }

        floatingUI = vrUIManager.floatingUI.GetComponent<FloatingUITest>();

        player = FindObjectOfType<CharacterController>();

        eyesCamera = Camera.main;
        if (!eyesCamera) {
            Debug.LogError("A VR Controller script was unable to find a main camera. Movement will be broken.");
            return;
        }

        if (LeftGlovePrefab && RightGlovePrefab && GloveAnimControllerPrefab) {
            if (hand.handType == SteamVR_Input_Sources.LeftHand) {
                gloveModel = Instantiate(LeftGlovePrefab);
            } else {
                gloveModel = Instantiate(RightGlovePrefab);
            }
            gloveModel.transform.SetParent(transform, false);
            gloveModel.transform.localPosition = new Vector3(0, 0, -0.05f);
            gloveModel.transform.rotation = Quaternion.identity;
            gloveModel.transform.Rotate(0, 90f, 0, Space.Self);
            gloveModel.transform.localScale = new Vector3(1.75f, 1.75f, 1.75f); 

            gloveAnimController = Instantiate(GloveAnimControllerPrefab);
            try {
                gloveAnimator = gloveModel.GetComponent<Animator>();
                gloveAnimator.runtimeAnimatorController = gloveAnimController;
            } catch (Exception e) {
                Debug.LogError("An error occurred while trying to set up the glove animation controller!");
            }
            
        } else {
            Debug.LogError("The VR UI Manager didn't find a left or right glove prefab, or it was missing the glove animation controller. The glove models will be missing.");
        }

        // Initiate controller-specific elements
        laser = Instantiate(LaserPrefab);
        laserTransform = laser.transform;
        inputManager = InputManager.Instance;

        // initiate SteamVR actions
        openMenu.onStateDown += OpenMenu_onStateDown;
        selectWorldObject.onStateDown += SelectWorldObject_onStateDown;

        _init = true;
	}

    void Awake() {
        hand = GetComponent<Hand>();
    }

    private void ShowLaser(RaycastHit hit) {
        laser.SetActive(true);
        laserTransform.position = Vector3.Lerp(hand.transform.position, hitPoint, 0.5f);
        laserTransform.LookAt(hitPoint);
        laserTransform.localScale = new Vector3(laserTransform.localScale.x, laserTransform.localScale.y, hit.distance);
    }

    void Update() {
        if (!_init) return;
        /*laser.SetActive(laserOn);
        if (laserOn) {
            RaycastHit hit;
            if (Physics.Raycast(trackedObj.transform.position, trackedObj.transform.forward, out hit, 100)) {
                hitPoint = hit.point;
                ShowLaser(hit);
            } 
        }
        */

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

        if (IsPointing) {
            Debug.Log(gameObject.name + " pointing");
            gloveModel.GetComponent<Animator>().SetBool("isPointing", true);
        } else {
            gloveModel.GetComponent<Animator>().SetBool("isPointing", false);
        }

    }

    private void tryAction() {
        DaggerfallUI.Instance.CustomMousePosition = new Vector2(0, 0);
        RaycastHit hit;
        //if (Physics.Raycast(trackedObj.transform.position, trackedObj.transform.forward, out hit, 75)) {
            Debug.Log("Adding action!");
            //hitPoint = hit.point;
            player.GetComponent<PlayerActivate>().rayEmitter = hand.gameObject;
            InputManager.Instance.AddAction(InputManager.Actions.ActivateCenterObject);
        //} 
    }

    private void handleHitDoor(GameObject hitDoor) {
        MeshRenderer mr = hitDoor.GetComponent<MeshRenderer>(); 
        if (mr) {
            mr.enabled = !mr.enabled;
        }
    }

    private void handleRightController() { 
        RaycastHit hit;

        // Touchpad press for rotate
        if (inputManager.IsPaused) {
            handleUIInput();
        } else {
            laser.SetActive(false);

            //activate hints
            if (Physics.Raycast(hand.transform.position, hand.transform.forward, out hit, 75)) {
                Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
                MeshRenderer mr = hit.transform.GetComponent<MeshRenderer>();
                if (mr) {
                    Vector3 newScale = new Vector3(mr.bounds.size.x, mr.bounds.size.y, mr.bounds.size.z);
                    vrUIManager.repositionHint(mr.bounds.center, newScale, mr.transform.rotation);
                }
            }
            //rotate
            float curRotation = rotate.GetAxis(hand.handType);
            if (curRotation > 0.3f) {
                if (curRotation > 0.6f) 
                    player.transform.RotateAround(player.transform.position, player.transform.up, 30);
                else
                    player.transform.RotateAround(player.transform.position, player.transform.up, 10);
            }
            else if (curRotation < -0.3f) {
                if (curRotation < -0.6f)  
                    player.transform.RotateAround(player.transform.position, player.transform.up, -30);
                else
                    player.transform.RotateAround(player.transform.position, player.transform.up, -10);
            }
        }
    }

    private void handleUIInput() {
        RaycastHit hit;
        if (Physics.Raycast(hand.transform.position, transform.forward, out hit, 100, (1 << LayerMask.NameToLayer(vrUIManager.UI_LAYER_MASK_NAME)))) {
            ShowLaser(hit);
            hitPoint = hit.point;
            if (hit.transform.gameObject.name == floatingUI.gameObject.name) {
                //Debug.Log("World position: " + hit.point.ToString());
                //Vector3 localPoint = hit.transform.InverseTransformPoint(hit.point);
                Vector3 localPoint = hit.transform.gameObject.GetComponent<RawImage>().rectTransform.InverseTransformPoint(hit.point);
                //Debug.Log("Inverse Transform Point: " + localPoint);
                floatingUI.HandlePointer(localPoint);
            }
        }
    }

    private void handleLeftController() {
        // Touchpad drag for slide
        if (!inputManager.IsPaused && walk.active) {
            Vector2 touchpad = walk.GetAxis(hand.handType);
            if (touchpad.y > 0.15f || touchpad.y < -0.15f) {
                player.Move(hand.transform.forward * touchpad.y * FORWARD_SENSITIVITY);
                //player.transform.position -= player.transform.forward * Time.deltaTime * (touchpad.y * FORWARD_SENSITIVITY);

                //Vector2 pos = player.transform.position;
                //pos.y = Terrain.activeTerrain.SampleHeight(player.transform.position);
                //player.transform.position = pos;
            }

            if (touchpad.x > 0.15f || touchpad.x < -0.15f) {
                player.Move(hand.transform.right * touchpad.x * FORWARD_SENSITIVITY);
                //player.transform.position -= player.transform.right * Time.deltaTime * (touchpad.x * STRAFE_SENSITIVITY);
            }
        }
    }

    // SteamVR actions

    private void OpenMenu_onStateDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        Debug.Log("SteamVR input action detected: OpenMenu");
        InputManager.Instance.AddAction(InputManager.Actions.Escape);
    }

    private void SelectWorldObject_onStateDown(SteamVR_Action_Boolean fromAction, SteamVR_Input_Sources fromSource)
    {
        Debug.Log("SteamVR input action detected: SelectWorldObject");
        if (!inputManager.IsPaused)
            tryAction();
    }
}