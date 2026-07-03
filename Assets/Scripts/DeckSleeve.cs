using UnityEngine;
using UnityEngine.EventSystems;

public class DeckSleeve : MonoBehaviour, IPointerClickHandler
{

    // Make private?
    public CardData Data { get; set; }

    public int ID { get; set; }
    public int HolderID { get; set; }

    public void SetSleeveDataID(int id, int holderId)
    {
        ID = id;
        HolderID = holderId;
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
