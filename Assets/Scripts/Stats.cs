using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Wolfheat.StartMenu;
public enum GemType { Red, Green, Blue , Neutral}

public static class GameStats
{
    public const int Width = 7;
    public const int Height = 5;
    public const int BoxWidth = 140;
    public const int BoxHeight = 140;
    public const float AnimationTime = 0.15f;
    public static float AnimationSpeed = 350f;
    //public const float AnimationSpeed = 1000f;
    public const bool UseSpeed = true;
}

public static class Stats
{
    public static int RedGems = 0;
    public static int GreenGems = 0;
    public static int BlueGems = 0;
    public static Action<GemType> GemUpdate;
    internal static int[] OwnedCards;
    internal static int[] CurrentDeck;

    internal static Stack<int> Deck = new();
    //internal static Stack<int> Toss = new();

    internal static bool AnimateCards = false;

    public static bool IsPaused { get; internal set; }
    public static int TopCardID => Deck.Count == 0 ? -1 : Deck.Peek();
    public static bool DeckHasCards => Deck.Count > 0;
    public static int CurrentDeckCards => CurrentDeck.Sum();

    public static bool MusicIsOn = false;

    public static Action DrawDeckUpdated;
    public static Action TossDeckUpdated;

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

    internal static void ScrambleDeckFromCurrentDeck()
    {
        Deck.Clear();
        //Toss.Clear();
                
        List<int> cards = new List<int>();

        for (int i = 0; i < CurrentDeck.Length; i++) {
            for (int j = 0; j < CurrentDeck[i]; j++) {
                cards.Insert(UnityEngine.Random.Range(0, cards.Count), i);
            }
        }
        Deck = new Stack<int>(cards);

        if(Deck.Count == 0) {
            Deck.Push(1);
            Deck.Push(2);
            Deck.Push(3);
        }


        // Could Use Fisher-Yates Shuffle if allowing many cards in the deck.
    }
    
    internal static void ShuffleTossPileIntoDrawDeck(List<Card> cardsIn)
    {
        // Get Toss Cards
        List<int> cards = cardsIn.Select(x => x.ID).ToList();

        // Could Use Fisher-Yates Shuffle if allowing many cards in the deck.
        cards = FisherYatesShuffle(ref cards);

        // Place Shuffled Toss Cards in Deck
        Deck = new Stack<int>(cards);

        SoundMaster.Instance.PlaySound(cards.Count > 3 ? SoundName.Shuffle : SoundName.ShuffleShort);
    }

    private static List<int> FisherYatesShuffle(ref List<int> cards)
    {
        for (int i = 0; i < cards.Count; i++) {
            int moveToIndex = UnityEngine.Random.Range(0, cards.Count);
            (cards[i], cards[moveToIndex]) = (cards[moveToIndex], cards[i]);
        }
        return cards;
    }
    internal static int DrawTopCard()
    {
        if (Deck.Count == 0) return -1;

        int cardID = Deck.Pop();
        Debug.Log("Drawing top card of Deck: " + cardID+" remaining cards: "+Deck.Count);
        return cardID;
    }

    internal static bool ShuffleNeeded => Deck.Count == 0;

    // Animate to toss pile
    /*
    internal static void AddTossCard(Card card, bool animate = true, Vector3 position = default)
    {
        if (animate) {
            // Don't animate the actual card, use the ghost for this
            Debug.Log("ANIMATE the ghost to the TossPile from: "+position + " to TOssPile "+ DrawCardController.Instance.GetTossPilePosition());
            // Animate Ghost from cards position to TossPile Position
            GameController.Instance.AnimateGhostFromTo(card, position : card.transform.position, DrawCardController.Instance.GetTossPilePosition(), () => TossCardCompleted(card));
        }
        else {
            TossCardCompleted(card);
        }
    }*/

}
