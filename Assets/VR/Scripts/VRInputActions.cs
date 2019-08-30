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
    private static SteamVR_Action_Boolean pointing = null;
    private static SteamVR_Action_Boolean openMenu = null;
    private static SteamVR_Action_Boolean selectWorldObject = null;
    private static SteamVR_Action_Vector2 rotate = null;
    private static SteamVR_Action_Vector2 walk = null;
    
    public static SteamVR_Action_Boolean GrabGripAction
    {
        get { return (pointing != null) ? pointing : pointing = SteamVR_Input.GetBooleanAction("GrabGrip"); }
        set { pointing = value; }
    }
    public static SteamVR_Action_Boolean OpenMenuAction
    {
        get { return (openMenu != null) ? openMenu : openMenu = SteamVR_Input.GetBooleanAction("OpenMenu"); }
        set { openMenu = value; }
    }
    public static SteamVR_Action_Boolean SelectWorldObjectAction
    {
        get { return (selectWorldObject != null) ? selectWorldObject : selectWorldObject = SteamVR_Input.GetBooleanAction("SelectWorldObject"); }
        set { selectWorldObject = value; }
    }
    public static SteamVR_Action_Vector2 RotateAction
    {
        get { return (rotate != null) ? rotate : rotate = SteamVR_Input.GetVector2Action("Rotate"); }
        set { rotate = value; }
    }
    public static SteamVR_Action_Vector2 WalkAction
    {
        get { return (walk != null) ? walk : walk = SteamVR_Input.GetVector2Action("Walk"); }
        set { walk = value; }
    }
}
