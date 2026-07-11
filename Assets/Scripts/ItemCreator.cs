using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public enum CardDataType {RedGemGain, GreenGemGain, BlueGemGain };

public class ItemCreator : MonoBehaviour
{
    [SerializeField] private Card cardPrefab;
    [SerializeField] private Card gainercardPrefab;
    [SerializeField] private Card neutralCardPrefab;
    [SerializeField] private Card deadCardPrefab;
    [SerializeField] private Card deckCardPrefab;
    
    [SerializeField] private CardData[] redCardDatas;
    [SerializeField] private CardData[] greenCardDatas;
    [SerializeField] private CardData[] blueCardDatas;
    [SerializeField] private CardData[] neutralCardDatas;
    private CardData[] cardLibrary;

    public static ItemCreator Instance { get; private set; }

    private Dictionary<int, CardData> cardDictionary = new();
    
    public CardData GetCardDataByIndex(int value) => cardDictionary[value];
    public int TotalCardAmount() => cardDictionary.Keys.Count;

    
    private void Awake()
    {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Make a List of all cards and give them an ID
        cardLibrary = redCardDatas.Concat(greenCardDatas).Concat(blueCardDatas).Concat(neutralCardDatas).ToArray();

        // Build the DictionaryLookup for cards
        for (int i = 0; i < cardLibrary.Length; i++) {
            cardDictionary[i] = cardLibrary[i];
        }

    }

    public Card GenerateDeckCard(bool isActive = true)
    {
        Card card = Instantiate(deckCardPrefab);
        return card;
    }

    public Card GenerateDeadCard(bool isActive = true) => GenerateCard(GemType.Neutral, 3, isActive);

    public Card GenerateCard(int cardID, bool inPlay = true)
    {
        Debug.Log("Generate card "+cardID);

        if(cardID < cardLibrary.Length && cardLibrary[cardID] != null) {
            //Debug.Log("Generated Card Type "+cardID);
            return GenerateCard(cardLibrary[cardID], inPlay);
        }

        Debug.Log("Generated Card Type was not founnd, making default");
        return GenerateCard(GemType.Red, 0, inPlay);
    }

    public Card GenerateCard(GemType gemColor, int subType = 0, bool inPlay = true)
    {
        CardData[] cardLibrary = GetLibraryByColor(gemColor);

        if(cardLibrary.Length <= subType) {
            Debug.Log("CardLibrary does not have this subtype:  Library: "+gemColor+" subType: "+subType);
            return null;
        }

        CardData data = cardLibrary[subType];

        return GenerateCard(data, inPlay = true);
    }

    public Card GenerateCard(CardData data, bool inPlay = true)
    {
        // Create a base Card
        Card card;
        
        switch (data) {
            case GainerCardData:
                card = Instantiate(gainercardPrefab);
                break;
            case MultiplyCardData:
            case MoverCardData:
            case AddCardData:
                card = Instantiate(neutralCardPrefab);
                break;
            case DeadCardData:
                card = Instantiate(deadCardPrefab);
                break;
            default:
                card = Instantiate(cardPrefab);
                break;
        }


        // Fill it with data
        card.SetData(data);

        if (!inPlay)
            card.DisableInPlay();

        // Scale it to fit the Box
        RectTransform rect = card.GetComponent<RectTransform>();
        //rect.sizeDelta = new Vector2(GameStats.BoxWidth, GameStats.BoxHeight);
        //rect.localScale = Vector2.one;
        return card;
    }

    private CardData[] GetLibraryByColor(GemType color)
    {
        return color switch
        {
            GemType.Red => redCardDatas,
            GemType.Green => greenCardDatas,
            GemType.Blue => blueCardDatas,
            GemType.Neutral => neutralCardDatas,
            _ => redCardDatas
        };
    }
}
