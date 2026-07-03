using UnityEngine;



public enum EffectType { StandardIncrement, MultiplyBy, Gainer}


[CreateAssetMenu(menuName = "ScriptableObjects/CardData", fileName = "CardData")]
public class CardData : ScriptableObject
{
    //public EffectType effectType;
    public GemType type;
    public string Description;
    public Sprite Image;
    public int baseIncome;
}
