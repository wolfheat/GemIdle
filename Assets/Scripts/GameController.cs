using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Wolfheat.StartMenu;

public enum CardAction { Invalid, PlaceGameArea, PlaceInventory, Swap, Merge, AddMerge, AddImprint };

public struct DropCardAction
{
    public CardAction action;
    public Vector2Int dropPosition;
    public Card targetCard;
}

public class GameController : MonoBehaviour
{
    private const float TickTime = 1f;
    private float tickTimer = 0f;

    [SerializeField] private BaseCard ghostCard; 
    [SerializeField] private Transform ghostHolder; 
	
	
	private Card mimicCard = null; 

	public static GameController Instance { get; private set; }

	private void Awake()
	{
		if (Instance != null) {
			Destroy(gameObject);
			return;
		}
		Instance = this;

		//Initiate Stats
		Stats.Initiate();

    }


    private void Update()
    {
        // Tick
        tickTimer += Time.deltaTime;

        if (tickTimer >= TickTime) {
			Tick();
			tickTimer = 0f;
		}


        if (Mouse.current.leftButton.wasReleasedThisFrame) {
			//Debug.Log("Mouse Released this frame");
			StopDrag();
        }

		if (mimicCard != null) {
			Vector2Int highlight = GetHighlightIndex();
            GameAreaController.Instance.HighlightSlot(highlight.x,highlight.y);

			ghostCard.transform.position = Mouse.current.position.ReadValue();


            // Scale it to fit the Box
            RectTransform rect = ghostCard.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(GameStats.BoxWidth, GameStats.BoxHeight);

		}
		
    }

    public void ToggleAnimate()
    {
		Stats.AnimateCards = !Stats.AnimateCards;
    }
	
    private void Tick()
    {
		//Debug.Log("TICK");
		GameAreaController.Instance.Tick();
    }

    public void StartDrag(Card cardToDrag)
	{
		// Request to start Dragging this card
		//Debug.Log("Mouse pressed: [" + Mouse.current.position.ReadValue().x+"," + Mouse.current.position.ReadValue().y + "]");

		
		//ghostCard.gameObject.SetActive(true);

		// Try instantiating and cloning the dragged card

		if(ghostCard != null)
			Destroy(ghostCard.gameObject);

		ghostCard = Instantiate(cardToDrag, ghostHolder);


		mimicCard = cardToDrag;
		// Deactivate the dragged one
        mimicCard?.gameObject.SetActive(false);

        // Set Ghostcard to mimic this Card
        ghostCard.Mimic(cardToDrag);

		// Hide the dragged card - fix later
		if(mimicCard == null)
			Debug.Log("Lost Mimic Card");
	}
	
	private Vector2Int GetHighlightIndex()
	{

        RectTransform gameArea = GameAreaController.Instance.GetComponent<RectTransform>();

        Vector2 localPoint;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            gameArea,
            Mouse.current.position.ReadValue(),
            null,   // Use null if the Canvas is Screen Space - Overlay
            out localPoint
        );

        Vector2 localPos = localPoint + new Vector2(984 / 2, 704 / 2);
		Vector2Int localIndex = new Vector2Int(Mathf.FloorToInt(localPos.x / 140), Mathf.FloorToInt(-localPos.y / 140 + 5));

