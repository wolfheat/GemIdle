using System;
using System.Collections.Generic;
using UnityEngine;

public class DrawCardController : MonoBehaviour
{
    [SerializeField] private Transform DeckHolder;
    [SerializeField] private Transform TossHolder;

    private void UpdateDeckStack()
    {
        List<int> deckPile = new List<int>(Stats.Deck);
        int amt = Math.Min(3, deckPile.Count);
        int startIndex = amt - 3;
        int offsets = 3;
        for (int i = startIndex; i < amt; i++) {
            offsets--;
            if (i < 0) continue;
            Card BackSideCard = ItemCreator.Instance.GenerateDeckCard();
            BackSideCard.SetScale();
            BackSideCard.transform.parent = DeckHolder;      
            BackSideCard.transform.localScale = Vector3.one;
            BackSideCard.transform.localPosition = new Vector2(offsets * -8, offsets * 8);
            BackSideCard.enabled = false;
        }
    }
    
    private void UpdateTossStack()
    {
        List<int> tossPile = new List<int>(Stats.Toss);
        int amt = tossPile.Count;
        int startIndex = amt - 3;
        int offsets = 3;
        for (int i = startIndex; i < amt; i++) {
            offsets--;
            if (i < 0) continue;
            Card card = GetTossCard(offsets, tossPile[i]);
        }
    }

    private Card GetTossCard(int offsets, int type)
    {
        // Show the actiual cards, last3 thrown 3 2 1
        Card card = ItemCreator.Instance.GenerateCard(type, false); // Set as not in play
        card.SetScale();
        card.transform.parent = TossHolder;
        card.transform.localScale = Vector3.one;
        card.transform.localPosition = new Vector2(offsets * -8, offsets * 8);
        card.enabled = false;
        return card;
    }

    public void DrawCard()
    {
        Debug.Log("Draw card.");
        InventoryController.Instance.DrawDeckCard();
    }
    
    public void TossDeckClicked()
    {
        Debug.Log("Clicking Tossed Cards Pile.");
    }

    internal void PlaceCard(Card mimicedCard)
    {
    }

    public void FillDeckAndToss()
    {
        // Remove any old card
        RemoveOldCards();

        // Use players Card to fill the shown decks
        UpdateDeckStack();
        UpdateTossStack();
    }

    private void RemoveOldCards()
    {
        foreach (Transform child in DeckHolder.transform) {
            Destroy(child.gameObject);
        }
        foreach (Transform child in TossHolder.transform) {
            Destroy(child.gameObject);
        }
    }
}
