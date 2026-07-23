using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Wolfheat.StartMenu;
using static UnityEngine.UI.Image;

public class InventoryState
{

    // Part to animate Inventory
    public List<Card> cards;
    public List<Vector2> cardsOrigin;

    //public InventoryState(){}
}

public class InventoryController : MonoBehaviour
{
    public const int Width = 7;
    public const int Height = 1;
    public const int BoxWidth = 140;
    public const int BoxHeight = 140;

    public const int MaxCardsInInventory = 12;

    private GridPosition[,] gridPositions;
    //private Card[,] gridcards;
    
    [SerializeField] private Transform itemHolder;
    [SerializeField] private GameObject tempCard;

    public int HeldItems => itemHolder.transform.childCount;

    public static InventoryController Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    internal void PlaceCard(Card card, Vector2 origin, bool useMousePositionForPlacement = true) // Only do this if it is valid = just execute
    {
        Debug.Log("Place a Card - Inventory ");

        //origin = card.transform.position;
        
        // Hide card, should be done automatically for all cards?
        card.HideVisuals();


        InventoryState state = StoreState();

        card.transform.parent = itemHolder;

        if (useMousePositionForPlacement) {
            int order = GetInventoryOrderByMousePosition();
            card.transform.SetSiblingIndex(order);
        }

        // Position Fix
        card.transform.localPosition = Vector2.zero;
        // Needed ??
        //card.SetPlace(-1, -1);
        //card.SetScale();
        card.ReactivateCard();
        card.UnsetPositionIndex();

        // Wait one frame for layoutgroup update to get new target position
        StartCoroutine(DelayedAnimationRoutine());


        IEnumerator DelayedAnimationRoutine()
        {
            yield return null; // All cards end Up in their new position here by the layoutgroup auto ordering

            for (int i = 0; i < state.cards.Count; i++) {
                GameController.Instance.AnimateGhostFromTo(state.cards[i], state.cardsOrigin[i]);
            }

            Debug.Log("Animate Ghost Here");
            GameController.Instance.AnimateGhostFromTo(card, origin);
            //GameController.Instance.AnimateGhostFromTo(card, copy, startPos, cardToPlace.transform.position, null);
        }
    }

    private InventoryState StoreState()
    {
        // Part to animate Inventory
        List<Card> cards = itemHolder.GetComponentsInChildren<Card>().ToList();
        List<Vector2> cardsOrigin = (cards.Select(x => (Vector2)x.transform.position)).ToList();

        for (int i = 0; i < cards.Count; i++) {
            cards[i].HideVisuals();
        }
        return new InventoryState() { cards = cards, cardsOrigin = cardsOrigin };
    }
    


    private int GetInventoryOrderByMousePosition()
    {
        Vector2 mousePos = MouseUtils.MousePosition;

        int finalIndex = 0;
        for (int i = 0; i < itemHolder.childCount; i++) {
            GameObject child = itemHolder.GetChild(i).gameObject;
            
            if (!child.activeSelf) {
                continue;
            }
            if(mousePos.x<itemHolder.GetChild(i).transform.position.x)
                return finalIndex;
            finalIndex++;
        }
        return itemHolder.childCount;
    }


    public void GenerateRedCard() => GenerateRandomCard(GemType.Red);
    public void GenerateGreenCard() => GenerateRandomCard(GemType.Green);
    public void GenerateBlueCard() => GenerateRandomCard(GemType.Blue);

    public void GenerateRedGainCard() => GenerateRandomCard(GemType.Red, 1);
    public void GenerateGreenGainCard() => GenerateRandomCard(GemType.Green, 1);
    public void GenerateBlueGainCard() => GenerateRandomCard(GemType.Blue, 1);

    public void GenerateMultCard() => GenerateRandomCard(GemType.Neutral,0);
    public void GenerateMoveRightCard() => GenerateRandomCard(GemType.Neutral,1);
    public void GenerateAddCard() => GenerateRandomCard(GemType.Neutral,2);


    public void GenerateRandomCard(GemType type, int subType = 0)
    {
        if(HeldItems >= MaxCardsInInventory) {
            InfoPanel.Instance.ShowInfo("Inventory Full");
            SoundMaster.Instance.PlaySound(SoundName.PlaceError);
            return;
        }



        // Play sound for button
        SoundMaster.Instance.PlaySound(SoundName.ButtonClick);

        GenerateCard(type, subType);
    }

    private void GenerateCard(GemType type, int subType = 0)
    {
        //Debug.Log("GenerateCard "+type);
        Card card = ItemCreator.Instance.GenerateCard(type, subType);
        if(card == null) {
            Debug.Log("No Card Generated");
            return;
        }
        Debug.Log("Generating Card: "+subType+" name: " + card.name); 

        GameController.Instance.PlaceGeneratedCardInInventory(card, DrawCardController.Instance.GetDeckPosition());
    }

    internal bool CanAddCard() => (HeldItems < MaxCardsInInventory);

    internal Vector2 GetPosition() => itemHolder.transform.position;

    internal void StoreAndAnimateInventory()
    {

        InventoryState state = StoreState();

        // Wait one frame for layoutgroup update to get new target position
        StartCoroutine(DelayedAnimationRoutine());

        IEnumerator DelayedAnimationRoutine()
        {
            yield return null; // All cards end Up in their new position here by the layoutgroup auto ordering

            for (int i = 0; i < state.cards.Count; i++) {
                GameController.Instance.AnimateGhostFromTo(state.cards[i], state.cardsOrigin[i]);
            }
        }
    }
}
