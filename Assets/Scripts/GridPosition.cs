using UnityEngine;
using UnityEngine.UI;

public class GridPosition : MonoBehaviour
{
    [SerializeField] private Image borderHighlight; 
    [SerializeField] private Color borderHighlightColor; 
    [SerializeField] private Color normalColor; 

    public void HighLight(bool doHighlight)
    {
        borderHighlight.color = doHighlight ? borderHighlightColor : normalColor;
    }

}
