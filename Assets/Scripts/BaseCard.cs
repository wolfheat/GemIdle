using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BaseCard : MonoBehaviour
{
    [SerializeField] protected Image image;
    [SerializeField] protected TextMeshProUGUI descriptionText;
        
    internal void Mimic(Card cardToDrag)
    {
        // Make this card a copy of the mimiced card

        descriptionText = cardToDrag.descriptionText;
        image = cardToDrag.image;
    }

}
