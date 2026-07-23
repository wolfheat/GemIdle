using TMPro;
using UnityEngine;

public class PositionPrinter : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI textField;
    [SerializeField] private Card card;


    private void Update()
    {
        textField.text = ""+card.PlacedGameAreaPosition.Pos.ToString();
    }
}
