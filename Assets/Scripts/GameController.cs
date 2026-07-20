using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Wolfheat.StartMenu;

public enum CardAction { Invalid, PlaceGameArea, PlaceInventory, Swap, Merge, AddMerge, AddImprint, TossCard };

public struct DropCardAction
{
    public CardAction action;
    public Vector2 originPosition;
    public Vector2Int dropPositionIndex;
    public Card targetCard;
    public bool useSpecifiedPosition;
}

public class GameController : MonoBehaviour
{
    private const float TickTime = 1f;
    private float tickTimer = 0f;

    [SerializeField] private BaseCard ghostCardDragged; 
    [SerializeField] private Transform ghostHolder; 
	
    private BaseCard[] ghostCards; 
	
	private Card mimicCard = null;
    //private bool placeHidden = false;

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

        DealNewDeck();
    }

    private void DealNewDeck()
    {
        Stats.ScrambleDeckFromCurrentDeck();
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

			ghostCardDragged.transform.position = Mouse.current.position.ReadValue();


            // Scale it to fit the Box
            RectTransform rect = ghostCardDragged.GetComponent<RectTransform>();
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
		Debug.Log("Mouse pressed: [" + Mouse.current.position.ReadValue().x+"," + Mouse.current.position.ReadValue().y + "] Starting to drag "+cardToDrag.name);

		
		//ghostCard.gameObject.SetActive(true);

		// Try instantiating and cloning the dragged card

		if(ghostCardDragged != null)
			Destroy(ghostCardDragged.gameObject);

		ghostCardDragged = Instantiate(cardToDrag, ghostHolder);


		mimicCard = cardToDrag;
		// Deactivate the dragged one
        mimicCard?.gameObject.SetActive(false);

        // Set Ghostcard to mimic this Card
        ghostCardDragged.Mimic(cardToDrag);

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
    public void PlaceGeneratedCardInInventory(Card card, Vector2 pos)
    {
        Debug.Log("Place A Generated Card into Inventory");
        mimicCard = card;
        //placeHidden = true;
        ExecuteDropAction(new DropCardAction() { action = CardAction.PlaceInventory, originPosition = pos });
        mimicCard = null;
    }

    private void ExecuteDropAction(DropCardAction dropCardAction)
    {
        // Do Action
        switch (dropCardAction.action) {
            case CardAction.Invalid:
                mimicCard?.ReactivateCard();
                InfoPanel.Instance.ShowInfo("Invalid Action.");
                SoundMaster.Instance.PlaySound(SoundName.PlaceError);
                break;
            case CardAction.PlaceGameArea:
                GameAreaController.Instance.PlaceCard(mimicCard, dropCardAction.dropPositionIndex.x, dropCardAction.dropPositionIndex.y);
                SoundMaster.Instance.PlaySound(SoundName.PlaceCard);
                break;
            case CardAction.PlaceInventory:
                InventoryController.Instance.PlaceCard(mimicCard, dropCardAction.originPosition, useMousePositionToOrder: dropCardAction.useSpecifiedPosition);
                SoundMaster.Instance.PlaySound(SoundName.PickupCard);
                break;
            case CardAction.TossCard:
                Debug.Log("TOSSING CARD BY DRAGGING");
                TossCard(mimicCard, false);
                SoundMaster.Instance.PlaySound(SoundName.PickupCard);
                break;
            case CardAction.Swap:
                GameAreaController.Instance.SwapCards(mimicCard, dropCardAction.targetCard, true);
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
        GameAreaController.Instance.NoHighlight();
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

        DropCardAction dropCardAction = new DropCardAction()
        {
            dropPositionIndex = localIndex,
            targetCard = GameAreaController.Instance.Occupier(localIndex.x, localIndex.y),
            action = CardAction.Invalid,
            originPosition = Mouse.current.position.ReadValue()
        };

        // Get the card thats already placed at the target Index

        

        // Is there even a card dragged?
        if (mimicCard == null) {
            dropCardAction.action = CardAction.Invalid;
        }
        else if (OutsidePlayArea(localIndex)) { // Position Index Dropped
            if (BelowPlayArea(localIndex)) {
                if (InventoryController.Instance.CanAddCard()) {
                    dropCardAction.action = CardAction.PlaceInventory;
                    dropCardAction.useSpecifiedPosition = true;
                }
                else
                    dropCardAction.action = CardAction.Invalid;
            }
            else if (DrawCardController.Instance.IsOverTossArea()) {
                dropCardAction.action = CardAction.TossCard;                
            }
            else {
                dropCardAction.action = CardAction.Invalid;
            }
        }else if(dropCardAction.targetCard == null) {
            dropCardAction.action = CardAction.PlaceGameArea;
        }
        else if (mimicCard.PlacedGameAreaPosition.Pos.x == localIndex.x && mimicCard.PlacedGameAreaPosition.Pos.y == localIndex.y) {
            dropCardAction.action = CardAction.Invalid;
        }
        else if (CanAddMerge(dropCardAction.targetCard, mimicCard)) { // A Card was at that spot - Check if the can Merge or should be Swapped
            Debug.Log("Can Add Merge");
            dropCardAction.action = CardAction.AddMerge;      
        }
        else if (CanAddImprint(dropCardAction.targetCard, mimicCard)) { // A Card was at that spot - Check if the can Merge or should be Swapped
            Debug.Log("Can Add Imprint");
            dropCardAction.action = CardAction.AddImprint;
        }
        else if (CanMerge(dropCardAction.targetCard, mimicCard)) { // A Card was at that spot - Check if the can Merge or should be Swapped
            Debug.Log("Can Merge");
            dropCardAction.action = CardAction.Merge;
        }
        else {
            dropCardAction.action = CardAction.Swap;
        }

        return dropCardAction;
    }

    // Card Checks
    private bool CanAddMerge(Card placedCard, Card mimicCard) => (mimicCard.HasImprint && placedCard.IsIncomeCard) || (mimicCard.IsIncomeCard && placedCard.HasImprint);
    private bool CanAddImprint(Card placedCard, Card mimicCard) => (mimicCard.IsAddCard && placedCard.IsIncomeCard) || (mimicCard.IsIncomeCard && placedCard.IsAddCard);
    private bool CanMerge(Card placedCard, Card mimicCard) => (mimicCard.IsMultiplier || placedCard.IsMultiplier) && (mimicCard.AcceptsMerge && placedCard.AcceptsMerge);
    private static bool BelowPlayArea(Vector2Int localIndex) => localIndex.y >= 5;
    private static bool OutsidePlayArea(Vector2Int localIndex) => localIndex.x < 0 || localIndex.y < 0 || localIndex.x >= 7 || localIndex.y >= 5;

    private void HideGhost() => ghostCardDragged.gameObject.SetActive(false);


    // Card Actions
    private void Merge(Card placedCard, Card draggedCard)
    {
        // If this is a multiplier and merging with normal card the merge is more difficult
        // Might need to move the other card to this position and then apply the mult
        Debug.Log("Normal Multiplier Merge "+draggedCard.name+" onto "+ placedCard);

        if (placedCard.IsMultiplier) {
            Debug.Log("Merge - Swap First");
            GameAreaController.Instance.SwapCards(draggedCard, placedCard);
            (placedCard,draggedCard) = (draggedCard,placedCard);
        } 

        ApplyMultToPlacedCard();

        return;

        void ApplyMultToPlacedCard()
        {
            placedCard.MultiplyBy(draggedCard.Multiplier);

            // Remove The Card
            TossCard(draggedCard, true , placedCard.transform.position);
        }
    }

    private void AddMerge(Card placedCard, Card draggedCard)
    {
        // Merge value is added to the Income card
        /*
        if (mimicCard.IsIncomeCard) { // Place Income Card in the Target Position
        
            // If AddMerging and the moved card is
        
            if (mimicCard.IsUnplaced)
                GameAreaController.Instance.PlaceCard(mimicCard, placedCard.PlacedGameAreaPosition.Pos.x, placedCard.PlacedGameAreaPosition.Pos.y, false);
            else
                GameAreaController.Instance.SwapCards(mimicCard, placedCard, false);
            (placedCard,mimicCard) = (mimicCard,placedCard);
            
        }*/
        if (placedCard.IsAddCard) {
            Debug.Log("AddMerge - Swap First");// replace with imprint card
            if (mimicCard.PlacedGameAreaPosition.Pos.x == -1) {
                GameAreaController.Instance.PlaceCard(mimicCard, placedCard.PlacedGameAreaPosition.Pos.x, placedCard.PlacedGameAreaPosition.Pos.y, false);
                Debug.Log("Imprint - Place the Inventory card at gameArea ");
            }
            else {
                GameAreaController.Instance.SwapCards(mimicCard, placedCard, false);
                Debug.Log("Imprint - swap, both cards are at the gameArea");
            }
            (placedCard, draggedCard) = (draggedCard, placedCard);
        }

        placedCard.AddToIncome(draggedCard.AddValue);

        Debug.Log("Addmerge - Call Toss Card - from targetPosition [" + placedCard.transform.position + "]");
        // Store Card ID in tossPile
        Stats.AddTossCard(draggedCard, true, placedCard.transform.position);
    }
    
    private void AddImprint(Card placedCard, Card draggedCard)
    {
        Vector2 targetPosition = placedCard.transform.position;
        // Imprint card is kept
        if (placedCard.IsIncomeCard) { // replace with imprint card
            if(draggedCard.IsUnplaced) { // Inventory 
                GameAreaController.Instance.PlaceCard(draggedCard, placedCard.PlacedGameAreaPosition.Pos.x, placedCard.PlacedGameAreaPosition.Pos.y, false);
                Debug.Log("Imprint - Place the Inventory card at gameArea ");
            }
            else {
                GameAreaController.Instance.SwapCards(draggedCard, placedCard, false);
                Debug.Log("Imprint - swap, both cards are at the gameArea");
            }
            (placedCard,draggedCard) = (draggedCard,placedCard);
        }
        else {
            Debug.Log("Imprint - No Swap needed");
        }

        ApplyAddonToPlacedCard();

        return;

        void ApplyAddonToPlacedCard()
        {
            placedCard.MultiplyBy(draggedCard.GetIncomeGain()); // Doesnt really multiply

            // Remove The Card
            //TossCard(mimicCard, true, placedCard.transform.position);

            // Store Card ID in tossPile
            Debug.Log("Imprint - Call Toss Card - from targetPosition ["+placedCard.PlacedGameAreaPosition.Pos+"]");
            Stats.AddTossCard(draggedCard, true, targetPosition);
        }
    }

    public static void TossCard(Card card, bool animate = true, Vector3 position = default)
    {
        Debug.Log("Tossing Card "+card.name+" active: "+card.gameObject.activeSelf);
        // Move to Toss
        card.UnsetPositionIndex();

        // Store Card ID in tossPile
        Stats.AddTossCard(card, animate, position); 
    }
    
    internal void AnimateGhostFromTo(Card cardToPlace, Card cardToMimic, Vector2 fromPos, Vector2 toPos, Action callback)
    {
        // Do animation
        Debug.Log("Animating Ghost card from position "+fromPos+" to " + toPos);

        StartCoroutine(Animate());

        // Local Coroutine Method
        IEnumerator Animate()
        {
            // Get a new GhostCard
            BaseCard ghostCard = GetGhostCard(cardToMimic,cardToPlace);
            ghostCard.transform.position = fromPos;
            ghostCard.gameObject.SetActive(true);
            yield return null;

            cardToPlace.HideVisuals();
            
            Debug.Log("Animate Ghost visible: "+ ghostCard.gameObject.activeSelf);

            float animationTime = !GameStats.UseSpeed ? GameStats.AnimationTime : Vector2.Distance(fromPos, toPos) / GameStats.AnimationSpeed;
                        
            float t = 0;
            while (t < animationTime) {
                t += Time.deltaTime;
                ghostCard.transform.position = Vector3.Lerp(fromPos, toPos, t/animationTime);
                yield return null;
            }

            Destroy(ghostCard.gameObject);

            if (callback == null) // Handle not showing card if there is a callback, since that is destroying the card
                cardToPlace.ShowVisuals();

            // Return to Callback
            callback?.Invoke();
        }
    }

    private BaseCard GetGhostCard(Card cardToMimic, Card cardToPlace)
    {
        Card newCard = Instantiate(cardToMimic, ghostHolder);
        newCard.Mimic(cardToPlace);
        return newCard;
    }
}
