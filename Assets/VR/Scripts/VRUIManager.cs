using DaggerfallWorkshop.Game;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DaggerfallWorkshop.Game.UserInterface;

/** * This component disables the default UI and replaces it with VR logic
 **/
public class VRUIManager : MonoBehaviour
{
    public const string UI_LAYER_NAME = "UI";
    public const string PLAYER_LAYER_NAME = "Player";

    public GameObject FloatingUIPrefab;
    public GameObject floatingUI;
    public GameObject FollowingUIPrefab;
    
    private Camera mainCamera { get { return GameManager.Instance.MainCamera; } }
    private GameObject hint;
    private GameObject followingUI;
    

    [Tooltip("The VR Hint is an object that'll be positioned over an object that's being pointed at, to indicate to the user that it can be interacted with")]
    public GameObject HintPrefab;

    private IUserInterfaceManager uiManager { get { return DaggerfallUI.UIManager; } }

    // Used for enabling/disabling the floating UI
    private int cachedMask = 0;
    // used to hide game world when floating UI is up
    private int playerLayerMask;
    private int uiLayerMask;

    #region Singleton

    public static VRUIManager Instance { get; private set; }

    private void SetupSingleton()
    {
        if (!Instance)
            Instance = this;
        else
        {
            Debug.LogError("2nd instance of VRUIManager singleton spawned. There should only be one.");
        }
    }

    #endregion

    private void Awake()
    {
        SetupSingleton();
        playerLayerMask = LayerMask.GetMask(PLAYER_LAYER_NAME);
        uiLayerMask = LayerMask.GetMask(UI_LAYER_NAME);
    }

    void Start()
    {
        if (FloatingUIPrefab) {
            floatingUI = Instantiate(FloatingUIPrefab);
        } else {
            Debug.LogError("The VR UI Manager was unable to create the floating UI! The VR UI will be very broken.");
            return;
        }

        if (FollowingUIPrefab) {
            followingUI = Instantiate(FollowingUIPrefab);
        } else {
            Debug.LogError("The VR UI Manager was unable to create the Following UI! The VR UI will be somewhat broken.");
            return;
        }
        

        if (HintPrefab) {
            hint = Instantiate(HintPrefab);
        } else {
            Debug.LogError("The VR UI Manager didn't find a Hint prefab set. Hinting will be broken, but this isn't a huge deal.");
        }
        
        mainCamera.backgroundColor = new Color(.1f, .1f, .1f);
        cachedMask = mainCamera.cullingMask;
        stickFloatingUIInFrontOfPlayer();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        floatingUI.SetActive(false);

        followingUI.GetComponent<FollowingUI>().whatToFollow = mainCamera.gameObject;
        //subscribe to window change event
        uiManager.OnWindowChange += UIManager_OnWindowChange;
        cachedMask = mainCamera.cullingMask;
    }
    private void OnDestroy()
    {
        uiManager.OnWindowChange -= UIManager_OnWindowChange;
    }

    private void UIManager_OnWindowChange(object sender, System.EventArgs e)
    {
        int windowCount = uiManager.WindowCount;
        if (windowCount > 0)
        {
            // Window count increased--display the UI!
            floatingUI.SetActive(true);
            mainCamera.cullingMask = uiLayerMask | playerLayerMask;
            stickFloatingUIInFrontOfPlayer();
        }
        else if (windowCount <= 0)
        {
            // Window count decreased--disable the UI
            floatingUI.SetActive(false);
            mainCamera.cullingMask = cachedMask;
        }
    }

    public void repositionHint(Vector3 position, Vector3 width_height_depth, Quaternion rotation) {
        if (hint)
        {
            hint.gameObject.SetActive(true);
            hint.transform.localScale = width_height_depth;
            hint.transform.position = position;
            hint.transform.rotation = rotation;
        }
    }
    public void HideHint()
    {
        if(hint)
            hint.gameObject.SetActive(false);
    }

    void stickFloatingUIInFrontOfPlayer() {
        if (!floatingUI || !mainCamera) return;
        //set position
        Vector3 floatPos = mainCamera.transform.position + (mainCamera.transform.forward * 3f);
        floatPos.y = mainCamera.transform.position.y;
        floatingUI.transform.position = floatPos;
        //set rotation
        Vector3 lookPos = mainCamera.transform.position;
        lookPos.y = floatingUI.transform.position.y;
        floatingUI.transform.LookAt(lookPos);
        floatingUI.transform.Rotate(Vector3.up, 180);
    }
}


//private bool stuckUI = false;
//   private bool lastPauseState = false;
//   private int skipFrame = 0;
//void Update () {
//if (skipFrame++ < 30) return;
//skipFrame = 0;

//int currentWindowCount = DaggerfallUI.Instance.UserInterfaceManager.WindowCount;
//lastWindowCount = currentWindowCount;

/*
bool currentPauseState = InputManager.Instance.IsPaused;
if (lastPauseState != currentPauseState) {
if (!lastPauseState && currentPauseState) {
    floatingUI.SetActive(true);
    cachedMask = actualCamera.cullingMask;
    actualCamera.cullingMask = (1 << LayerMask.NameToLayer(UI_LAYER_MASK_NAME));
    stickFloatingUIInFrontOfPlayer();
} else if (lastPauseState && !currentPauseState) {
    //floatingUI.SetActive(false);
    actualCamera.cullingMask = cachedMask;
}
lastPauseState = currentPauseState;
}
*/
//}