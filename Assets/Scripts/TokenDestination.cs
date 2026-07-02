using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Variables are injected from HexAreaHandler
public class TokenDestination : MonoBehaviour
{
    // 1. EVENT FOR REFRESH LOOP
    public System.Action<TokenDestination> OnDestinationEmpty;

    public string destinationID; 
    [SerializeField] private List<TileData> availableTiles = new List<TileData>();
    private PlayerInventory playerInventory;

    private GameObject uiPanelPrefab;            
    private GameObject buttonPrefab; 

    // Find the DeliveryHandler component sitting alongside the UI or Hex
    // Inside TokenDestination.cs
    [HideInInspector] public DeliveryHandler deliveryHandler; // HexAreaHandler will fill this automatically

    public void Initialize(string id, List<TileData> tilesToGive, PlayerInventory inventory, GameObject uiPrefab, GameObject btnPrefab)
    {
        destinationID = id;
        availableTiles = tilesToGive;
        playerInventory = inventory;
        uiPanelPrefab = uiPrefab;
        buttonPrefab = btnPrefab;

        playerLayer = LayerMask.GetMask("Player");
        
        Debug.Log($"[TokenDestination] Initialized ID: {destinationID} with {availableTiles.Count} tiles.");
    }

    private bool wasPlayerDetected = false;

    [SerializeField] private float rayDistance = 5f;
    [SerializeField] private LayerMask playerLayer; 

    void Update()
    {
        Ray ray = new Ray(transform.position, transform.up);
        RaycastHit hit;

        bool isPlayerDetectedCurrently = false;

        if (Physics.Raycast(ray, out hit, rayDistance, playerLayer))
        {
            if (hit.collider.CompareTag("Player"))
            {
                isPlayerDetectedCurrently = true;
            }
        }

        Color rayColor = isPlayerDetectedCurrently ? Color.green : Color.red;
        Debug.DrawRay(transform.position, transform.up * rayDistance, rayColor);

        // 1. Just entered the zone (Mimics OnTriggerEnter)
        if (isPlayerDetectedCurrently && !wasPlayerDetected)
        {
            Debug.Log("[TokenDestination] Player ENTERED raycast detection.");
            
            // Tell the delivery handler exactly which destination the player is standing on right now
            if (deliveryHandler != null)
            {
                deliveryHandler.SetCurrentDestination(this); // <-- ADD THIS LINE
                deliveryHandler.OnPlayerEnterZone();         // <-- ADD THIS LINE
            }
            
            if (availableTiles.Count > 0)
            {
                GenerateTokenUI();
            }
            else
            {
                Debug.LogWarning("[TokenDestination] Player entered, but availableTiles count is 0.");
            }
        }
        // 2. Just left the zone (Mimics OnTriggerExit)
        else if (!isPlayerDetectedCurrently && wasPlayerDetected)
        {
            Debug.Log("[TokenDestination] Player EXITED raycast detection.");
            
            // Clear the destination reference when the player walks away
            if (deliveryHandler != null)
            {
                deliveryHandler.OnPlayerExitZone();          // <-- ADD THIS LINE
            }

            CleanUpUI();
        }
        wasPlayerDetected = isPlayerDetectedCurrently;
    }

    private void GenerateTokenUI()
    {
        Debug.Log("[TokenDestination] Generating Token UI...");
        CleanUpUI();        

        foreach (TileData tile in availableTiles)
        {
            GameObject btnObj = Instantiate(buttonPrefab, uiPanelPrefab.transform);
            TilePlayableButton tileBtn = btnObj.GetComponent<TilePlayableButton>();
            
            if (tileBtn != null)
            {
                tileBtn.SetupButton(tile, playerInventory);
                
                Button btnComponent = btnObj.GetComponent<Button>();
                TileData capturedTile = tile; 
                btnComponent.onClick.AddListener(() => RemoveTileFromDestination(capturedTile));
            }
        }
    }

    private void RemoveTileFromDestination(TileData tile)
    {
        Debug.Log($"[TokenDestination] Removing tile from available registry. Remaining before removal: {availableTiles.Count}");
        availableTiles.Remove(tile);
        
        if (availableTiles.Count == 0)
        {
            Debug.Log("[TokenDestination] All tiles claimed. Cleaning up UI.");
            CleanUpUI();

            // 2. TRIGGER THE REFRESH TIMER IN THE MANAGER
            OnDestinationEmpty?.Invoke(this);
        }
    }

    // 3. RECEIVE FRESH TILES FROM HEXAREAHANDLER TIMER
    public void RefreshTokens(List<TileData> newTiles)
    {
        availableTiles = newTiles;
        Debug.Log($"[TokenDestination] {destinationID} successfully refreshed with {availableTiles.Count} new tiles!");

        // If the player is standing directly on this tile when it refreshes, update the UI immediately
        if (wasPlayerDetected && availableTiles.Count > 0)
        {
            GenerateTokenUI();
        }
    }

    private void CleanUpUI()
    {
        Debug.Log("[TokenDestination] Cleaning up UI. Deleting all child buttons.");

        if (uiPanelPrefab != null)
        {
            Transform parentTransform = uiPanelPrefab.transform;

            for (int i = parentTransform.childCount - 1; i >= 0; i--)
            {
                Destroy(parentTransform.GetChild(i).gameObject);
            }
        }
    }
}