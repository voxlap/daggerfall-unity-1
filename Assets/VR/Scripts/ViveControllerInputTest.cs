using DaggerfallWorkshop.Game;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class ViveControllerInputTest : MonoBehaviour
{
    public GameObject LaserPrefab;
    private GameObject laser;
    private Transform laserTransform;
    private Vector3 hitPoint;
    private SteamVR_Action_Boolean interactUI = SteamVR_Input.GetBooleanAction("InteractUI");
    private SteamVR_Action_Boolean openMenu = SteamVR_Input.GetBooleanAction("OpenMenu");

    private bool InteractUIPressedDown { get { return interactUI.GetStateDown(trackedObj.handType); } }
    private bool OpenMenuPressed { get { return openMenu.GetState(trackedObj.handType); } }

    // Use this for initialization
    void Start()
    {
        laser = Instantiate(LaserPrefab);
        laserTransform = laser.transform;
    }

    private Hand trackedObj;

    private void Awake()
    {
        trackedObj = GetComponent<Hand>();
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
        RaycastHit hit;
        if (Physics.Raycast(trackedObj.transform.position, transform.forward, out hit, 100))
        {
            hitPoint = hit.point;
            ShowLaser(hit);
            FloatingUITest test;
            if (test = hit.transform.GetComponent<FloatingUITest>())
            {
                Vector3 localPoint = hit.transform.GetComponent<RawImage>().rectTransform.InverseTransformPoint(hit.point);
                test.HandlePointer(localPoint);
            }
        }
        else
            laser.SetActive(false);

        if (InteractUIPressedDown)
        {
            InputManager.Instance.AddAction(InputManager.Actions.ActivateCenterObject);
            Debug.Log(gameObject.name + " UI clicked with VR");
        }
	}
}
