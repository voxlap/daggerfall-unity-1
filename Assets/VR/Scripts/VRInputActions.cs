// Project:         Daggerfall Tools For Unity
// Copyright:       Copyright (C) 2009-2019 Daggerfall Workshop
// Web Site:        http://www.dfworkshop.net
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/Interkarma/daggerfall-unity
// Original Author: Vincent Wing (vincentwing00@gmail.com)
// Contributors:    
// 
// Notes:
//

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class VRInputActions
{
    private static SteamVR_Action_Boolean grabGrip = null;
    private static SteamVR_Action_Boolean grabPinch = null;
    private static SteamVR_Action_Boolean openMenu = null;
    private static SteamVR_Action_Boolean selectWorldObject = null;
    private static SteamVR_Action_Boolean activateWalk = null;
    private static SteamVR_Action_Boolean activateRotate = null;
    private static SteamVR_Action_Boolean interactUI = null;
    private static SteamVR_Action_Vector2 rotate = null;
    private static SteamVR_Action_Vector2 walk = null;

    public static SteamVR_Action_Boolean GrabGripAction
    {
        get { return (grabGrip != null) ? grabGrip : grabGrip = SteamVR_Input.GetBooleanAction("GrabGrip", true); }
        set { grabGrip = value; }
    }
    public static SteamVR_Action_Boolean GrabPinchAction
    {
        get { return (grabPinch != null) ? grabPinch : grabPinch = SteamVR_Input.GetBooleanAction("GrabPinch", true); }
        set { grabPinch = value; }
    }
    public static SteamVR_Action_Boolean OpenMenuAction
    {
        get { return (openMenu != null) ? openMenu : openMenu = SteamVR_Input.GetBooleanAction("OpenMenu", true); }
        set { openMenu = value; }
    }
    public static SteamVR_Action_Boolean InteractUIAction
    {
        get { return (interactUI != null) ? interactUI : interactUI = SteamVR_Input.GetBooleanAction("InteractUI", true); }
        set { interactUI = value; }
    }
    public static SteamVR_Action_Boolean SelectWorldObjectAction
    {
        get { return (selectWorldObject != null) ? selectWorldObject : selectWorldObject = SteamVR_Input.GetBooleanAction("SelectWorldObject", true); }
        set { selectWorldObject = value; }
    }
    public static SteamVR_Action_Boolean ActivateRotate
    {
        get { return (activateRotate != null) ? activateRotate : activateRotate = SteamVR_Input.GetBooleanAction("ActivateRotate", true); }
        set { activateRotate = value; }
    }
    public static SteamVR_Action_Boolean ActivateWalk
    {
        get { return (activateWalk != null) ? activateWalk : activateWalk = SteamVR_Input.GetBooleanAction("ActivateWalk", true); }
        set { activateWalk = value; }
    }
    public static SteamVR_Action_Vector2 RotateAction
    {
        get { return (rotate != null) ? rotate : rotate = SteamVR_Input.GetVector2Action("Rotate", true); }
        set { rotate = value; }
    }
    public static SteamVR_Action_Vector2 WalkAction
    {
        get { return (walk != null) ? walk : walk = SteamVR_Input.GetVector2Action("Walk", true); }
        set { walk = value; }
    }
}
