using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using Wolfheat.StartMenu;

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
    private int currentMultiplier = 0;

    public GameAreaPosition PlacedGameAreaPosition { get; internal set; } = new GameAreaPosition() { Pos = new Vector2Int(-1, -1) };
    public bool IsMultiplier => cardData is MultiplyCardData;
    public int Multiplier => currentMultiplier;

    public void UnsetPosition()
    {
        GameAreaController.Instance.RemoveOldPlacement(this);

        PlacedGameAreaPosition.Pos = new Vector2Int(-1, -1);

        if(gameObject.TryGetComponent(out CardAnimator animator))
            animator.ResetScale();
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

        if (eventData.button == PointerEventData.InputButton.Right) {
            if(PlacedGameAreaPosition.Pos.x == -1) {
                Debug.Log("Card in inventory, on Right click, try to place it on first Empty space");
                // Maybe place it on first available empty spot?
                bool canPlace = GameAreaController.Instance.PlaceCardOnFirstEmptySpot(this);
                if (!canPlace)
                    SoundMaster.Instance.PlaySound(SoundName.PlaceError);
            }
            else {
                InventoryController.Instance.PlaceCard(this);
            }
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

        switch (cardData) {
            case GainerCardData:
                currentGain = ((GainerCardData)cardData).BaseGain;
                gainTextField.text = "+" + currentGain;
                image.sprite = cardData.Image;
                currentIncome = cardData.baseIncome;
                break;
            case MultiplyCardData:
                currentGain = 0;
                currentMultiplier = ((MultiplyCardData)cardData).BaseMultiplier;
                gainTextField.text = "x " + currentMultiplier;
                break;
            default:
                image.sprite = cardData.Image;
                currentIncome = cardData.baseIncome;
                break;
        }

        string colorForGemInsideText = GemColorString(cardData);

        // Fill in the data onto the card from the datafile
        descriptionText.text = "+" + cardData.baseIncome + "   <sprite name=" + cardData.Image.name + ">";
                
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
        }else if (cardData is MultiplyCardData) {
            gainTextField.text = "x " + currentMultiplier;
        }
        // Fill in the data onto the card from the datafile
        if(descriptionText != null)
            descriptionText.text = "+" + currentIncome + "   <sprite name=" + cardData.Image.name + ">";
    }

    internal void DisableInPlay() => inPlay = false;


    public void MultiplyGainBy(int multiplier)
    {
        currentIncome *= multiplier;

        Debug.Log("De setting Multiplier from "+ currentMultiplier+" * "+multiplier);
        currentMultiplier *= multiplier;
        Debug.Log("De setting Multiplier to "+ currentMultiplier + " * " + multiplier);

        UpdateTextsOnCard();
    }
}
