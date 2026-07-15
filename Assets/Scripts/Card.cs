using System;
using System.Collections;
using TMPro;
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
    
    public const float MoveTime = 0.25f;

    private bool isDragged = false;
    
    private int currentIncome = 0;
    private int currentGain = 0;
    private int currentMultiplier = 0;
    private int currentMover = 0;
    private int currentAddValue = 0;

    public GameAreaPosition PlacedGameAreaPosition { get; internal set; } = new GameAreaPosition() { Pos = new Vector2Int(-1, -1) };
    public bool IsMultiplier => cardData is MultiplyCardData;
    public bool AcceptsMerge => cardData.AcceptsMerge;
    public bool IsMover => cardData is MoverCardData;
    public bool IsIncomeCard => currentIncome > 0;
    public bool HasImprint => currentAddValue > 0;
    public int AddValue => currentAddValue;
    public bool IsAddCard => cardData is AddCardData;
    public int Multiplier => currentMultiplier;
    public int ID => cardData.ID;

    public void UnsetPosition()
    {
        GameAreaController.Instance.RemoveOldPlacement(this);

        Place(-1, -1);

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
        Debug.Log("Clicking Card: "+cardData.name);
        if (!inPlay) {
            // Do deckbuilding stuff here instead
            Debug.Log("Card not in play: ");
            return;
        }

        if (eventData.button == PointerEventData.InputButton.Right) {
            if(PlacedGameAreaPosition.Pos.x == -1) {
                Debug.Log("Card in inventory, on Right click, try to place it on first Empty space");
                // Maybe place it on first available empty spot?
                bool canPlace = GameAreaController.Instance.PlaceCardOnFirstEmptySpot(this);
                if (!canPlace) {
                    InfoPanel.Instance.ShowInfo("Play Area is Full, Can not place item.");
                    SoundMaster.Instance.PlaySound(SoundName.PlaceError);
                }
            }
            else {
                if (InventoryController.Instance.CanAddCard()) {
                    InventoryController.Instance.PlaceCard(this,true,false);
                    SoundMaster.Instance.PlaySound(SoundName.PickupCard);
                }
                else {
                    InfoPanel.Instance.ShowInfo("Inventory Full");
                    SoundMaster.Instance.PlaySound(SoundName.PlaceError);
                }
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

        border.color = cardData.borderColor;
        backgroundImage.color = cardData.BackgroundSpriteColor;

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
                image.sprite = cardData.Image;
                gainTextField.text = "x " + currentMultiplier;
                break;
            case MoverCardData:
                currentMover = ((MoverCardData)cardData).BaseMover;
                image.sprite = cardData.Image;
                gainTextField.text = "" + currentMover;
                break;
            case AddCardData:
                image.sprite = cardData.Image;
                Debug.Log("GainTextField: "+gainTextField);
                Debug.Log("GainTextField.text: "+gainTextField.text);
                gainTextField.text = "";
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
        if (!gameObject.activeSelf)
            ReactivateCard();

        if (!inPlay) {
            // Do deckbuilding stuff here instead
            return;
        }
        PlacedGameAreaPosition.Pos = new Vector2Int(xPos, yPos);

    }

    public void ReactivateCard() => gameObject.SetActive(true);

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

        // If Mover
        if (IsMover) {
            // Get card to left and right if both can take a value do the move
            Card prevCard = GameAreaController.Instance.GetCardAt(PlacedGameAreaPosition.Pos + Vector2Int.left);
            Card nextCard = GameAreaController.Instance.GetCardAt(PlacedGameAreaPosition.Pos + Vector2Int.right);
            
            if(prevCard == null || nextCard == null)
                return;

            if (prevCard.currentIncome > 0 && nextCard.currentIncome > 0) {
                int movedAmt = Math.Min(prevCard.currentIncome, currentMover);
                prevCard.currentIncome -= movedAmt;
                if(prevCard.currentIncome <= 0) {
                    prevCard.Kill();
                }
                nextCard.currentIncome += movedAmt;
            }
        }
    }

    private void Kill()
    {
        GameAreaController.Instance.ChangeToKilledCard(this);
    }

    public void UpdateTextsOnCard()
    {
        if (cardData is GainerCardData) {

            gainTextField.text = "+" + currentGain;
        }else if (cardData is MultiplyCardData) {
            gainTextField.text = "x " + currentMultiplier;
        }else if (cardData is MoverCardData) {
            gainTextField.text = "" + currentMover;
        }else if (cardData is AddCardData) {
            gainTextField.text = "" + (currentAddValue > 0 ? currentAddValue:"");
        }
        // Fill in the data onto the card from the datafile
        if(descriptionText != null)
            descriptionText.text = "+" + currentIncome + "   <sprite name=" + cardData.Image.name + ">";

    }

    internal void DisableInPlay() => inPlay = false;

    public void MultiplyBy(int multiplier)
    {
        Debug.Log("Applying Multiplier: "+multiplier);
        if (cardData is GainerCardData) {
            currentIncome *= multiplier;
            currentMultiplier *= multiplier;
            Debug.Log("Multiplying Gainer card");
        }
        else if (cardData is MultiplyCardData) {
            currentMover *= multiplier;
            currentMultiplier *= multiplier;
            Debug.Log("Multiplying Multiply card");
        }
        else if (cardData is MoverCardData) {
            currentMover *= multiplier;
            Debug.Log("Multiplying Mover card");
        }
        else if (cardData is AddCardData) {
            currentAddValue = multiplier;
            Debug.Log("Setting the Add cards add value");
        }
        else {
            currentIncome *= multiplier;
            currentMultiplier *= multiplier;
            Debug.Log("Multiplying Normal Card");
        }

        UpdateTextsOnCard();

        // Animate card at any Merging
        GetComponent<CardAnimator>()?.ForcedAnimate();
    }

    public void AddToIncome(int addAmt)
    {
        Debug.Log("Add to Income "+addAmt);
        currentIncome += addAmt;
        UpdateTextsOnCard();
    }

    internal void SetScale()
    {
        // Need to do this delayed

        StartCoroutine(DelayedScale());
    }

    private IEnumerator DelayedScale()
    {
        yield return null;
        // Scale it to fit the Box
        RectTransform rect = GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(GameStats.BoxWidth, GameStats.BoxHeight);
        rect.localScale = Vector3.one;
        Debug.Log("Scaling card to "+GameStats.BoxWidth+","+GameStats.BoxHeight);
    }

    internal void AnimateToPosition(Vector2 endPos, Action callback) => StartCoroutine(AnimateToPositionCO(endPos, callback));

    private IEnumerator AnimateToPositionCO(Vector2 endPos, Action callback)
    {
        Vector2 startPos = transform.position;
        
        float moveTimer = 0f;

        while (moveTimer < MoveTime) {
            moveTimer += Time.deltaTime;
            float fraction = moveTimer / MoveTime;
            transform.position = Vector2.Lerp(startPos, endPos, fraction);
            yield return null;
        }

        callback?.Invoke();
    }

    internal void Destroy() => Destroy(gameObject);
}
