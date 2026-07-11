using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public enum GemType { Red, Green, Blue , Neutral}

public static class GameStats
{
    public const int Width = 7;
    public const int Height = 5;
    public const int BoxWidth = 140;
    public const int BoxHeight = 140;
}



public static class Stats
{
    public static int RedGems = 0;
    public static int GreenGems = 0;
    public static int BlueGems = 0;
    public static Action<GemType> GemUpdate;
    internal static int[] OwnedCards;
    internal static int[] CurrentDeck;
    internal static bool AnimateCards = false;

    public static bool IsPaused { get; internal set; }
    public static int CurrentDeckCards => CurrentDeck.Sum();

    public static void Initiate()
    {
        Debug.Log("Stats Initiate");
        int totalAmtCards = ItemCreator.Instance.TotalCardAmount();


        // Initiate the Card Owned Array - Later load this from file
        OwnedCards = new int[totalAmtCards];
        CurrentDeck = new int[totalAmtCards];
        OwnedCards[0] = 5;
        OwnedCards[1] = 4;
        OwnedCards[2] = 4;
        OwnedCards[3] = 2;
        OwnedCards[4] = 2;

        CurrentDeck[0] = 1;
        CurrentDeck[1] = 4;
        CurrentDeck[2] = 1;

    }

    public static void AddGems(GemType type, int amt = 1)
    {
        switch (type) {
            case GemType.Red:
                RedGems+= amt;
                GemUpdate?.Invoke(GemType.Red);
                break;
            case GemType.Green:
                GreenGems+= amt;
                GemUpdate?.Invoke(GemType.Green);
                break;
            case GemType.Blue:
                BlueGems+= amt;
                GemUpdate?.Invoke(GemType.Blue);
                break;
            default:
                return;
        }


    }

    internal static int GetGemAmount(GemType itemType)
    {
        switch (itemType) {
            case GemType.Red:
                return RedGems;
            case GemType.Green:
                return GreenGems;
            case GemType.Blue:
                return BlueGems;
            default:
                return 0;
        }
    }
}
