using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using System;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UI;

public class VRController : MonoBehaviour  {
    [Tooltip("This should be set to the name of the GameObject that contains the VRUIManager script. '(Clone)' will be added if necessary")]
    public string VR_UI_MANAGER_NAME = "VRUIManager";

    [Tooltip("This should be set to the name of the GameObject that contains all the player objects.")]
    public string PLAYER_ADVANCED_NAME = "PlayerAdvanced";

    [Tooltip("This should be set to the name of the UI component that contains a User Interface Render Target script. '(Clone)' will be added if necessary")]
    public string FLOATING_UI_TARGET_NAME = "FloatingUI";

    [Tooltip("The name of the Steam VR model object, typically 'Model'")]
    public string CONTROLLER_MODEL_NAME = "Model";

    [Tooltip("The laser prefab is drawn when the controller is pointed at certain objects, such as distant UI targets.")]
    public GameObject LaserPrefab;

    [Tooltip("Set this to the proper hand.")]
    public HANDEDNESS hand = HANDEDNESS.UNKNOWN;

    [Tooltip("The name assigned by SteamVR when it creates the camera object for the player's eyes")]
    public String CameraEyeName = "Camera (eye)";

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
    public enum HANDEDNESS { LEFT, RIGHT, UNKNOWN };
    private SteamVR_TrackedObject trackedObj;
    private GameObject player;
    private CharacterController cc;
    private GameObject eyesCamera;
    private FloatingUITest floatingUI;

    private SteamVR_Controller.Device Controller {
        get { return SteamVR_Controller.Input((int)trackedObj.index); }
    }

    // UI targetting
    private GameObject laser;
    private Transform laserTransform;
    private Vector3 hitPoint;
    private bool laserOn = false;
    private InputManager inputManager;

    private bool _init = false;


    void Start() {
        if (hand == HANDEDNESS.UNKNOWN) {
            Debug.LogError("I don't know what hand this controller is! This should've been set at initialization time.");
            return;
        }

        // Obtain references
        GameObject go = GameObject.Find(VR_UI_MANAGER_NAME);
        if (!go) {
            go = GameObject.Find(VR_UI_MANAGER_NAME + "(Clone)");
        }
        if (go) {
            vrUIManager = go.GetComponent<VRUIManager>();
        } else { 
            Debug.LogError("A critical error occurred in the VR Controller while trying to get the VR UI Manager GameObject in the " + hand + 
                " VR Controller. Incorrect VR_UI_MANAGER name? The VR UI is going to be broken.");
            return;
        }

        floatingUI = vrUIManager.floatingUI.GetComponent<FloatingUITest>();

        player = GameObject.Find(PLAYER_ADVANCED_NAME);
        cc = player.GetComponent<CharacterController>();
        if (!player) {
            Debug.LogError("A critical error occurred in the VR Controller while trying to get either the main player Game Object or its character controller. " +
                "A problem with the provided GameObject name? Locomotion will be broken.");
            return;
        }

        eyesCamera = GameObject.Find(CameraEyeName);
        if (!eyesCamera) {
            Debug.LogError("A VR Controller script was unable to find the Camera (eyes) component with name " + CameraEyeName + ". Improper setting in the injected VRUI Manager prefab? Movement will be broken.");
            return;
        }

        if (LeftGlovePrefab && RightGlovePrefab && GloveAnimControllerPrefab) {
            if (hand == HANDEDNESS.LEFT) {
                gloveModel = Instantiate(LeftGlovePrefab);
            } else {
                gloveModel = Instantiate(RightGlovePrefab);
            }
            gloveModel.transform.SetParent(transform, false);
            gloveModel.transform.localPosition = new Vector3(0, 0, -0.05f);
            gloveModel.transform.rotation = Quaternion.identity;
            gloveModel.transform.Rotate(0, 90f, 0);
            gloveModel.transform.localScale = new Vector3(1.75f, 1.75f, 1.75f); 

            gloveAnimController = Instantiate(GloveAnimControllerPrefab);
            try {
                gloveAnimator = gloveModel.GetComponent<Animator>();
                gloveAnimator.runtimeAnimatorController = gloveAnimController;
            } catch (Exception e) {
                Debug.LogError("An error occurred while trying to set up the glove animation controller!");
            }

            transform.Find(CONTROLLER_MODEL_NAME).gameObject.SetActive(false);
        } else {
            Debug.LogError("The VR UI Manager didn't find a left or right glove prefab, or it was missing the glove animation controller. The glove models will be missing.");
        }

        // Initiate controller-specific elements
        laser = Instantiate(LaserPrefab);
        laserTransform = laser.transform;
        inputManager = InputManager.Instance;
        _init = true;
	}

