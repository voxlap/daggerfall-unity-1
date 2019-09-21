using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.UserInterface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(UserInterfaceRenderTarget))]
public class CustomVitals : MonoBehaviour
{
    UserInterfaceRenderTarget ui;
    HUDVitals vitals;

    private void Start() {
        // Setup offscreen UI - compass frame is 69x17 pixels
        ui = GetComponent<UserInterfaceRenderTarget>();

        // Create HUD compass and add to offscreen UI parent panel
        vitals = new HUDVitals();
        ui.CustomWidth = 205;
        ui.CustomHeight = 350;
        ui.ParentPanel.Components.Add(vitals);
    }

    private void Update()
    {
        // Scale vitals to parent
        vitals.Scale = ui.ParentPanel.LocalScale;
    }
}
