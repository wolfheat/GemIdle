using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class DeckSleeve : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private TextMeshProUGUI amtText; 
    [SerializeField] private GameObject amtTextHolder; 
    [SerializeField] private Transform cardHolder; 

    // Make private?
    public CardData Data { get; set; }

    public int ID { get; set; }
    public int HolderID { get; set; }
    public Transform CardHolder => cardHolder;

    public void SetSleeveDataID(int id, int holderId, int remainingAmt = 1)
    {
        ID = id;
        HolderID = holderId;
        SetAmoutText(remainingAmt);
    }

    private void SetAmoutText(int remainingAmt)
    {
        amtTextHolder.SetActive(remainingAmt > 3); 
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
