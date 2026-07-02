using UnityEngine;

[CreateAssetMenu(fileName = "NewTileData", menuName = "Game/Tile Data")]
public class TileData : ScriptableObject
{
    public string tileName;
    public Color tileColor;
    public string destinationID; // The role/tag this needs to go to (e.g., "RED_STOP")

    public int minReward = 8;

    public int maxReward = 10;

    public int GetRandomBaseReward()
    {      
        return Random.Range(minReward, maxReward);
    }
}