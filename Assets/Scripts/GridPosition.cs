using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GridPosition : MonoBehaviour
{
    [SerializeField] private Image borderHighlight; 
    [SerializeField] private Color borderHighlightColor; 
    [SerializeField] private Color normalColor;


    [SerializeField] private TextMeshProUGUI textField;

    public void HighLight(Card card)
    {
        // Use card to highlight
        HighLight(card != null);
    }

    public void HighLight(bool doHighlight)
    {
        borderHighlight.color = doHighlight ? borderHighlightColor : normalColor;
    }    

    public void ShowOccupant(Card card)
    {
        textField.text = card?.name ?? "-";
    }    
}
