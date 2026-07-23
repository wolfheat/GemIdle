using System;
using System.Collections;
using UnityEngine;
using Wolfheat.StartMenu;

public class GameAreaController : MonoBehaviour
{
    private GridPosition[,] gridPositions;
    private Card[,] placedCards;

    public Vector2 GetGridPositionPositionVector(Vector2Int pos) => GridPositions[pos.x,pos.y].transform.position;

    [SerializeField] private GridPosition gridPositionPrefab;
    [SerializeField] private Transform gridPositionHolder;
    [SerializeField] private Transform itemHolder;

    public static GameAreaController Instance { get; private set; }
    public GridPosition[,] GridPositions { get => gridPositions; set => gridPositions = value; }
    public Card[,] PlacedCards { get => placedCards; set => placedCards = value; }

    private void Awake()
    {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        placedCards = new Card[GameStats.Width, GameStats.Height];
    }


    void Start()
    {
        GeneratePlayArea();
    }

    private void GeneratePlayArea()
    {
        // Destroy Old Area
        foreach (Transform t in gridPositionHolder) {
            Destroy(t.gameObject);
        }

        GridPositions = new GridPosition[GameStats.Width, GameStats.Height];

        for (int j = 0; j < GameStats.Height; j++) {
            for (int i = 0; i < GameStats.Width; i++) {
                GridPosition gridPos = Instantiate(gridPositionPrefab, gridPositionHolder);
                GridPositions[i, j] = gridPos;
                gridPos.name = "GridPos: ("+i+","+j+")";
            }
        }
    }

    internal void NoHighlight() => HighlightSlot(-1, -1);
    internal void HighlightSlot(int xPos, int yPos)
    {
        for (int j = 0; j < GameStats.Height; j++) {
            for (int i = 0; i < GameStats.Width; i++) {
                GridPositions[i, j].HighLight(xPos == i && yPos == j);
            }
        }
    }

    internal void SwapCards(Card card, Card targetCard, bool animate = false)
    {
        Debug.Log("SWAP Cards in GameArea");
        Vector2Int fromPos = card.PlacedGameAreaPosition.Pos;
        Vector2Int toPos = targetCard.PlacedGameAreaPosition.Pos;
        Vector2 toPosGlobal = GetGridPositionPositionVector(toPos);

        if (card.IsPlaced) { 
            // Place both cards in each others positions in the playArea
            PlaceCard(card, toPos.x, toPos.y, false);
            PlaceCard(targetCard, fromPos.x, fromPos.y, false);

            if (animate) {
                card.HideVisuals();
                targetCard.HideVisuals();

                GameController.Instance.AnimateGhostFromTo(card, MouseUtils.MousePosition);
                GameController.Instance.AnimateGhostFromTo(targetCard, toPosGlobal);
            }

        }
        else {

            // Card is coming from the hand - Place the target card back in the inventory
            InventoryController.Instance.PlaceCard(targetCard, toPosGlobal, false);

            // And place the dragged card in the playArea
            PlaceCard(card, toPos.x, toPos.y, false);
        }
    }
    /*
    internal void SwapCards(Card mimicedCard, Card targetCard, bool animate = false)
    {
        Vector2Int fromPos = mimicedCard.PlacedGameAreaPosition.Pos;
        Vector2Int toPos = targetCard.PlacedGameAreaPosition.Pos;

        if (mimicedCard.IsPlacedInGameArea()) {
            
            // Card is coming from the hand - PLace the target card back in the invnetory
            InventoryController.Instance.PlaceCard(targetCard, targetCard.transform.position, false);

            // And place the dragged card in the playArea
            PlaceCard(mimicedCard, toPos.x, toPos.y, false);
        }
        else {
            // Place both cards in each others positions in the playArea
            PlaceCard(mimicedCard, toPos.x, toPos.y, false);
            PlaceCard(targetCard, fromPos.x, fromPos.y, false);

            // Animate this
            if (animate) {
                Debug.Log("Animate a ghost that moves from the TargetCard to the MimicedCards position");
                GameController.Instance.AnimateGhostFromTo(targetCard, targetCard, GetGridPositionPositionVector(toPos), GetGridPositionPositionVector(fromPos), null);
            }
            else {
                Debug.Log("Instantly Swap the cards");
            }
        }
    }*/


