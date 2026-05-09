using UnityEngine;

public enum ContractDifficulty
{
    Regular,
    Long,
    Mega
}

[CreateAssetMenu(fileName = "NewContractType", menuName = "ContractSystem/Type")]
public class ContractType : ScriptableObject
{
    [Header("Difficulty Classification")]
    public ContractDifficulty difficulty;
    
    [Header("Quantity Rules")]
    // total quantity is totla number of item i.e 4 yellow, 3 red = 7 total
    public Vector2Int totalQuantityRange = new Vector2Int(2, 5);
    public Vector2Int totalDestinationsRange = new Vector2Int(1, 1);

    [Header("Economy")]
    public Vector2Int rewardRange = new Vector2Int(5, 10);

    //total stop to pickup new tokens
    [Header("Environment")]
    public Vector2Int pickupLocationsRange = new Vector2Int(1, 1);

    /// <summary>
    /// Helper method to grab a random integer within the defined range (Inclusive).
    /// </summary>
    public int GetRandomValue(Vector2Int range)
    {
        // Random.Range for ints is (min, maxExclusive), 
        // so we add 1 to make the Y value inclusive.
        return Random.Range(range.x, range.y + 1);
    }
}