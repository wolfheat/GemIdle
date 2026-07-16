using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Wolfheat.StartMenu;

public class DrawCardController : MonoBehaviour
{
    [SerializeField] private Transform DeckHolder;
    [SerializeField] private Transform TossHolder;
    [SerializeField] private RectTransform TossArea;

    private GameObject[] drawPileVisuals = new GameObject[3];
    private Vector2[] drawPilePositions = new Vector2[3];

    private Vector2 drawPileOffset = new Vector2(-8, 8);

    public Vector2 GetDeckPosition() => DeckHolder.transform.position;

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
        Stats.DrawDeckUpdated += UpdateDeckStackVisuals;
        Stats.TossDeckUpdated += UpdateTossStackVisuals;
    }
    
    private void OnDisable()
    {
        Stats.DrawDeckUpdated -= UpdateDeckStackVisuals;
        Stats.TossDeckUpdated -= UpdateTossStackVisuals;
    }

    private void Start()
    {
        // Initiate the Deck Pile, then just enable or disable the visuals of it
        GenerateDeckVisuals();
        FillDeckAndToss();
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
            drawPilePositions[i] = offsets * drawPileOffset;

            BackSideCard.transform.localPosition = drawPilePositions[i];

            BackSideCard.enabled = false;
            drawPileVisuals[i] = BackSideCard.gameObject;

            //BackSideCard.AnimateToPosition(endPos, null, true);
        }
    }

    private void UpdateDeckStackVisuals()
    {
        // Just show the correct parts of the visuals
        int amt = Math.Min(3, Stats.Deck.Count);
        
        Debug.Log("DECK: Deck Visuals should show " + amt + " cards.");
        
        int offset = 3;
        for (int i = 0; i < 3; i++) {
            offset--;
            
            Debug.Log("DECK: Card: " + i+ " = Active: "+ (amt > i));
            
            drawPileVisuals[i].SetActive(amt > offset ? true : false);

            if (!drawPileVisuals[i].activeSelf) continue;

            // Only Animate for visuable cards
            // Set them at ther prev position and animate them into place       

            Card card = drawPileVisuals[i].GetComponent<Card>();

            if (i > 0) { // Thord card never moves into position

                card.transform.localPosition = drawPilePositions[i] + new Vector2(-8,8);
                card.AnimateToPosition(drawPilePositions[i], null, true);
            }
        }
    }
    
    private void UpdateTossStackVisuals()
    {   
        RemoveOldCards();
        List<int> tossPile = new List<int>(Stats.Toss);
        tossPile.Reverse();
        Debug.Log("TOSS: Updating TossPile cards: " + tossPile.Count);
        int amt = tossPile.Count;
        int startIndex = amt - 3;
        int offsets = 3;
        for (int i = startIndex; i < amt; i++) {
            Debug.Log("TOSS: Index: " + i+"  offsets:" + offsets);
            offsets--;
            
            if (i < 0) continue;

            Debug.Log("TOSS: Index: " + i+"  Create Card!");

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

        int cardIdOfDrawnCard = Stats.TopCardID;

        bool canDraw = InventoryController.Instance.CanAddCard();

        if (!canDraw || cardIdOfDrawnCard < 0) {

            // Shuffle
            if (Stats.ShuffleIfNeeded()) {
                UpdateDecks(true);
                return;
            }

            InfoPanel.Instance.ShowInfo("Inventory Full or No Cards In Toss Pile.");
            SoundMaster.Instance.PlaySound(SoundName.PlaceError);   
            return;
        }

        Stats.DrawTopCard();

        // Only Create a Card if there is One in the deck
        Card card = ItemCreator.Instance.GenerateCard(cardIdOfDrawnCard, true);
        GameController.Instance.PlaceGeneratedCardInInventory(card, DeckHolder.position);

        // Shuffle
        bool didShuffle = Stats.ShuffleIfNeeded();    
    
        // Also Animate the DrawPile Cards
        UpdateDecks(didShuffle);
    }

    private void UpdateDecks(bool didShuffle)
    {
        UpdateDeckStackVisuals();

        if (didShuffle)
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

    internal Vector2 GetTossPilePosition() => TossHolder.transform.position;
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
