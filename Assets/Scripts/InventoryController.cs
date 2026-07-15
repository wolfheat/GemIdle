using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Wolfheat.StartMenu;

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
        
    internal void PlaceCard(Card mimicedCard, bool unsetOldPosition = true, bool useMousePositionToOrder = true)
    {
        // Place Card in Inventory -  Add it to the Holder - Also keep track of it?
        mimicedCard.transform.parent = itemHolder;
        
        if (useMousePositionToOrder) {
            int order = GetInventoryOrderByMousePosition();
            mimicedCard.transform.SetSiblingIndex(order);
        }

        // Removes from GameArea if present there
        GameAreaController.Instance.RemoveOldPlacement(mimicedCard);

        // Make the card forget its placement as well - Need to happen after
        mimicedCard.UnsetPosition();

        mimicedCard.SetScale();
    }

    private int GetInventoryOrderByMousePosition()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();

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

    public bool DrawDeckCard()
    {
        if (!CanAddCard()) {
            InfoPanel.Instance.ShowInfo("Inventory Full.");
            SoundMaster.Instance.PlaySound(SoundName.PlaceError);
            return false;
        }

        int cardIdOfDrawnCard = Stats.DrawTopCard();

        // Only Draw Card if there is One
        if(cardIdOfDrawnCard >= 0) {
            Card card = ItemCreator.Instance.GenerateCard(cardIdOfDrawnCard, true);
            GameController.Instance.PlaceGeneratedCardInInventory(card);
        }

        // Shuffle
        bool didShuffle = Stats.ShuffleIfNeeded();
        
        return didShuffle;
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
        Debug.Log("GenerateCard "+type);
        Card card = ItemCreator.Instance.GenerateCard(type, subType);
        if(card == null) {
            Debug.Log("No Card Generated");
            return;
        }
        Debug.Log("Generated a Card of type: "+type.ToString()+" and subtype: "+subType+" name: " + card.name); 

        

        GameController.Instance.PlaceGeneratedCardInInventory(card);
    }

    internal bool CanAddCard()
    {
        return (HeldItems < MaxCardsInInventory);
    }
}
