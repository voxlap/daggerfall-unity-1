using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Serialization;

public class LoadGameDebug : MonoBehaviour
{
    public string playerName;
    public string saveName;

#if UNITY_EDITOR
    private IEnumerator Start ()
    {
        while (!SaveLoadManager.Instance.IsReady() || !VRInjector.Instance.IsInitialized)
        {
            yield return null;
        }
        LoadGame();
    }
#endif

    public void LoadGame()
    {
        if(!string.IsNullOrEmpty(playerName) && !string.IsNullOrEmpty(saveName))
        {
            VRUIManager.Instance.CloseAllDaggerfallWindows();
            GameManager.Instance.SaveLoadManager.EnumerateSaves();
            GameManager.Instance.SaveLoadManager.Load(playerName, saveName);
        }
    }
}
