using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeliveryHandler : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button deliveryButton;

    [Header("Dependencies")]
    private TokenDestination currentDestination;
    private PlayerInventory playerInventory;

    private bool isPlayerHere = false;

    private string lastDeliveredDestinationID = "";
    private string lastDeliveredTileID = ""; // Assumes your TileData has a string field like tileID or destinationID

    private GameController gameController;

    void Start()
    {
        gameController = FindFirstObjectByType<GameController>();
        playerInventory = gameController.GetPlayerInventory();

        if (deliveryButton != null)
        {
            deliveryButton.onClick.AddListener(ExecuteDelivery);
        }
        
        if (playerInventory != null)
        {
            playerInventory.OnInventoryChanged.AddListener(EvaluateDeliveryValidity);
        }
        
        UpdateButtonState(false);
    }

    void OnDestroy()
    {
        if (playerInventory != null)
        {
            playerInventory.OnInventoryChanged.RemoveListener(EvaluateDeliveryValidity);
        }
    }

    public void SetCurrentDestination(TokenDestination destination)
    {
        currentDestination = destination;
        EvaluateDeliveryValidity(); 
    }

    private void EvaluateDeliveryValidity()
    {
        if (gameController.GetPaused() || !isPlayerHere || playerInventory == null || currentDestination == null || deliveryButton == null)
        {
            UpdateButtonState(false);
            return;
        }

        List<TileData> heldTiles = playerInventory.stackedTiles;

        if (heldTiles != null && heldTiles.Count > 0)
        {
            TileData topTile = heldTiles[heldTiles.Count - 1];
            bool isValidDelivery = topTile.destinationID == currentDestination.destinationID;
            
            UpdateButtonState(isValidDelivery);
        }
        else
        {
            UpdateButtonState(false);
        }
    }

    private void UpdateButtonState(bool isInteractable)
    {
        if (deliveryButton == null) return;
        deliveryButton.interactable = isInteractable;
    }

    private void ExecuteDelivery()
    {
        if (playerInventory == null || currentDestination == null) return;

        List<TileData> heldTiles = playerInventory.stackedTiles;
        
        if (heldTiles != null && heldTiles.Count > 0)
        {
            int topIndex = heldTiles.Count - 1;
            TileData deliveredTile = heldTiles[topIndex];
            
            if (deliveredTile.destinationID == currentDestination.destinationID)
            {
                // --- MULTIPLIER STREAK LOGIC ---
                // Check if player hasn't moved (same destination) AND it's the same type of tile ID
                // NOTE: Change 'deliveredTile.destinationID' to 'deliveredTile.tileID' if your asset uses a unique asset ID field!
                if (currentDestination.destinationID == lastDeliveredDestinationID && 
                    deliveredTile.destinationID == lastDeliveredTileID)
                    {
                                     //if current score multiplier(2) is higher than current highest high score(1) increase score
                        if(playerInventory.currentScoreMultiplier == gameController.currentHighestSteak) 
                        {
                            playerInventory.currentScoreMultiplier += 1;
                        
                            Debug.Log($"Streak active! Multiplier increased to: x{playerInventory.currentScoreMultiplier}");
                        }  
                        gameController.currentHighestSteak += 1;                                              
                    }                

                // Record this delivery's information for the next round's comparison
                lastDeliveredDestinationID = currentDestination.destinationID;
                lastDeliveredTileID = deliveredTile.destinationID; 
                // -------------------------------

                if (gameController != null)
                {
                    // 1. Roll the random base reward fresh right here on click
                    int freshlyRolledBaseReward = deliveredTile.GetRandomBaseReward();
                    int calculatedPoints = Mathf.RoundToInt(playerInventory.currentScoreMultiplier * freshlyRolledBaseReward);
                    
                    // 2. Hand it off to GameController to accumulate and flash on screen
                    gameController.AddScore(calculatedPoints, playerInventory.currentScoreMultiplier, freshlyRolledBaseReward);
                }

                // 3. Remove from inventory stack
                heldTiles.RemoveAt(topIndex);
                playerInventory.OnInventoryChanged?.Invoke(); 

                Debug.Log($"Successfully delivered cargo to {currentDestination.destinationID}!");
            }
        }
    }

    public void OnPlayerEnterZone()
    {
        isPlayerHere = true;
        EvaluateDeliveryValidity();
    }

    public void OnPlayerExitZone()
    {
        isPlayerHere = false;
        currentDestination = null; 
        UpdateButtonState(false);

        // Optional: Break the combo entirely if they walk away from the hex zone
        lastDeliveredDestinationID = "";
        lastDeliveredTileID = "";
        gameController.currentHighestSteak = 1;
    }
}