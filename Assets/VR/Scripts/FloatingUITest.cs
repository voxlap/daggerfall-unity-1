using UnityEngine;
using UnityEngine.UI;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.UserInterface;

[RequireComponent(typeof(UserInterfaceRenderTarget))]
public class FloatingUITest : MonoBehaviour
{
    public RawImage cursor;
    public string cursorResourcePath = "Cursor";

    private UserInterfaceRenderTarget ui;
    private Vector2 virtualMousePos = Vector2.zero;
    private RawImage rawImage;
    private Canvas canvas;
    private Vector2 offscreenMouse = new Vector2(-1, -1);
    private bool _init = false;

    private void Start()
    {
        // Redirect main UI stack to our custom target and disable HUD
        ui = GetComponentInChildren<UserInterfaceRenderTarget>();
        DaggerfallUI.Instance.CustomRenderTarget = ui;

        // Get references
        rawImage = GetComponentInChildren<RawImage>();
        canvas = GetComponent<Canvas>();

        if (cursor && !string.IsNullOrEmpty(cursorResourcePath)) {
            cursor.texture = Resources.Load<Texture2D>(cursorResourcePath);
        } else {
            Debug.LogError("Unable to create a cursor in the floating VR UI.");
        }
        _init = true;
    }
    
    public void HandlePointer(Vector3 point)
    {
        if (!_init)
            return;
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

            float u = point.x / rect.width + 0.5f;
            float v = 1.0f - (point.y / rect.height + 0.5f);

            // We know size of render target so we can convert this into x, y coordinates
            float x = u * ui.TargetSize.x;
            float y = v * ui.TargetSize.y;

            // Set virtual mouse position into UI system
            virtualMousePos = new Vector2(x, y);
            RepositionCursor(point);
        }

        // Feed custom mouse position into UI system
        DaggerfallUI.Instance.CustomMousePosition = virtualMousePos;
    }

    private void RepositionCursor(Vector2 point) {
        cursor.transform.localPosition = point;
    }
}

