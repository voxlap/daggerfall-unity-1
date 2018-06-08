using DaggerfallWorkshop.Game;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ViveControllerInputTest : MonoBehaviour {

    // Use this for initialization
    void Start() {

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

    // Update is called once per frame
    void Update ()
    {
        if (Controller.GetHairTriggerDown())
        {
            //InputManager.Instance.addAction(InputManager.Actions.ActivateCenterObject);
            Debug.Log(gameObject.name + " Trigger pressed");
            DaggerfallUI.Instance.EnableDefaultUserInterface = false;
        }
/*        if (Controller.GetAxis() != Vector2.zero)
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
