using System;
using UnityEngine;

public class GameAreaVisualizer : MonoBehaviour
{
    [SerializeField] private GridPosition[,] grid;
    [SerializeField] private Transform gridPositionHolder;
    [SerializeField] private GridPosition gridPositionPrefab;



    void Start()
    {
        GeneratePlayArea();
    }

    private void GeneratePlayArea()
    {
        // Destroy Old Area
        foreach (Transform t in gridPositionHolder) {
            Destroy(t.gameObject);
        }

        grid = new GridPosition[GameStats.Width, GameStats.Height];

        for (int j = 0; j < GameStats.Height; j++) {
            for (int i = 0; i < GameStats.Width; i++) {
                GridPosition gridPos = Instantiate(gridPositionPrefab, gridPositionHolder);
                grid[i, j] = gridPos;
            }
        }
    }


    private void Update()
    {
        ShowOccupationColors();
    }

    private void ShowOccupationColors()
    {
        for (int j = 0; j < GameStats.Height; j++) {
            for (int i = 0; i < GameStats.Width; i++) {
                Card card = GameAreaController.Instance.PlacedCards[i, j];
                grid[i, j].HighLight(card);
            }
        }
    }
}
