using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewLevelProfile", menuName = "Procedural/Level Profile")]
public class ProceduralLevelProfile : ScriptableObject
{
    // Assign your special TileData assets (Red, Blue, Yellow SOs) here in the inspector
    public int levelID;

    public List<TileData> specialTiles; 

    public int minTilesAvailable = 2;
    public int maxTilesAvailable = 5;

    public int hexAmount = 35;

    public float preferredDistanceBetweenDests = 3f;

    public float destTokenRefreshCooldown = 10f;

    public List<TileData> GetSpecialTiles() => specialTiles;

    public TileData GetRandomTile()
    {
        if (specialTiles == null || specialTiles.Count == 0) return null;
        
        int index = Random.Range(0, specialTiles.Count);
        return specialTiles[index];
    }

    public int levelMaxScore;

    public int travelTokenReward;
}