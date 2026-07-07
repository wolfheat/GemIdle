using UnityEngine;


public enum MoveDirection { Up, Right, Down, Left };

[CreateAssetMenu(menuName = "ScriptableObjects/MoverCardData", fileName = "MoverCardData")]
public class MoverCardData : CardData
{    
    public MoveDirection moveDirection; 
    public int BaseMover;
}
