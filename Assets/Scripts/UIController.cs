using System;
using UnityEngine;

public class UIController : MonoBehaviour
{
    [SerializeField] private GameObject deckBuilderMenu;


    public static UIController Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    internal void OpenDeckbuilderMenu(bool open = true)
    {
        deckBuilderMenu.SetActive(open);
    }

}
