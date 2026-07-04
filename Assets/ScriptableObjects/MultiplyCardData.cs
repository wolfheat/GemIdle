using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/MultiplierCardData", fileName = "MultiplierCardData")]
public class MultiplyCardData : CardData
{    
    public int BaseMultiplier;

    public int CurrentMultiplier { get; internal set; }
}
