using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using System;
using UnityEngine;

public class VRController : MonoBehaviour  {
    [Tooltip("This should be set to the name of the GameObject that contains the VRUIManager script. '(Clone)' will be added if necessary")]
    public string VR_UI_MANAGER_NAME = "VRUIManager";

    [Tooltip("This should be set to the name of the GameObject that contains all the player objects.")]
    public string PLAYER_ADVANCED_NAME = "PlayerAdvanced";

    [Tooltip("The laser prefab is drawn when the controller is pointed at certain objects, such as distant UI targets.")]
    public GameObject LaserPrefab;

    [Tooltip("Set this to the proper hand.")]
    public HANDEDNESS hand = HANDEDNESS.UNKNOWN;

    public float FORWARD_SENSITIVITY = 0.05f;
    public float STRAFE_SENSITIVITY = 0.05f;

    // References
    private VRUIManager vrUIManager;
    public enum HANDEDNESS { LEFT, RIGHT, UNKNOWN };
    private SteamVR_TrackedObject trackedObj;
    private GameObject player;
    private CharacterController cc;

    private SteamVR_Controller.Device Controller {
        get { return SteamVR_Controller.Input((int)trackedObj.index); }
    }

    // UI targetting
    private GameObject laser;
    private Transform laserTransform;
    private Vector3 hitPoint;
    private bool laserOn = false;

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

        if (!go || !vrUIManager) {
            Debug.LogError("A critical error occurred in the VR Controller while trying to get the VR UI Manager script in the GameObject " +
                "(which was found correctly). The VR UI is going to be broken.");
            return;
        }

        player = GameObject.Find(PLAYER_ADVANCED_NAME);
        cc = player.GetComponent<CharacterController>();
        if (!player) {
            Debug.LogError("A critical error occurred in the VR Controller while trying to get either the main player Game Object or its character controller. " +
                "A problem with the provided GameObject name? Locomotion will be broken.");
            return;
        }

        // Initiate controller-specific elements
        laser = Instantiate(LaserPrefab);
        laserTransform = laser.transform;
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
        laser.SetActive(laserOn);
        if (laserOn) {
            RaycastHit hit;
            if (Physics.Raycast(trackedObj.transform.position, transform.forward, out hit, 100)) {
                hitPoint = hit.point;
                if (handleHitObject(hit.collider.gameObject)) {
                    laserOn = false;
                } else {
                    ShowLaser(hit);
                }
            } 
        }

        if (hand == HANDEDNESS.LEFT) {
            handleLeftController();
        } else if (hand == HANDEDNESS.RIGHT) {
            handleRightController();
        }
    }

    private bool handleHitObject(GameObject hitObject) {
        bool handled = false;
        if (hitObject) {
            DaggerfallActionDoor door = hitObject.GetComponent<DaggerfallActionDoor>();
            if (door) {
                handleHitDoor(hitObject);
                handled = true;
            }
        }
        return handled;
    }

    private void handleHitDoor(GameObject hitDoor) {
        MeshRenderer mr = hitDoor.GetComponent<MeshRenderer>(); 
        if (mr) {
            mr.enabled = !mr.enabled;
        }
    }

    private void handleRightController() { 
        // Touchpad press for rotate
        if (Controller.GetPressDown(SteamVR_Controller.ButtonMask.Touchpad)) {
            Vector2 touchpad = Controller.GetAxis();
            if (touchpad.x > 0.3f) {
                if (touchpad.x > 0.6f) 
                    player.transform.Rotate(0, 30, 0);
                else
                    player.transform.Rotate(0, 10, 0);
            }
            else if (touchpad.x < -0.3f) {
                if (touchpad.x < -0.6f)  
                    player.transform.Rotate(0, -30, 0);
                else
                    player.transform.Rotate(0, -10, 0);
            } else {
                laserOn = !laserOn;
            }
        }
    }

    private void handleLeftController() {
        // Touchpad drag for slide
        if (Controller.GetAxis() != Vector2.zero) {
            Vector2 touchpad = Controller.GetAxis();
            if (touchpad.y > 0.2f || touchpad.y < -0.2f) {
                cc.Move(transform.forward * touchpad.y * FORWARD_SENSITIVITY);
                //player.transform.position -= player.transform.forward * Time.deltaTime * (touchpad.y * FORWARD_SENSITIVITY);

                //Vector2 pos = player.transform.position;
                //pos.y = Terrain.activeTerrain.SampleHeight(player.transform.position);
                //player.transform.position = pos;
            }

            if (touchpad.x > 0.2f || touchpad.x < -0.2f) {
                cc.Move(transform.right * touchpad.x * FORWARD_SENSITIVITY);
                //player.transform.position -= player.transform.right * Time.deltaTime * (touchpad.x * STRAFE_SENSITIVITY);
            }
            Debug.Log(gameObject.name + Controller.GetAxis());
        }
    }
}