using System;
using System.Linq;
using UnityEngine;
public enum CardDataType {RedGemGain, GreenGemGain, BlueGemGain };

public class ItemCreator : MonoBehaviour
{
    [SerializeField] private Card cardPrefab;
    [SerializeField] private Card gainercardPrefab;
    [SerializeField] private Card neutralCardPrefab;
    
    [SerializeField] private CardData[] redCardDatas;
    [SerializeField] private CardData[] greenCardDatas;
    [SerializeField] private CardData[] blueCardDatas;
    [SerializeField] private CardData[] blackCardDatas;
    private CardData[] cardLibrary;

    public static ItemCreator Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Make a List of all cards and give them an ID
        cardLibrary = redCardDatas.Concat(greenCardDatas).Concat(blueCardDatas).Concat(blackCardDatas).ToArray();

    }

    public Card GenerateCard(int cardID, bool inPlay = true)
    {
        if(cardLibrary.Length>cardID && cardLibrary[cardID] != null)
            return GenerateCard(cardLibrary[cardID], inPlay);
        
        return GenerateCard(GemType.Red, 0, inPlay);
    }

    public Card GenerateCard(GemType gemColor, int subType = 0, bool inPlay = true)
    {
        CardData[] cardLibrary = GetLibraryByColor(gemColor);

        if(cardLibrary.Length <= subType) {
            Debug.Log("CardLibrary does not have this subtype");
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
            GemType.Neutral => blackCardDatas,
            _ => redCardDatas
        };
    }
}