    internal void PlaceCard(Card card, int xPos, int yPos, bool unsetOldPosition = true)
    {
        Debug.Log("GameArea - Placing Card " + card.name + " Position: " + xPos+","+yPos);

        // If the card comes from the Inventory, inventory needs to know so it can animate correctly
        if (!card.IsPlaced) {
            InventoryController.Instance.StoreAndAnimateInventory();
        }

        card.transform.parent = itemHolder.transform;

        // Get the gridPosition to place the card at        
        card.transform.position = GridPositions[xPos, yPos].transform.position;

        RemoveOldPlacementIndex(card, unsetOldPosition);

        card.SetPlace(xPos, yPos);

        card.SetScale();

        placedCards[xPos, yPos] = card;
    }

    /*
    internal void PlaceCard(Card mimicedCard, int xPos, int yPos, bool unsetOldPosition = true)
    {
        // Unset its old position?  Not needed if only reference to it is it being a child - might change later

        mimicedCard.transform.parent = itemHolder.transform;
        //mimicedCard.UnsetOldPosition

        // Get the gridPosition to place the card at
        GridPosition positionToPlaceAt = gridPositions[xPos, yPos];

        // Dont set the item to the new parent, just its position
        mimicedCard.transform.position = positionToPlaceAt.transform.position;
        //mimicedCard.transform.localPosition = Vector3.zero;

        // Scale it to fit the Box
        RectTransform rect = mimicedCard.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(GameStats.BoxWidth, GameStats.BoxHeight);
        
        RemoveOldPlacementIndex(mimicedCard, unsetOldPosition);

        mimicedCard.Place(xPos,yPos);

        mimicedCard.SetScale();

        placedCards[xPos, yPos] = mimicedCard;

    }*/

    public void RemoveOldPlacementIndex(Card card, bool unsetOldPosition = true)
    {
        // If it was placed, remove old placement
        Vector2Int oldPos = card.PlacedGameAreaPosition.Pos;
        if (card.IsPlaced) {
            if (unsetOldPosition) {
                Debug.Log("GameArea - Unregister Card Placement in GameArea: "+oldPos);
                placedCards[oldPos.x, oldPos.y] = null;
            }
            card.SetPlace(-1, -1);
        }
    }

    internal void Tick()
    {
        // Evaluate all placed cards and calculate the total profit? Show increment on each card?

        // Stage 1 - Do the Tick for each card
        for (int j = 0; j < GameStats.Height; j++) {
            for (int i = 0; i < GameStats.Width; i++) {
                if (placedCards[i, j] == null)
                    continue;

                Card card = placedCards[i, j];
                card.Tick();
            }
        }

        // Stage 2 - Update All Texts - and animate if animation is enabled
        for (int j = 0; j < GameStats.Height; j++) {
            for (int i = 0; i < GameStats.Width; i++) {
                if (placedCards[i, j] == null)
                    continue;

                Card card = placedCards[i, j];

                card.UpdateTextsOnCard();

                CardAnimator animator = card.GetComponent<CardAnimator>();
                animator?.Animate();

                GemType gemType = card.GemType();

                Stats.AddGems(gemType, card.GetIncomeGain());
            }
        }
    }

    internal bool PositionHasOccupier(int x, int y) => placedCards[x, y] != null;

    internal Card Occupier(int x, int y) => IndexOutsidePlacedCardsArray(x, y) ? null : placedCards[x, y];
    private bool IndexOutsidePlacedCardsArray(int x, int y) => (x < 0 || x >= placedCards.GetLength(0) || y < 0 || y >= placedCards.GetLength(1));
    
    internal bool PlaceCardOnFirstEmptySpot(Card cardToPlace)
    {   
        Vector2 startPos = cardToPlace.transform.position;

        for (int j = 0; j < GameStats.Height; j++) {
            for (int i = 0; i < GameStats.Width; i++) {
                if (placedCards[i, j] != null) continue;

                // Found an empty space
                cardToPlace.HideVisuals();

                PlaceCard(cardToPlace, i, j);
                
                GameController.Instance.AnimateGhostFromTo(cardToPlace, startPos);

                return true;
            }
        }
        return false;
    }

    internal Card GetCardAt(Vector2Int pos)
    {
        if (IndexOutsidePlacedCardsArray(pos.x, pos.y))
            return null;
        return placedCards[pos.x, pos.y];
    }

    internal void ChangeToKilledCard(Card card)
    {
        // Swap this card for a dead one
        Vector2Int cardsPosition = card.PlacedGameAreaPosition.Pos;

        Card deadCard = ItemCreator.Instance.GenerateDeadCard();
        //deadCard.Place(cardsPosition.x,cardsPosition.y);

        PlaceCard(deadCard,cardsPosition.x,cardsPosition.y);

        Destroy(card.gameObject);
    }
}
