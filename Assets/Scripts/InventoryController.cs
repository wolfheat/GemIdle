using UnityEngine;

public class InventoryController : MonoBehaviour
{
    public const int Width = 7;
    public const int Height = 1;
    public const int BoxWidth = 140;
    public const int BoxHeight = 140;

    private GridPosition[,] gridPositions;
    //private Card[,] gridcards;
    
    [SerializeField] private Transform itemHolder;

    public static InventoryController Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
        
    internal void PlaceCard(Card mimicedCard)
    {
        // Place Card in Inventroy -  Add it to the Holder - Also keep track of it?
        mimicedCard.transform.parent = itemHolder;

        // Scale it to fit the Box
        RectTransform rect = mimicedCard.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(BoxWidth, BoxHeight);
    }

    public void GenerateRandomCard()
    {
        Card card = ItemCreator.Instance.GenerateCard();

        // Place Card in Inventroy -  Add it to the Holder - Also keep track of it?
        card.transform.parent = itemHolder;

        // Scale it to fit the Box
        RectTransform rect = card.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(BoxWidth, BoxHeight);
    }
}
