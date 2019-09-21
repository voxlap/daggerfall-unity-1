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
