using System;
using UnityEngine;
using UnityEngine.UI;

public class DotTimer : MonoBehaviour
{
    [SerializeField] private Image[] timerDots; 
    [SerializeField] private Color unLit; 
    [SerializeField] private Color lit;

    private int step = -1;

    internal bool Tick()
    {
        step++;
        if(step >= timerDots.Length) {
            step = -1;
            ResetAllDots();
            return true;
        }
        else {
            timerDots[step].color = lit;
        }
        return false;
    }

    private void ResetAllDots()
    {
        //Debug.Log("Resetting all Dots "+timerDots.Length);
        foreach (var dot in timerDots) {
            dot.color = unLit;
        }
    }
}
