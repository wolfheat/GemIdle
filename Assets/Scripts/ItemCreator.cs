using System;
using UnityEngine;

public class ItemCreator : MonoBehaviour
{
    [SerializeField] private Card cardPrefab;
    
    [SerializeField] private CardData[] cardDatas;



    public static ItemCreator Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public Card GenerateCard()
    {
        // Create a base Card
        Card card = Instantiate(cardPrefab);

        // Fill it with data
        card.SetData(cardDatas[0]);

        // Scale it to fit the Box
        RectTransform rect = card.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(GameStats.BoxWidth, GameStats.BoxHeight);

        return card;
    }
}
