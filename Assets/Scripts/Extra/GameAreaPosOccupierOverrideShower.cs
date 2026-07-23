using UnityEngine;

public class GameAreaPosOccupierOverrideShower : MonoBehaviour
{



    private void Update()
    {
        GridPosition[,] positions = GameAreaController.Instance.GridPositions;
        Card[,] placed = GameAreaController.Instance.PlacedCards;

        for (int j = 0; j < GameStats.Height; j++) {
            for (int i = 0; i < GameStats.Width; i++) {
                positions[i, j].ShowOccupant(placed[i,j]);
            }
        }

    }
}
