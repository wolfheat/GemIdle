using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class DeckSleeve : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private TextMeshProUGUI amtText; 

    // Make private?
    public CardData Data { get; set; }

    public int ID { get; set; }
    public int HolderID { get; set; }

    public void SetSleeveDataID(int id, int holderId, int remainingAmt = 1)
    {
        ID = id;
        HolderID = holderId;
        amtText.text = remainingAmt.ToString();
    }

    public void SetSleeveData(CardData data)
    {
        Data = data;
    }

    // Keep data of the card
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Clicking Sleeve");
        DeckBuilder.Instance.ClickingCard(ID, HolderID);
    }
}
