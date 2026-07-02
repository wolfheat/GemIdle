using UnityEngine;



public enum EffectType { StandardIncrement, MultiplyBy}


[CreateAssetMenu(menuName = "ScriptableObjects/CardData", fileName = "CardData")]
public class CardData : ScriptableObject
{
    public EffectType effectType;
    public string Description;
    public Sprite Image;
    public int amt;
}
