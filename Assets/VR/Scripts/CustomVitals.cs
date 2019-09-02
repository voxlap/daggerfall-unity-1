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
        vitals.Enabled = true;
        vitals.Update();
        ui.ParentPanel.Components.Add(vitals);
        ui.OutputImage = GetComponent<RawImage>();


        PlayerEntity entity = GameManager.Instance.PlayerEntity;
        vitals.Health = entity.CurrentHealthPercent;
        vitals.Fatigue = (entity.CurrentFatigue / entity.MaxFatigue);
        vitals.Magicka = (entity.CurrentMagicka / entity.MaxMagicka);
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
    private void OnEnable()
    {
        if (vitals != null)
        {
            vitals.Enabled = true;
            vitals.Update();
        }
    }
    private void OnDisable()
    {
        if (vitals != null)
            vitals.Enabled = false;
    }
}
