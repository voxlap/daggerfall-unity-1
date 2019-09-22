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
using UnityEngine.UI;
using DaggerfallWorkshop.Game;

public class VRPauseMenu : MonoBehaviour
{
    public FloatingDaggerfallUI daggerfallUI;
    public FloatingUI selectPageUI;
    public FloatingUI vrOptionsUI;

    public Button openSaveLoadMenuButton;
    public Button openCharacterSheetButton;
    public Button openMapButton;
    public Button openVRSettingsButton;

    private bool _init;

    private void Start()
    {
        openSaveLoadMenuButton.onClick.AddListener(OpenSaveLoadMenu);
        openCharacterSheetButton.onClick.AddListener(OpenCharacterSheetMenu);
        openMapButton.onClick.AddListener(OpenMapMenu);
        openVRSettingsButton.onClick.AddListener(OpenVROptions);
        CloseVROptions();
        _init = true;
    }
    private void OnEnable()
    {
        if(_init)
            CloseVROptions();
    }

    public void CloseVROptions()
    {
        vrOptionsUI.gameObject.SetActive(false);
        daggerfallUI.gameObject.SetActive(true);
    }
    public void OpenVROptions()
    {
        vrOptionsUI.gameObject.SetActive(true);
        daggerfallUI.gameObject.SetActive(false);
    }

    private void OpenSaveLoadMenu()
    {
        CloseVROptions();
        DaggerfallUI.PostMessage(DaggerfallUIMessages.dfuiOpenPauseOptionsDialog);
    }

    private void OpenMapMenu()
    {
        CloseVROptions();
        if (GameManager.Instance.IsPlayerInside)
            DaggerfallUI.PostMessage(DaggerfallUIMessages.dfuiOpenAutomap);
        else
            DaggerfallUI.PostMessage(DaggerfallUIMessages.dfuiOpenTravelMapWindow);
    }

    private void OpenCharacterSheetMenu()
    {
        CloseVROptions();
        DaggerfallUI.PostMessage(DaggerfallUIMessages.dfuiOpenCharacterSheetWindow);
    }

}
