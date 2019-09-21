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

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum VRTurningMode { Smooth, Snap }
public enum VRMovementMode { Head, Controller }

public class VRSettingsMenu : FloatingUI
{
    [Header("VRSettings References")]
    public Dropdown turningModeDropdown;
    public Dropdown smoothTurningSpeedDropdown;
    public Dropdown snapTurningSpeedDropdown;
    public Dropdown movementDirectionDropdown;

    public float SmoothTurningSpeed { get { return float.Parse(smoothTurningSpeedDropdown.options[SavedSmoothTurningValue].text); } }
    public float SnapTurningSpeed { get { return float.Parse(snapTurningSpeedDropdown.options[SavedSmoothTurningValue].text); } }
    public VRTurningMode TurningMode { get { return (VRTurningMode)SavedTurningModeValue; } }
    public VRMovementMode MovementMode { get { return (VRMovementMode)SavedMovementModeValue; } }

    public static event Action<VRMovementMode> MovementModeChanged;
    public static event Action<VRTurningMode> TurningModeChanged;
    public static event Action<float> SmoothTurningSpeedChanged;
    public static event Action<float> SnapTurningSpeedChanged;

    #region Player Prefs

    private const string k_pprefSuffix = "VRSettingsMenu.";

    private int SavedTurningModeValue
    {
        get { return PlayerPrefs.GetInt(k_pprefSuffix + "SavedTurningMode", 0); }
        set { PlayerPrefs.SetInt(k_pprefSuffix + "SavedTurningMode", value); }
    }
    private int SavedMovementModeValue
    {
        get { return PlayerPrefs.GetInt(k_pprefSuffix + "SavedMovementModeValue", 0); }
        set { PlayerPrefs.SetInt(k_pprefSuffix + "SavedMovementModeValue", value); }
    }
    private int SavedSmoothTurningValue
    {
        get { return PlayerPrefs.GetInt(k_pprefSuffix + "SavedSmoothTurningValue", smoothTurningSpeedDropdown.value); }
        set { PlayerPrefs.SetInt(k_pprefSuffix + "SavedSmoothTurningValue", value); }
    }
    private int SavedSnapTurningValue
    {
        get { return PlayerPrefs.GetInt(k_pprefSuffix + "SavedSmoothTurningValue", snapTurningSpeedDropdown.value); }
        set { PlayerPrefs.SetInt(k_pprefSuffix + "SavedSmoothTurningValue", value); }
    }
    #endregion

    #region Singleton

    public static VRSettingsMenu Instance { get; private set; }

    void SetupSingleton()
    {
        if (Instance)
            Debug.LogError("There's more than one instance of VRSettingsMenu. This obviously shouldn't happen.");
        else
            Instance = this;
    }

    #endregion

    private void Awake()
    {
        SetupSingleton();
    }

    protected override void Start()
    {
        base.Start();

        //set up dropdowns
        turningModeDropdown.ClearOptions();
        turningModeDropdown.AddOptions(new List<string>(System.Enum.GetNames(typeof(VRTurningMode))));
        movementDirectionDropdown.ClearOptions();
        movementDirectionDropdown.AddOptions(new List<string>(Enum.GetNames(typeof(VRMovementMode))));

        //add unity UI listeners
        turningModeDropdown.onValueChanged.AddListener(OnTurningModeDropdownChanged);
        smoothTurningSpeedDropdown.onValueChanged.AddListener(OnSmoothTurningSpeedDropdownChanged);
        snapTurningSpeedDropdown.onValueChanged.AddListener(OnSnapTurningSpeedDropdownChanged);
        movementDirectionDropdown.onValueChanged.AddListener(OnMovementModeDropdownChanged);

        //select saved values
        movementDirectionDropdown.value = SavedMovementModeValue;
        turningModeDropdown.value = SavedTurningModeValue;
        smoothTurningSpeedDropdown.value = SavedSmoothTurningValue;
        snapTurningSpeedDropdown.value = SavedSnapTurningValue;
    }

    private void OnMovementModeDropdownChanged(int value)
    {
        VRMovementMode selection = (VRMovementMode)value;
        SavedMovementModeValue = value;
        if (MovementModeChanged != null)
            MovementModeChanged(selection);
    }

    private void OnTurningModeDropdownChanged(int value)
    {
        VRTurningMode selection = (VRTurningMode)value;
        switch (selection)
        {
            case VRTurningMode.Smooth:
                smoothTurningSpeedDropdown.gameObject.SetActive(true);
                snapTurningSpeedDropdown.gameObject.SetActive(false);
                break;
            case VRTurningMode.Snap:
                smoothTurningSpeedDropdown.gameObject.SetActive(false);
                snapTurningSpeedDropdown.gameObject.SetActive(true);
                break;
            default:
                string reason;
                if (value > Enum.GetNames(typeof(VRTurningMode)).Length || value < 0)
                    reason = "value is out of range of VRTurningMode enum.";
                else
                    reason = "the " + (VRTurningMode)value + " case has not yet being handled.";

                Debug.LogError("Did not recognize value " + value + " for this VR turning mode because " + reason);
                return; //return early, invalid value
        }
        SavedTurningModeValue = value;
        if (TurningModeChanged != null)
            TurningModeChanged((VRTurningMode)value);
    }

    private void OnSmoothTurningSpeedDropdownChanged(int value)
    {
        string speedStr = smoothTurningSpeedDropdown.options[value].text;
        float speed;
        if(float.TryParse(speedStr, out speed))
        {
            SavedSmoothTurningValue = value;
            if (SmoothTurningSpeedChanged != null)
                SmoothTurningSpeedChanged(value);
        }
    }

    private void OnSnapTurningSpeedDropdownChanged(int value)
    {
        string speedStr = snapTurningSpeedDropdown.options[value].text;
        float speed;
        if (float.TryParse(speedStr, out speed))
        {
            SavedSnapTurningValue = value;
            if (SnapTurningSpeedChanged != null)
                SnapTurningSpeedChanged(value);
        }
    }
}