    void Awake() {
        trackedObj = GetComponent<SteamVR_TrackedObject>();
    }

    private void ShowLaser(RaycastHit hit) {
        laser.SetActive(true);
        laserTransform.position = Vector3.Lerp(trackedObj.transform.position, hitPoint, 0.5f);
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

        if (hand == HANDEDNESS.LEFT) {
            handleLeftController();
        } else if (hand == HANDEDNESS.RIGHT) {
            handleRightController();
        }

        if (Controller.GetAxis(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger).x > 0.6) {
            Debug.Log(gameObject.name + " Trigger pressed");
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
            player.GetComponent<PlayerActivate>().rayEmitter = trackedObj.gameObject;
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
            if (Physics.Raycast(trackedObj.transform.position, trackedObj.transform.forward, out hit, 75)) {
                Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow);
                MeshRenderer mr = hit.transform.GetComponent<MeshRenderer>();
                if (mr) {
                    Vector3 newScale = new Vector3(mr.bounds.size.x, mr.bounds.size.y, mr.bounds.size.z);
                    vrUIManager.repositionHint(mr.bounds.center, newScale, mr.transform.rotation);
                }
            }

            if (Controller.GetPressDown(SteamVR_Controller.ButtonMask.Touchpad)) {
                Vector2 touchpad = Controller.GetAxis();
                if (!inputManager.IsPaused && touchpad.x > 0.3f) {
                    if (touchpad.x > 0.6f) 
                        player.transform.RotateAround(player.transform.position, player.transform.up, 30);
                    else
                        player.transform.RotateAround(player.transform.position, player.transform.up, 10);
                }
                else if (!inputManager.IsPaused && touchpad.x < -0.3f) {
                    if (touchpad.x < -0.6f)  
                        player.transform.RotateAround(player.transform.position, player.transform.up, -30);
                    else
                        player.transform.RotateAround(player.transform.position, player.transform.up, -10);
                } else {
                    tryAction();
                }
            }
        }
    }

    private void handleUIInput() {
        RaycastHit hit;
        if (Physics.Raycast(trackedObj.transform.position, transform.forward, out hit, 100, (1 << LayerMask.NameToLayer(vrUIManager.UI_LAYER_MASK_NAME)))) {
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
        if (!inputManager.IsPaused && Controller.GetAxis() != Vector2.zero) {
            Vector2 touchpad = Controller.GetAxis();
            if (touchpad.y > 0.15f || touchpad.y < -0.15f) {
                cc.Move(trackedObj.transform.forward * touchpad.y * FORWARD_SENSITIVITY);
                //player.transform.position -= player.transform.forward * Time.deltaTime * (touchpad.y * FORWARD_SENSITIVITY);

                //Vector2 pos = player.transform.position;
                //pos.y = Terrain.activeTerrain.SampleHeight(player.transform.position);
                //player.transform.position = pos;
            }

            if (touchpad.x > 0.15f || touchpad.x < -0.15f) {
                cc.Move(trackedObj.transform.right * touchpad.x * FORWARD_SENSITIVITY);
                //player.transform.position -= player.transform.right * Time.deltaTime * (touchpad.x * STRAFE_SENSITIVITY);
            }
        }
    }
}