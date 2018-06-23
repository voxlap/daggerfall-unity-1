using DaggerfallWorkshop.Game;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ViveControllerInputTest : MonoBehaviour
{
    [Tooltip("This should be set to the name of the UI component that contains a User Interface Render Target script.")]
    public string FLOATING_UI_TARGET_NAME = "FloatingUI";

    public GameObject LaserPrefab;
    private GameObject laser;
    private Transform laserTransform;
    private Vector3 hitPoint;

    // Use this for initialization
    void Start()
    {
        laser = Instantiate(LaserPrefab);
        laserTransform = laser.transform;
    }

    private SteamVR_TrackedObject trackedObj;

    private SteamVR_Controller.Device Controller
    { 
        get { return SteamVR_Controller.Input((int)trackedObj.index); }
    }

    private void Awake()
    {
        trackedObj = GetComponent<SteamVR_TrackedObject>();
    }

    private void ShowLaser(RaycastHit hit)
    {
        laser.SetActive(true);
        laserTransform.position = Vector3.Lerp(trackedObj.transform.position, hitPoint, 0.5f);
        laserTransform.LookAt(hitPoint);
        laserTransform.localScale = new Vector3(laserTransform.localScale.x, laserTransform.localScale.y, hit.distance);
    }

    // Update is called once per frame
    void Update() { 
        if (Controller.GetPress(SteamVR_Controller.ButtonMask.Touchpad)) {
            RaycastHit hit;
            if (Physics.Raycast(trackedObj.transform.position, transform.forward, out hit, 100)) {
                hitPoint = hit.point;
                ShowLaser(hit);
                if (hit.transform.gameObject.name == FLOATING_UI_TARGET_NAME + "(Clone)") {
                    //Debug.Log("World position: " + hit.point.ToString());
                    //Vector3 localPoint = hit.transform.InverseTransformPoint(hit.point);
                    Vector3 localPoint = hit.transform.gameObject.GetComponent<RawImage>().rectTransform.InverseTransformPoint(hit.point);
                    //Debug.Log("Inverse Transform Point: " + localPoint);
                    FloatingUITest test = hit.transform.gameObject.GetComponent<FloatingUITest>();
                    if (test) {
                        test.HandlePointer(localPoint);
                    }


                }
            }
        }
        else
        {
            laser.SetActive(false);
        }

        if (Controller.GetHairTriggerDown()) {
            InputManager.Instance.AddAction(InputManager.Actions.ActivateCenterObject);
            Debug.Log(gameObject.name + " Trigger pressed");
        }

/*        if (Controller.GetHairTriggerDown())
        if (Controller.GetAxis() != Vector2.zero)
        {
            Debug.Log(gameObject.name + Controller.GetAxis());
        }

        if (Controller.GetHairTriggerDown())
        {
            Debug.Log(gameObject.name + " Trigger pressed");
        }

        if (Controller.GetHairTriggerUp())
        {
            Debug.Log(gameObject.name + " Trigger released");
        }

        if (Controller.GetPressDown(SteamVR_Controller.ButtonMask.Grip))
        {
            Debug.Log(gameObject.name + " Grip pressed");
        }

        if (Controller.GetPressUp(SteamVR_Controller.ButtonMask.Grip))
        {
            Debug.Log(gameObject.name + " Grip released");
        }
        */
	}
}
