using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class Card : BaseCard, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private CardData cardData;
    private bool isDragged = false;

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("Start To Drag Item");

        isDragged = true;


        GameController.Instance?.StartDrag(this);
        // Hide this Item and show a copy of it on top of game that is dragged - if released in false position return it to original position
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log("Stop Drag Item");
        isDragged = false;

        GameController.Instance?.StopDrag();
    }

    internal int GetIncome()
    {
        // Calculate the income
        int income = cardData.amt;

        return income;
    }

    internal void SetData(CardData data)
    {
        cardData = data;

        //<sprite name=RedGem>

        // +35 <sprite name=RedGem>

        // Fill in the data onto the card from the datafile
        descriptionText.text = "+" + data.amt+" <sprite name=RedGem>";

        image.sprite = data.Image;



    }
}
