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

using UnityEngine;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace DaggerfallWorkshop.Game.UserInterface
{
    /// <summary>
    /// Has some of the same functionality as UnityEngine's Input class, but allows for setting custom states.
    /// </summary>
    public class DaggerfallInput
    {
        private const int NUM_MOUSE_BUTTONS = 7; //there are 7 mouse inputs in the Unity KeyCodes. MouseDown0 to MouseDown6
        private static bool[] customMouseIsDown = new bool[NUM_MOUSE_BUTTONS];
        private static bool[] customMouseWasDown = new bool[NUM_MOUSE_BUTTONS];
        private static int[] lastFrameCustomMouseStateWasSet = new int[NUM_MOUSE_BUTTONS];

        //get custom and actual mouse button states
        public static bool GetMouseButtonUp(int button)
        {
            return Input.GetMouseButtonUp(button) || (button < NUM_MOUSE_BUTTONS && !customMouseIsDown[button] && customMouseWasDown[button]);
        }
        public static bool GetMouseButtonDown(int button)
        {
            return Input.GetMouseButtonDown(button) || (button < NUM_MOUSE_BUTTONS && customMouseIsDown[button] && !customMouseWasDown[button]);
        }
        public static bool GetMouseButton(int button)
        {
            return Input.GetMouseButton(button) || (button < NUM_MOUSE_BUTTONS && customMouseIsDown[button]);
        }

        //set custom mouse button states
        public static void SetMouseButton(int button, bool isDown)
        {
            //don't update mouseWasDown state multiple times in one frame
            if (lastFrameCustomMouseStateWasSet[button] != Time.frameCount)
                customMouseWasDown[button] = customMouseIsDown[button];
            //set mouse state for this button
            customMouseIsDown[button] = isDown;
            //set lastFrame for this button to this frame
            lastFrameCustomMouseStateWasSet[button] = Time.frameCount;
        }
    }
}