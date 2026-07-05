using System;
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

    internal void HighlightSlot(int xPos, int yPos)
    {

        for (int j = 0; j < GameStats.Height; j++) {
            for (int i = 0; i < GameStats.Width; i++) {
                gridPositions[i, j].HighLight(xPos == i && yPos == j);
            }
        }
    }

    internal void SwapCards(Card mimicedCard, Card targetCard, bool playSound = true)
    {
        Vector2Int fromPos = mimicedCard.PlacedGameAreaPosition.Pos;
        Vector2Int toPos = targetCard.PlacedGameAreaPosition.Pos;

        if(fromPos.x == -1) {
            // Card is coming from the hand
            InventoryController.Instance.PlaceCard(targetCard, false);
            PlaceCard(mimicedCard, toPos.x, toPos.y, false, playSound);
        }
        else {
            PlaceCard(mimicedCard, toPos.x, toPos.y ,false, playSound);
            PlaceCard(targetCard, fromPos.x, fromPos.y ,false, playSound);
        }
        if(playSound)
            SoundMaster.Instance.PlaySound(SoundName.PlaceSwap);
    }

    internal void PlaceCard(Card mimicedCard, int xPos, int yPos, bool unsetOldPosition = true, bool playSound = true)
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
        
        RemoveOldPlacement(mimicedCard, unsetOldPosition);

        mimicedCard.Place(xPos,yPos);

        placedCards[xPos, yPos] = mimicedCard;

        if(playSound)
            SoundMaster.Instance.PlaySound(SoundName.PlaceCard);

    }

    public void RemoveOldPlacement(Card mimicedCard, bool unsetOldPosition = true)
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

        for (int j = 0; j < GameStats.Height; j++) {
            for (int i = 0; i < GameStats.Width; i++) {
                if (placedCards[i, j] == null)
                    continue;

                Card card = placedCards[i, j];

                card.Tick();
                CardAnimator animator = card.GetComponent<CardAnimator>();
                animator?.Animate();

                Debug.Log("Gain Income from card on " + i + "," + j + " = " + card.GetIncomeGain());

                GemType gemType = card.GemType();

                Stats.AddGems(gemType, card.GetIncomeGain());

            }
        }
    }

    internal bool PositionHasOccupier(int x, int y) => placedCards[x, y] != null;
    internal Card Occupier(int x, int y) => placedCards[x, y];

    internal bool PlaceCardOnFirstEmptySpot(Card card)
    {
        for (int j = 0; j < GameStats.Height; j++) {
            for (int i = 0; i < GameStats.Width; i++) {
                if (placedCards[i, j] != null) continue;

                // Found an empty space
                PlaceCard(card, i, j);
                return true;
            }
        }
        return false;
    }
}
