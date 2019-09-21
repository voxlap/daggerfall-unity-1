// Project:         Daggerfall Tools For Unity
// Copyright:       Copyright (C) 2009-2019 Daggerfall Workshop
// Web Site:        http://www.dfworkshop.net
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/Interkarma/daggerfall-unity
// Original Author: InconsolableCellist
// Contributors: Vincent Wing (vincentwing00@gmail.com)
// 
// Notes:
//

using UnityEngine;
using UnityEngine.UI;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.UserInterface;

public class FloatingDaggerfallUI : FloatingUI
{
    private UserInterfaceRenderTarget ui;
    private DaggerfallUI daggerfallUI;

    protected override void Start()
    {
        base.Start();

        //get references
        daggerfallUI = DaggerfallUI.Instance;
        ui = GetComponentInChildren<UserInterfaceRenderTarget>();

        // Redirect main UI stack to our custom target
        daggerfallUI.CustomRenderTarget = ui;
        //disable HUD
        //...
    }

    protected override void SetCursorActive(bool active)
    {
        base.SetCursorActive(active);
        if(!active)
            daggerfallUI.CustomMousePosition = null;
    }

    private void OnEnable()
    {
        if(_init)
            ResizeCanvas(ui.TargetSize);
    }

    private void OnDisable()
    {
        // When not active, we set custom mouse position to null to release any custom position set from a prior open UI session
        if(daggerfallUI)
            daggerfallUI.CustomMousePosition = null;
    }

    public override void HandlePointer(Vector3 point)
    {
        base.HandlePointer(point);

        if (!_init)
            return;
        
        // Setting mouse offscreen unless can resolve position below
        virtualMousePos = offscreenMouse;

        // Get rect of rawimage
        Rect rect = RectTransformUtility.PixelAdjustRect(rawImage.rectTransform, canvas);

        // Is screen position inside rectTransform? Here you would use your own means of firing a ray at target canvas from controller
        if (rect.Contains(point))
        {
            // Get local point inside canvas
            virtualMousePos = new Vector2(point.x, point.y);

            float u = point.x / rect.width + 0.5f;
            float v = 1.0f - (point.y / rect.height + 0.5f);

            // We know size of render target so we can convert this into x, y coordinates
            float x = u * ui.TargetSize.x;
            float y = v * ui.TargetSize.y;

            // Set virtual mouse position into UI system
            virtualMousePos = new Vector2(x, y);
        }

        // Feed custom mouse position into UI system
        DaggerfallUI.Instance.CustomMousePosition = virtualMousePos;
    }

    public override void HandleClick(Vector3 point)
    {
        //this is handled in VRController.SetMousePressed, instead, for the daggerfall kind of UI
    }
    public override void HandleUnclick(Vector3 point)
    {
        //this is handled in VRController.SetMousePressed, instead, for the daggerfall kind of UI
    }
}

