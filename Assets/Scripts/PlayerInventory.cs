using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "PlayerInventory", menuName = "Game/Player Inventory")]
public class PlayerInventory : ScriptableObject
{
    public int currentScoreMultiplier = 1;
    public List<TileData> stackedTiles = new List<TileData>();

    public int currentTravelTokens;
    public int startingTravelTokens = 35;

    [System.NonSerialized]
    public UnityEvent OnInventoryChanged = new UnityEvent();

    public void AddTile(TileData tile)
    {
        stackedTiles.Add(tile);
        OnInventoryChanged?.Invoke();
    }

    public void ClearInventory()
    {
        stackedTiles.Clear();
        OnInventoryChanged?.Invoke();
        currentScoreMultiplier = 1;
        currentTravelTokens = startingTravelTokens;
    }

    public void SetTravelTokens(int input)
    {
        currentTravelTokens += input;
    }

    public int GetTravelTokens()
    {
        return currentTravelTokens;
    }
}