using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameAreaPosition
{
    public Vector2Int Pos;

    public bool IsPlaced => Pos.x != -1;
}

public class Card : BaseCard, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private bool inPlay = true;
    [SerializeField] private CardData cardData;
    
    [SerializeField] private DotTimer dotTimer;

    [SerializeField] private TextMeshProUGUI gainTextField;
    
    private bool isDragged = false;
    
    private int currentIncome = 0;
    private int currentGain = 0;

    public GameAreaPosition PlacedGameAreaPosition { get; internal set; } = new GameAreaPosition() { Pos = new Vector2Int(-1, -1) };
    public void UnsetPosition()
    {
        GameAreaController.Instance.RemoveOldPlacement(this);

        PlacedGameAreaPosition.Pos = new Vector2Int(-1, -1);
    }

    private void Start()
    {
        if (!inPlay) {
            // Do deckbuilding stuff here instead
            if (cardData == null) return;

            /// Apply the data
            ApplyData();
            return;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!inPlay) {
            // Do deckbuilding stuff here instead
            return;
        }
        Debug.Log("Start To Drag Item");

        isDragged = true;


        GameController.Instance?.StartDrag(this);
        // Hide this Item and show a copy of it on top of game that is dragged - if released in false position return it to original position
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!inPlay) {
            // Do deckbuilding stuff here instead
            return;
        }
        Debug.Log("Stop Drag Item");
        isDragged = false;

        GameController.Instance?.StopDrag();
    }

    public GemType GemType() => cardData.type;

    internal int GetIncomeGain()
    {
        if(cardData == null)
            return 0;

        // Calculate the income
        
        return currentIncome;
    }

    internal void SetData(CardData data)
    {
        cardData = data;

        //<sprite name=RedGem>

        // +35 <sprite name=RedGem>

        ApplyData();
    }

    private void ApplyData()
    {
        if (cardData == null) return;

        if (cardData is GainerCardData) {

            currentGain = ((GainerCardData)cardData).baseGain;
            gainTextField.text = "+" + currentGain;
        }


        string colorForGemInsideText = GemColorString(cardData);

        // Fill in the data onto the card from the datafile
        descriptionText.text = "+" + cardData.baseIncome + "   <sprite name=" + cardData.Image.name + ">";

        image.sprite = cardData.Image;

        currentIncome = cardData.baseIncome;
    }

    private string GemColorString(CardData data)
    {
        return data.type switch
        {
            global::GemType.Red => "RedGem",
            global::GemType.Green => "GreenGem",
            global::GemType.Blue => "BlueGem",
            _ => ""
        };
    }

    internal void Place(int xPos, int yPos)
    {
        if (!inPlay) {
            // Do deckbuilding stuff here instead
            return;
        }
        PlacedGameAreaPosition.Pos = new Vector2Int(xPos, yPos);
    }

    internal void Tick()
    {
        if (!inPlay) {
            // Do deckbuilding stuff here instead
            return;
        }
        // Tick updates the card to its new value, but does not send away the gain - gain is asked for later
        if (dotTimer != null) {
            if (dotTimer.Tick()) {
                // It lapped around, should now increment accordingly
                if (cardData is GainerCardData)
                    currentIncome += currentGain;
                // Check for other card type changes here also later
            }
        }


        UpdateTextsOnCard();


    }

    private void UpdateTextsOnCard()
    {
        if (cardData is GainerCardData) {

            gainTextField.text = "+" + currentGain;
        }
        // Fill in the data onto the card from the datafile
        descriptionText.text = "+" + currentIncome + "   <sprite name=" + cardData.Image.name + ">";
    }

    internal void DisableInPlay() => inPlay = false;
}
