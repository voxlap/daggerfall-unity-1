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
    private bool wasUIPressed = false;
    private SteamVR_Action_Boolean interactUI = SteamVR_Input.GetBooleanAction("InteractUI");
    private SteamVR_Action_Boolean openMenu = SteamVR_Input.GetBooleanAction("OpenMenu");

    private VRController myController;
    
    private bool InteractUIPressed { get { return interactUI.GetState(myController.VRHand.handType); } }
    private bool OpenMenuPressed { get { return openMenu.GetState(myController.VRHand.handType); } }
    
    void Start()
    {
        laser = Instantiate(LaserPrefab);
    }

    private void Awake()
    {
        myController = GetComponent<VRController>();
    }
    private void OnDisable()
    {
        if (wasUIPressed)
            SetMousePressed(false);
    }
    void Update()
    {
        HandleLaser();
    }

    private void ShowLaser(RaycastHit hit)
    {
        laser.SetActive(true);
        laser.transform.position = Vector3.Lerp(myController.transform.position, hit.point, 0.5f);
        laser.transform.LookAt(hit.point);
        laser.transform.localScale = new Vector3(laser.transform.localScale.x, laser.transform.localScale.y, hit.distance);
    }
    
    private void HandleLaser()
    {
        //set activity of laser and position of UI pointer
        RaycastHit hit;
        if (Physics.Raycast(myController.transform.position, transform.forward, out hit, 100, LayerMask.GetMask("UI")))
        {
            ShowLaser(hit);
            FloatingDaggerfallUI floatingUI;
            if (floatingUI = hit.transform.GetComponent<FloatingDaggerfallUI>())
            {
                Vector3 localPoint = hit.transform.GetComponent<RectTransform>().InverseTransformPoint(hit.point);
                floatingUI.HandlePointer(localPoint);
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
