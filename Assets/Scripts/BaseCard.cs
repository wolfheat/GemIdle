using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BaseCard : MonoBehaviour
{
    [SerializeField] protected Image image;
    [SerializeField] protected Image backgroundImage;
    [SerializeField] protected Image border;
    [SerializeField] protected TextMeshProUGUI descriptionText;
    [SerializeField] protected GameObject actualCardPart;
    [SerializeField] private bool inPlay = true;
    public bool InPlay { get => inPlay; set => inPlay = value; }

    internal void Mimic(Card cardToDrag)
    {
        //Debug.Log("Mimic the dragged card, set image: "+cardToDrag.image.name);
        // Make this card a copy of the mimiced card
        descriptionText = cardToDrag.descriptionText;
        image = cardToDrag.image;

    }

    internal void HideVisuals() => actualCardPart.SetActive(false);
    internal void ShowVisuals() => actualCardPart.SetActive(true);

}
