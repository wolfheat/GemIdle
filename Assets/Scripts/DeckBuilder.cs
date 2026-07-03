using System;
using System.Collections;
using UnityEngine;

public class DeckBuilder : MonoBehaviour
{
    [SerializeField] private Transform deckHolder; 
    [SerializeField] private Transform ownedCardHolder;
    [SerializeField] private DeckSleeve deckbuilderCardSleevePrefab;

    private const int DeckCardSize = 120;


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

        int[] currentDeck = Stats.CurrentDeck;

        for (int i = 0; i < currentDeck.Length; i++) {
            if (currentDeck[i] == 0) continue;

            for (int amt = 0; amt < currentDeck[i]; amt++) {

                Debug.Log("Creating Deck Card "+i+ " out of " + currentDeck[i]);

                DeckSleeve sleeve = Instantiate(deckbuilderCardSleevePrefab, deckHolder);

                sleeve.SetSleeveDataID(i,1);

                Card deckCard = ItemCreator.Instance.GenerateCard(i, false);

                RectTransform rect = deckCard.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(DeckCardSize, DeckCardSize);
                rect.localScale = Vector2.one;

                deckCard.transform.SetParent(deckHolder, false);
                sleeve.transform.SetParent(deckCard.transform, false);
            }
            // Center the sleeve ontop of the card
        }
    }

    private int[] ownedCards;

    private void FillOwnedCards()
    {

        // Destroy Old Area
        foreach (Transform t in ownedCardHolder) {
            Destroy(t.gameObject);
        }

        Debug.Log("FillOwnedCards");

        ownedCards = Stats.OwnedCards;

        for (int i = 0; i < ownedCards.Length; i++) {
            if (ownedCards[i] == 0) continue;

            DeckSleeve sleeve = Instantiate(deckbuilderCardSleevePrefab, deckHolder);

            sleeve.SetSleeveDataID(i, 0);
            Debug.Log("Creating Owned Card "+i);

            Card ownedCard = ItemCreator.Instance.GenerateCard(i, false);

            ownedCard.transform.SetParent(ownedCardHolder,false);
            sleeve.transform.SetParent(ownedCard.transform, false);
        }
    }

    public void ClickingCard(int ID, int HolderID)
    {
        Debug.Log("Received click from Holder "+ HolderID+" ID: "+ID);

        if(HolderID == 0) {

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
    }
}
