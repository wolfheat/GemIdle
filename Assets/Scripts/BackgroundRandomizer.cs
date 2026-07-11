using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BackgroundRandomizer : MonoBehaviour
{

    [SerializeField] private Image backgroundImage; 

    [SerializeField] private Sprite[] availableBackgroundSprites; 
    [SerializeField] private Color[] availableBackgroundColors; 
    
    [SerializeField] private bool lockColor; 
    [SerializeField] private bool lockImage; 
    [SerializeField] private bool lockSize;

    [Range(0.1f,3f)]
    [SerializeField] private float imageSize; 
    
    private float imageSizeMin = 0.1f; 
    private float imageSizeMax = 3f;

    public void RandomizeBackground()
    {
        if (!lockSize) {
            imageSize = Random.Range(imageSizeMin, imageSizeMax);
            backgroundImage.pixelsPerUnitMultiplier = imageSize;
        }
        if (!lockImage) {
            backgroundImage.sprite =  availableBackgroundSprites[Random.Range(0, availableBackgroundSprites.Length)];
        }
        if (!lockColor) {
            backgroundImage.color =  availableBackgroundColors[Random.Range(0, availableBackgroundColors.Length)];
        }
    }
}
