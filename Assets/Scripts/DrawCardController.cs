using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DrawCardController : MonoBehaviour
{
    [SerializeField] private Transform DeckHolder;
    [SerializeField] private Transform TossHolder;
    [SerializeField] private RectTransform TossArea;

    private GameObject[] drawPileVisuals = new GameObject[3];


    public static DrawCardController Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }


    private void OnEnable()
    {
        Stats.DrawDeckUpdated += FillDeckAndToss;
    }
    
    private void OnDisable()
    {
        Stats.DrawDeckUpdated -= FillDeckAndToss;
    }

    private void Start()
    {
        // Initiate the Deck Pile, then just enable or disable the visuals of it
        GenerateDeckVisuals();
    }

    private void GenerateDeckVisuals()
    {
        int amt = 3;
        int offsets = 3;
        Debug.Log("Generate Deck Visuals to have " + amt + " cards.");
        for (int i = 0; i < amt; i++) {
            offsets--;
            Card BackSideCard = ItemCreator.Instance.GenerateDeckCard();
            BackSideCard.SetScale();
            BackSideCard.transform.parent = DeckHolder;
            BackSideCard.transform.localScale = Vector3.one;
            BackSideCard.transform.localPosition = new Vector2(offsets * -8, offsets * 8);
            BackSideCard.enabled = false;
            drawPileVisuals[offsets] = BackSideCard.gameObject;
        }
    }

    private void UpdateDeckStackVisuals()
    {
        // Just show the correct parts of the visuals
        int amt = Math.Min(3, Stats.Deck.Count);
        
        Debug.Log("Deck Visuals should show " + amt + " cards.");
        
        for (int i = 0; i < 3; i++) {
            Debug.Log("Deck card " + i + " amt > (2-i) = " + (amt > (2 - i)));
            drawPileVisuals[i].SetActive(amt > i ? true : false);
        }
    }
    
    private void UpdateTossStackVisuals()
    {
        RemoveOldCards();
        List<int> tossPile = new List<int>(Stats.Toss);
        tossPile.Reverse();
        Debug.Log("Updating TossPile cards: " + tossPile.Count);
        int amt = tossPile.Count;
        int startIndex = amt - 3;
        int offsets = 3;
        for (int i = startIndex; i < amt; i++) {
            Debug.Log("Index: " + i+"  offsets:" + offsets);
            offsets--;
            
            if (i < 0) continue;

            Debug.Log("Index: " + i+"  Create Card!");

            Card card = GetTossCardVisual(offsets, tossPile[i]);
        }

        // If there is no toss cards, thene there is no option to shuffle them ? 
    }

    private Card GetTossCardVisual(int offsets, int type)
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
        bool didShuffle = InventoryController.Instance.DrawDeckCard();

        UpdateDeckStackVisuals();

        if(didShuffle)
            UpdateTossStackVisuals();
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
        UpdateDeckStackVisuals();
        UpdateTossStackVisuals();
    }

    private void RemoveOldCards()
    {
        //foreach (Transform child in DeckHolder.transform) {
        //    Destroy(child.gameObject);
        //}
        foreach (Transform child in TossHolder.transform) {
            Destroy(child.gameObject);
        }
    }

    internal Vector2 GetTossPilePosition(bool animate) => TossHolder.transform.position;
    internal bool IsOverTossArea()
    {
        Debug.Log("Checking if over Toss Area");
        // TossArea is the Recttransform to check
        // Return if mouse is over it
        Vector2 mousePosition = Mouse.current.position.ReadValue();

        return RectTransformUtility.RectangleContainsScreenPoint(
            TossArea,
            mousePosition
        );
    }
}
