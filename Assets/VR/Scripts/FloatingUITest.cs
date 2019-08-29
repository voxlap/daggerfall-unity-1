using UnityEngine;
using UnityEngine.UI;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.UserInterface;

[RequireComponent(typeof(UserInterfaceRenderTarget))]
public class FloatingUITest : MonoBehaviour
{
    public GameObject DebugIndicatorPrefab;
    private GameObject debugIndicator;

    UserInterfaceRenderTarget ui;
    Vector2 virtualMousePos = Vector2.zero;
    RawImage rawImage;
    Canvas canvas;
    Vector2 offscreenMouse = new Vector2(-1, -1);

    private void Start()
    {
        // Redirect main UI stack to our custom target and disable HUD
        ui = GetComponent<UserInterfaceRenderTarget>();
        DaggerfallUI.Instance.CustomRenderTarget = ui;
        DaggerfallUI.Instance.enableHUD = false;

        // Get references
        rawImage = GetComponent<RawImage>();
        canvas = GetComponent<Canvas>();

        if (DebugIndicatorPrefab) {
            debugIndicator = Instantiate(DebugIndicatorPrefab);
            debugIndicator.transform.localPosition.Set(0, 0, 10);
            debugIndicator.transform.parent = gameObject.transform;
        } else {
            Debug.LogError("Unable to create a debug indicator in the floating VR UI.");
        }
    }


    private void Update()
    {
    }

    public void HandlePointer(Vector3 point)
    {
        // Do nothing unless game is paused - this can happen at any time either through user input or a quest message popping up text
        // When not active, we set custom mouse position to null to release any custom position set from a prior open UI session
        // Also disabling raw image when not required here - you would manage your own output canvas in VR as needed
        if (!GameManager.IsGamePaused)
        {
            rawImage.enabled = false;
            DaggerfallUI.Instance.CustomMousePosition = null;
            return;
        }

        // Show the raw image - in VR you would bring up your diegetic output panel in front of player
        rawImage.enabled = true;

        // Setting mouse offscreen unless can resolve position below
        virtualMousePos = offscreenMouse;

        // Get rect of rawimage
        Rect rect = RectTransformUtility.PixelAdjustRect(rawImage.rectTransform, canvas);
        //Debug.Log("rawImage Rect is: " + rect + ". Pos: " + rect.position);

        // Is screen position inside rectTransform? Here you would use your own means of firing a ray at target canvas from controller
        //if (RectTransformUtility.RectangleContainsScreenPoint(rawImage.rectTransform, point, GameManager.Instance.MainCamera))
        if (rect.Contains(point))
        {
            //Debug.Log(point + " is inside!");
            // Get local point inside canvas
            virtualMousePos = new Vector2(point.x, point.y);

            /*if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rawImage.rectTransform, Input.mousePosition, GameManager.Instance.MainCamera, out localPoint))
            {
                // Convert to UV coordinates inside tranform area using rect - v is raised into the 0-1 domain and inverted for 0 to be top-left
                float u = localPoint.x / rect.width + 0.5f;
                float v = 1.0f - (localPoint.y / rect.height + 0.5f);

                // We know size of render target so we can convert this into x, y coordinates
                float x = u * ui.TargetSize.x;
                float y = v * ui.TargetSize.y;

                // Set virtual mouse position into UI system
                virtualMousePos = new Vector2(x, y);
            }
            */
            float u = point.x / rect.width + 0.5f;
            float v = 1.0f - (point.y / rect.height + 0.5f);

            // We know size of render target so we can convert this into x, y coordinates
            float x = u * ui.TargetSize.x;
            float y = v * ui.TargetSize.y;

            // Set virtual mouse position into UI system
            virtualMousePos = new Vector2(x, y);
            repositionDebugIcon(point);
        }

        // Feed custom mouse position into UI system
        DaggerfallUI.Instance.CustomMousePosition = virtualMousePos;
    }

    private void repositionDebugIcon(Vector2 point) {
        debugIndicator.transform.localPosition = point;
        debugIndicator.transform.Translate(0f, -0.2f, -0.1f);
        debugIndicator.transform.localRotation = Quaternion.identity;
        debugIndicator.transform.Rotate(0, 0, 90f);
    }
}

