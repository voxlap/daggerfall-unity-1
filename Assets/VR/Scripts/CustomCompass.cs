using DaggerfallWorkshop.Game.UserInterface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UserInterfaceRenderTarget))]
public class CustomCompass : MonoBehaviour
{
    UserInterfaceRenderTarget ui;
    HUDCompass compass;

    private void Start()
    {
        // Setup offscreen UI - compass frame is 69x17 pixels
        ui = GetComponent<UserInterfaceRenderTarget>();
        ui.CustomWidth = 69;
        ui.CustomHeight = 17;

        // Create HUD compass and add to offscreen UI parent panel
        compass = new HUDCompass();
        ui.ParentPanel.Components.Add(compass);
    }

    private void Update()
    {
        // Scale compass to parent
        compass.Scale = ui.ParentPanel.LocalScale;
    }
}
