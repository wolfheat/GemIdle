using System;
using UnityEngine;
using UnityEngine.UI;
public class AnimationSpeedToggle : MonoBehaviour
{
    [SerializeField] private Sprite[] speedsprites; 
    [SerializeField] private Image image; 

    private int[] speedsPreset = { 350, 700, 1500, 3000};
    private int activeSpeed = 0;

    public void Toggle()
    {
        // Set next speed
        activeSpeed = (activeSpeed + 1) % speedsPreset.Length;
        GameStats.AnimationSpeed = speedsPreset[activeSpeed];
        SetSprite();
    }

    private void SetSprite() => image.sprite = speedsprites[Math.Min(activeSpeed, speedsprites.Length - 1)];
}
