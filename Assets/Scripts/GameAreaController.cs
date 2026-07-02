using UnityEngine;

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

    internal void PlaceCard(Card mimicedCard, int xPos, int yPos)
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
    }

    internal void Tick()
    {
        // Evaluate all placed cards and calculate the total profit? Show increment on each card?

        for (int j = 0; j < GameStats.Height; j++) {
            for (int i = 0; i < GameStats.Width; i++) {
                if (placedCards[i, j] == null)
                    continue;

                Card card = placedCards[i, j];

                Debug.Log("Gain Income from card on " + i + "," + j + " = " + card.GetIncome());

            }
        }
    }
}
