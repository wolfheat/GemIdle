using System;
using System.Collections;
using UnityEngine;
using Wolfheat.StartMenu;

public class GameAreaController : MonoBehaviour
{

    private GridPosition[,] gridPositions;
    private Card[,] placedCards;
    
    [SerializeField] private GridPosition gridPositionPrefab;
    [SerializeField] private Transform gridPositionHolder;
    [SerializeField] private Transform itemHolder;

    public static GameAreaController Instance { get; private set; }

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

        gridPositions = new GridPosition[GameStats.Width, GameStats.Height];

        for (int j = 0; j < GameStats.Height; j++) {
            for (int i = 0; i < GameStats.Width; i++) {
                GridPosition gridPos = Instantiate(gridPositionPrefab, gridPositionHolder);
                gridPositions[i, j] = gridPos;
                gridPos.name = "GridPos: ("+i+","+j+")";
            }
        }
    }

    internal void NoHighlight() => HighlightSlot(-1, -1);
    internal void HighlightSlot(int xPos, int yPos)
    {

        for (int j = 0; j < GameStats.Height; j++) {
            for (int i = 0; i < GameStats.Width; i++) {
                gridPositions[i, j].HighLight(xPos == i && yPos == j);
            }
        }
    }

    internal void SwapCards(Card mimicedCard, Card targetCard)
    {
        Vector2Int fromPos = mimicedCard.PlacedGameAreaPosition.Pos;
        Vector2Int toPos = targetCard.PlacedGameAreaPosition.Pos;

        if(fromPos.x == -1) {
            // Card is coming from the hand
            Debug.Log("SWAPFAIL - Returning card to Inventory from position: "+ targetCard.transform.position);
            InventoryController.Instance.PlaceCard(targetCard,targetCard.transform.position, false);
            PlaceCard(mimicedCard, toPos.x, toPos.y, false);
        }
        else {
            PlaceCard(mimicedCard, toPos.x, toPos.y ,false);
            PlaceCard(targetCard, fromPos.x, fromPos.y ,false);
        }
    }

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

    }

    public void RemoveOldPlacementIndex(Card mimicedCard, bool unsetOldPosition = true)
    {
        // If it was placed, remove old placement
        Vector2Int oldPos = mimicedCard.PlacedGameAreaPosition.Pos;
        if (oldPos.x != -1) {
            if(unsetOldPosition)
                placedCards[oldPos.x, oldPos.y] = null;
            mimicedCard.Place(-1, -1);
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
                PlaceCard(cardToPlace, i, j);
                // Wait one frame
                StartCoroutine(AnimateOneFrameLater());

                return true;
            }
        }
        return false;

        IEnumerator AnimateOneFrameLater()
        {
            yield return null;

            if (true) { // Always Animate
                // If it should animate
                // Place it, but have it hidden until ghost animation is complete

                // Animate - Might need to wait a frame for the end pos to update
                GameController.Instance.AnimateGhostFromTo(cardToPlace, startPos, cardToPlace.transform.position, null, true);
            }
        }
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