		return localIndex;
    }



	public void StopDrag()
    {
        if (mimicCard == null) return;

        // Drag ghost should nolonger be visible
        HideGhost();

        // Determine Drop Action
        DropCardAction dropCardAction = DetermineDropAction();

        // Execute it
        ExecuteDropAction(dropCardAction);
        
        // Unset reference to any dragged card
        mimicCard = null;

    }
    public void PlaceGeneratedCardInInventory(Card card)
    {
        mimicCard = card;
        ExecuteDropAction(new DropCardAction() { action = CardAction.PlaceInventory });
        mimicCard = null;
    }

    private void ExecuteDropAction(DropCardAction dropCardAction)
    {
        // Do Action
        switch (dropCardAction.action) {
            case CardAction.Invalid:
                mimicCard?.ReactivateCard();
                SoundMaster.Instance.PlaySound(SoundName.PlaceError);
                break;
            case CardAction.PlaceGameArea:
                GameAreaController.Instance.PlaceCard(mimicCard, dropCardAction.dropPosition.x, dropCardAction.dropPosition.y);
                SoundMaster.Instance.PlaySound(SoundName.PlaceCard);
                break;
            case CardAction.PlaceInventory:
                InventoryController.Instance.PlaceCard(mimicCard);
                SoundMaster.Instance.PlaySound(SoundName.PickupCard);
                break;
            case CardAction.Swap:
                GameAreaController.Instance.SwapCards(mimicCard, dropCardAction.targetCard);
                SoundMaster.Instance.PlaySound(SoundName.PlaceSwap);
                break;
            case CardAction.Merge:
                Merge(dropCardAction.targetCard, mimicCard);
                SoundMaster.Instance.PlaySound(SoundName.MergeCards);
                break;
            case CardAction.AddMerge:
                AddMerge(dropCardAction.targetCard, mimicCard);
                SoundMaster.Instance.PlaySound(SoundName.MergeCards);
                break;
            case CardAction.AddImprint:
                AddImprint(dropCardAction.targetCard, mimicCard);
                SoundMaster.Instance.PlaySound(SoundName.MergeCards);
                break;
        }
    }

    private DropCardAction DetermineDropAction()
    {
        // From input Determine the action to be performed
        //public enum CardAction { Invalid, PlaceGameArea, PlaceInventory, Swap, Merge };

        // Stuff that determmines outcome

        // Placement position Valid
        // Placement position GameArea or Inventory
        // Existing card at placement if GameArea
        // Type Of cards moved and on target position

        // All determines different sounds and actions

        Vector2Int localIndex = GetHighlightIndex();

        // Get the card thats already placed at the target Index
        Card placedCard = GameAreaController.Instance.Occupier(localIndex.x, localIndex.y);

        CardAction cardAction = CardAction.Invalid;

        // Is there even a card dragged?
        if (mimicCard == null) {
            cardAction = CardAction.Invalid;
        }
        else if (OutsidePlayArea(localIndex)) { // Position Index Dropped
            if (BelowPlayArea(localIndex)) {
                cardAction = CardAction.PlaceInventory;
            }
            else {
                cardAction = CardAction.Invalid;
            }
        }else if(placedCard == null) {
            cardAction = CardAction.PlaceGameArea;
        }
        else if (mimicCard.PlacedGameAreaPosition.Pos.x == localIndex.x &&mimicCard.PlacedGameAreaPosition.Pos.y == localIndex.y) {
            cardAction = CardAction.Invalid;
        }
        else if (CanAddMerge(placedCard, mimicCard)) { // A Card was at that spot - Check if the can Merge or should be Swapped
            Debug.Log("Can Add Merge");
            cardAction = CardAction.AddMerge;
        }
        else if (CanAddImprint(placedCard, mimicCard)) { // A Card was at that spot - Check if the can Merge or should be Swapped
            Debug.Log("Can Add Imprint");
            cardAction = CardAction.AddImprint;
        }
        else if (CanMerge(placedCard, mimicCard)) { // A Card was at that spot - Check if the can Merge or should be Swapped
            Debug.Log("Can Merge");
            cardAction = CardAction.Merge;
        }
        else {
            cardAction = CardAction.Swap;
        }

        return new DropCardAction() {dropPosition = localIndex, targetCard = placedCard, action = cardAction };
    }


    private bool CanAddMerge(Card placedCard, Card mimicCard) => (mimicCard.HasImprint && placedCard.IsIncomeCard) || (mimicCard.IsIncomeCard && placedCard.HasImprint);
    private bool CanAddImprint(Card placedCard, Card mimicCard) => (mimicCard.IsAddCard && placedCard.IsIncomeCard) || (mimicCard.IsIncomeCard && placedCard.IsAddCard);
    private bool CanMerge(Card placedCard, Card mimicCard) => (mimicCard.IsMultiplier || placedCard.IsMultiplier) && (mimicCard.AcceptsMerge && placedCard.AcceptsMerge);
    
    private void HideGhost() => ghostCard.gameObject.SetActive(false);

    private static bool BelowPlayArea(Vector2Int localIndex)
    {
        return localIndex.y >= 5;
    }

    private static bool OutsidePlayArea(Vector2Int localIndex)
    {
        return localIndex.x < 0 || localIndex.y < 0 || localIndex.x >= 7 || localIndex.y >= 5;
    }

    private void Merge(Card placedCard, Card mimicCard)
    {
        // If this is a multiplier and merging with normal card the merge is more difficult
        // Might need to move the other card to this position and then apply the mult
        Debug.Log("Merge "+mimicCard.name+" onto "+ placedCard);

        if (placedCard.IsMultiplier) {
            Debug.Log("Swap First");
            GameAreaController.Instance.SwapCards(mimicCard, placedCard);
            (placedCard,mimicCard) = (mimicCard,placedCard);
        } 

        ApplyMultToPlacedCard();

        return;

        void ApplyMultToPlacedCard()
        {
            int multiplier = mimicCard.Multiplier;

            placedCard.MultiplyBy(multiplier);

            // Remove The Card
            RemoveCard(mimicCard);
        }
    }

    private void AddMerge(Card placedCard, Card mimicCard)
    {
        // Merge value is added to the Income card
        if (mimicCard.IsIncomeCard) { // Place Income Card in the Target Position
            GameAreaController.Instance.SwapCards(mimicCard, placedCard);
            (placedCard,mimicCard) = (mimicCard,placedCard);
            Debug.Log("Swap Before Add Merging");
        } 

        ApplyAddonToPlacedCard();

        return;

        void ApplyAddonToPlacedCard()
        {
            int add = mimicCard.AddValue;

            placedCard.AddToIncome(add); // Doesnt really multiply

            // Remove The Card
            RemoveCard(mimicCard);
        }
    }
    
    private void AddImprint(Card placedCard, Card mimicCard)
    {
        // Imprint is added to the Adder card
        if (placedCard.IsIncomeCard) { // replace with adder
            GameAreaController.Instance.SwapCards(mimicCard, placedCard);
            (placedCard,mimicCard) = (mimicCard,placedCard);
        } 

        ApplyAddonToPlacedCard();

        return;

        void ApplyAddonToPlacedCard()
        {
            int add = mimicCard.GetIncomeGain();

            placedCard.MultiplyBy(add); // Doesnt really multiply

            // Remove The Card
            RemoveCard(mimicCard);
        }
    }

    private static void RemoveCard(Card mimicCard)
    {
        // Later maybe move to dump for reuse
        mimicCard.UnsetPosition();
        Destroy(mimicCard.gameObject);
    }
}
