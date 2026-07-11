using System;
using System.Collections;
using NUnit.Framework;
using TMPro;
using UnityEngine;

public enum DeckBuilderAreaIndex { OwnedCards, CurrentDeck}

public class DeckBuilder : MonoBehaviour
{
    [SerializeField] private Transform deckHolder; 
    [SerializeField] private Transform ownedCardHolder;
    [SerializeField] private DeckSleeve deckbuilderCardSleevePrefab;
    [SerializeField] private TextMeshProUGUI heldCardsText;
    [SerializeField] private GameObject okButton;

    private const int DeckCardSize = 120;
    private const int DeckCardstackOffset = 6;
    private const int MinCardsPerDeckAllowed = 15;

    public static DeckBuilder Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void OnClose()
    {

        // Check if Current Deck is valid, if not ont close or ask to undo any changes
        PauseMenu.Instance.HideDeckbuilder();
    }
    
    private void OnEnable()
    {
        // Activating panel, update the values

        StartCoroutine(DelayedFill());
    }

    private IEnumerator DelayedFill()
    {
        yield return null;
        yield return null;
        yield return null;
        yield return null;
        yield return null;
        yield return null;
        yield return null;

        UpdateDeckbuilder();

    }

    private void FillCurrentDeckCards()
    {
        // Destroy Old Area
        foreach (Transform t in deckHolder) {
            Destroy(t.gameObject);
        }

        int sumCards = Stats.CurrentDeckCards;
        int[] currentDeck = Stats.CurrentDeck;

        for (int i = 0; i < currentDeck.Length; i++) {
            if (currentDeck[i] == 0) continue;
            
            DeckSleeve sleeve = Instantiate(deckbuilderCardSleevePrefab, deckHolder);

            sleeve.SetSleeveDataID(i, (int)DeckBuilderAreaIndex.CurrentDeck, currentDeck[i]);
            int cardsToShow = Math.Min(3, currentDeck[i]);

            Debug.Log("Creating Deck Card " + i + " Amt: " + cardsToShow);

            for (int j =  cardsToShow - 1; j >= 0; j--) {
                CreateDeckBuilderCard(i, sleeve, j);
            }
            //sleeve.transform.SetParent(deckCard.transform, false);
        }

        // Show Cards Amount
        heldCardsText.text = "Cards: " + sumCards;
    }


    private void FillOwnedCards()
    {
        // Destroy Old Area
        foreach (Transform t in ownedCardHolder) {
            Destroy(t.gameObject);
        }

        Debug.Log("FillOwnedCards");
                
        int[] ownedCards = Stats.OwnedCards;
        int[] currentDeck = Stats.CurrentDeck;

        for (int i = 0; i < ownedCards.Length; i++) {
            int amtLeft = ownedCards[i]-currentDeck[i];
            if (amtLeft == 0) continue;

            DeckSleeve sleeve = Instantiate(deckbuilderCardSleevePrefab, ownedCardHolder);
            sleeve.SetSleeveDataID(i, (int)DeckBuilderAreaIndex.OwnedCards, amtLeft);

            int cardsToShow = Math.Min(3, amtLeft);

            Debug.Log("Creating Owned Card "+i+" Amt: "+amtLeft);

            for (int j = cardsToShow - 1; j >= 0; j--) {
                CreateDeckBuilderCard(i, sleeve, j);
            }
        }
    }

    private static void CreateDeckBuilderCard(int i, DeckSleeve sleeve, int j)
    {
        Card deckCard = ItemCreator.Instance.GenerateCard(i, false);

        RectTransform rect = deckCard.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(DeckCardSize, DeckCardSize);
        rect.localScale = Vector2.one;

        deckCard.transform.SetParent(sleeve.CardHolder, false);
        deckCard.transform.localPosition = new Vector2(-DeckCardstackOffset * j, DeckCardstackOffset * j);
    }

    public void ClickingCard(int ID, int HolderID)
    {
        Debug.Log("Received click from Holder "+ HolderID+" ID: "+ID);

        if(HolderID == 0) {
            if (Stats.OwnedCards[ID] - Stats.CurrentDeck[ID] <= 0) {
                Debug.Log("No More cards in ");
                return;
            }
            // Add this card to the Deck
            Stats.CurrentDeck[ID]++;

            Debug.Log("Adding a card "+ID+" to current Deck. Now contains: "+ Stats.CurrentDeck[ID]);

            // Also Remove It from Owned and Anvailable so it cant be added infinitely

            // Update The Deckbuilder
            UpdateDeckbuilder();

        }
        else {
            // Remove this Card from the Deck
            Stats.CurrentDeck[ID]--;

            Debug.Log("Removing a card "+ID+" to current Deck. Now contains: "+ Stats.CurrentDeck[ID]);
            // Update The Deckbuilder
            UpdateDeckbuilder();
        }
    }

    private void UpdateDeckbuilder()
    {
        FillOwnedCards();
        FillCurrentDeckCards();

        // Determine if the deck is valid        
        okButton.SetActive((Stats.CurrentDeckCards >= MinCardsPerDeckAllowed));
    }
}
