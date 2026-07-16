using System;
using TMPro;
using UnityEngine;

public class WorldTime : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private GameObject textObject;

    private int style = 0;
    public void ToggleWorldTime()
    {
        style = (style + 1) % 3;
        
        textObject.SetActive(style > 0);
    }

    private void Update()
    {
        // Autoupdate the Time
        if(!textObject.activeSelf) return;

        DateTime now = DateTime.Now;

        timeText.text = now.Hour.ToString("D2") +":"+now.Minute.ToString("D2") + ( style == 2 ? ("."+now.Second.ToString("D2")) : "");
    }

}
