using DaggerfallWorkshop.Game;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Valve.VR;
using Valve.VR.InteractionSystem;
using DaggerfallWorkshop.Game.UserInterface;

public class ViveControllerInputTest : MonoBehaviour
{
    public GameObject LaserPrefab;
    private GameObject laser;
    private Transform laserTransform;
    private Vector3 hitPoint;
    private bool wasUIPressed = false;
    private SteamVR_Action_Boolean interactUI = SteamVR_Input.GetBooleanAction("InteractUI");
    private SteamVR_Action_Boolean openMenu = SteamVR_Input.GetBooleanAction("OpenMenu");

    private Hand myHand;


    private bool InteractUIPressed { get { return interactUI.GetState(myHand.handType); } }
    private bool OpenMenuPressed { get { return openMenu.GetState(myHand.handType); } }
    
    void Start()
    {
        laser = Instantiate(LaserPrefab);
        laserTransform = laser.transform;
    }

    private void Awake()
    {
        myHand = GetComponent<Hand>();
    }

    private void OnDisable()
    {
        if (wasUIPressed)
            SetMousePressed(false);
    }

    private void ShowLaser(RaycastHit hit)
    {
        laser.SetActive(true);
        laserTransform.position = Vector3.Lerp(myHand.transform.position, hitPoint, 0.5f);
        laserTransform.LookAt(hitPoint);
        laserTransform.localScale = new Vector3(laserTransform.localScale.x, laserTransform.localScale.y, hit.distance);
    }
    
    void Update()
    {
        //set activity of laser and position of UI pointer
        RaycastHit hit;
        if (Physics.Raycast(myHand.transform.position, transform.forward, out hit, 100, LayerMask.GetMask("UI")))
        {
            hitPoint = hit.point;
            ShowLaser(hit);
            FloatingUITest test;
            if (test = hit.transform.GetComponent<FloatingUITest>())
            {
                Vector3 localPoint = hit.transform.GetComponent<RectTransform>().InverseTransformPoint(hit.point);
                test.HandlePointer(localPoint);
            }
        }
        else
            laser.SetActive(false);

        //set custom mouse state for ui mouse down events
        SetMousePressed(InteractUIPressed);
	}

    private void SetMousePressed(bool isPressed)
    {
        DaggerfallInput.SetMouseButton(0, isPressed);
        wasUIPressed = isPressed;
    }
}
