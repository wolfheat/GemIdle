using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Wolfheat.StartMenu;

public class InventoryController : MonoBehaviour
{
    public const int Width = 7;
    public const int Height = 1;
    public const int BoxWidth = 140;
    public const int BoxHeight = 140;

    private GridPosition[,] gridPositions;
    //private Card[,] gridcards;
    
    [SerializeField] private Transform itemHolder;

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



        // removes from GameArea if present there
        GameAreaController.Instance.RemoveOldPlacement(mimicedCard);

        // Make the card forget its placement as well - Need to happen after
        mimicedCard.UnsetPosition();

        // Scale it to fit the Box
        RectTransform rect = mimicedCard.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(BoxWidth, BoxHeight);

        if(unsetOldPosition) // This also acts as a determiner if the pickup sound should be heared or if its enought with the swap sound
            SoundMaster.Instance.PlaySound(SoundName.PickupCard);

    }

    private int GetInventoryOrderByMousePosition()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();

        int finalIndex = 0;
        for (int i = 0; i < itemHolder.childCount; i++) {
            GameObject child = itemHolder.GetChild(i).gameObject;
            Debug.Log("Child: "+i+" Name:" + child.name +  " Active: "+child.activeSelf);

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


    public void GenerateRandomCard(GemType type, int subType = 0)
    {
        // Play sound for button

        SoundMaster.Instance.PlaySound(SoundName.ButtonClick);

        GenerateCard(type, subType);
    }

    private void GenerateCard(GemType type, int subType = 0)
    {
        Card card = ItemCreator.Instance.GenerateCard(type, subType);

        // Place Card in Inventroy -  Add it to the Holder - Also keep track of it?
        card.transform.parent = itemHolder;

        // Scale it to fit the Box
        RectTransform rect = card.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(GameStats.BoxWidth, GameStats.BoxHeight);
        rect.localScale = Vector2.one;
    }
}
