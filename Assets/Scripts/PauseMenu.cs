using System;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{

    public static PauseMenu Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void OpenDeckMenu()
    {
        // Only Allow deck changes when no game is active later
        Stats.IsPaused = true;

        UIController.Instance.OpenDeckbuilderMenu();


    }

    public void HideDeckbuilder()
    {
        Stats.IsPaused = false;

        UIController.Instance.OpenDeckbuilderMenu(false);
    }
}
