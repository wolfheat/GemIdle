using System;
using UnityEngine;
public enum GemType { Red, Green, Blue }


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
