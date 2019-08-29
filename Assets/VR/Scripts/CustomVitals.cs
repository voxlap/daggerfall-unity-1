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
        ui.CustomWidth = 160;
        ui.CustomHeight = 150;

        // Create HUD compass and add to offscreen UI parent panel
        vitals = new HUDVitals();
        ui.ParentPanel.Components.Add(vitals);
        ui.OutputImage = GetComponent<RawImage>();


        PlayerEntity entity = GameManager.Instance.PlayerEntity;
        /*
        entity.CurrentHealth;
        entity.CurrentFatigue;
        entity.CurrentMagicka;
        */
    }

    private void Update() {
        // Scale vitals to parent
        vitals.Scale = ui.ParentPanel.LocalScale;
    }
}
