using UnityEngine;
using System.Collections.Generic;

public class HexAreaHandler : MonoBehaviour
{  
    [Header("Prefabs")]
    public GameObject hexPrefab;
    public GameObject playerPrefab;

    [Header("Grid Settings")]
    public float hexSize = 1f;
    private readonly Vector2Int[] directions = {
        new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(-1, 1),
        new Vector2Int(-1, 0), new Vector2Int(0, -1), new Vector2Int(1, -1)
    };

    [Header("Tile Stack Game Data")]
    private GameController gameController; 
    private ProceduralLevelProfile levelProfile; 
    private PlayerInventory playerInventory;     

    [Header("UI Prefabs for Destinations")]
    public GameObject uiPanelPrefab;            
    public GameObject buttonPrefab;  
    public DeliveryHandler deliveryHandler;           

    [HideInInspector] public Dictionary<Vector2Int, GameObject> spawnedHexes = new Dictionary<Vector2Int, GameObject>();
    [HideInInspector] public List<Vector2Int> specialPoints = new List<Vector2Int>();
    [HideInInspector] public Dictionary<Vector2Int, string> specialPointRoles = new Dictionary<Vector2Int, string>();
    
    private List<Vector2Int> availableSpots = new List<Vector2Int>();

    // Tracking active destinations and active countdown timers
    private List<TokenDestination> activeDestinations = new List<TokenDestination>();
    private Dictionary<TokenDestination, float> activeRefreshTimers = new Dictionary<TokenDestination, float>();

    public void Awake()
    {
        gameController = FindFirstObjectByType<GameController>();
        //levelProfile = gameController.GetLevelProfile();
        playerInventory = gameController.GetPlayerInventory();
    }

    
    
    public void SpawnNewLevel(ProceduralLevelProfile level)
    {
        levelProfile = level;
        GenerateNextArea(Vector2Int.zero);
        SpawnPlayer(Vector2Int.zero);
    }

    void Update()
    {
        HandleActiveTimers();
    }    

    public void GenerateNextArea(Vector2Int startPoint)
    {
        ClearOldGrid(startPoint);
        ClearAllPlayer();
        
        ResetTrackingCollections(startPoint);
        BuildRandomHexClump(startPoint);
        SpawnSpecialPoints(startPoint);
    }

    // --- Generation & Tracking Sub-Steps ---

    private void ResetTrackingCollections(Vector2Int startPoint)
    {
        // Unsubscribe from old destinations to avoid memory leaks before clearing
        foreach (var dest in activeDestinations)
        {
            if (dest != null) dest.OnDestinationEmpty -= HandleDestinationEmpty;
        }

        specialPoints.Clear();
        specialPointRoles.Clear();
        activeDestinations.Clear();
        activeRefreshTimers.Clear();

        specialPoints.Add(startPoint);  
    }

    private void BuildRandomHexClump(Vector2Int startPoint)
    {
        UpdateAvailableSpots(startPoint);

        for (int i = 1; i < levelProfile.hexAmount; i++)
        {
            if (availableSpots.Count == 0) break;

            int randomIndex = Random.Range(0, availableSpots.Count);
            Vector2Int nextCoord = availableSpots[randomIndex];

            SpawnHex(nextCoord);
            availableSpots.RemoveAt(randomIndex);
            UpdateAvailableSpots(nextCoord);
        }
    }

    private void SpawnSpecialPoints(Vector2Int startPoint)
    {
        List<TileData> specialTileConfigs = levelProfile.GetSpecialTiles(); 

        foreach (TileData tileDataAsset in specialTileConfigs)
        {
            if (tileDataAsset != null)
            {
                PlaceSpecialPoint(startPoint, tileDataAsset);
            }
        }
    }

    void PlaceSpecialPoint(Vector2Int origin, TileData config, float minDistanceFromOthers = 3f)
    {
        if (!FindBestCoordinate(origin, config, minDistanceFromOthers, out Vector2Int bestCoord))
        {
            return; 
        }

        specialPoints.Add(bestCoord); 
        specialPointRoles.Add(bestCoord, config.destinationID); 

        GameObject hex = spawnedHexes[bestCoord];
        ConfigureHexVisuals(hex, config);

        List<TileData> generatedTiles = GenerateAlternativeTiles(config);
        
        // Inside HexAreaHandler.cs -> PlaceSpecialPoint()
        TokenDestination destScript = hex.AddComponent<TokenDestination>();

        // Pass the delivery handler reference directly to the component right here!
        destScript.deliveryHandler = this.deliveryHandler; 

        // Track the destination and subscribe to its empty event
        activeDestinations.Add(destScript);
        
        // Track the destination and subscribe to its empty event
        activeDestinations.Add(destScript);
        destScript.OnDestinationEmpty += HandleDestinationEmpty;

        destScript.Initialize(config.destinationID, generatedTiles, playerInventory, uiPanelPrefab, buttonPrefab);

    }

    // --- Dynamic Refresh Timer Logic ---

    private void HandleDestinationEmpty(TokenDestination emptyDestination)
    {
        // Start a refresh timer for this specific destination if one isn't already active
        if (!activeRefreshTimers.ContainsKey(emptyDestination))
        {
            activeRefreshTimers.Add(emptyDestination, levelProfile.destTokenRefreshCooldown);
            Debug.Log($"Destination {emptyDestination.destinationID} is empty! Starting a {levelProfile.destTokenRefreshCooldown}s refresh timer.");
        }
    }

    private void HandleActiveTimers()
    {
        if (activeRefreshTimers.Count == 0) return;

        // Create a list of keys to safely modify the dictionary while iterating
        List<TokenDestination> destinationsToUpdate = new List<TokenDestination>(activeRefreshTimers.Keys);

        foreach (var dest in destinationsToUpdate)
        {
            if (dest == null)
            {
                activeRefreshTimers.Remove(dest);
                continue;
            }

            // Countdown
            activeRefreshTimers[dest] -= Time.deltaTime;

            if (activeRefreshTimers[dest] <= 0f)
            {
                RefreshSingleDestination(dest);
                activeRefreshTimers.Remove(dest); // Remove timer once executed
            }
        }
    }

    private void RefreshSingleDestination(TokenDestination destScript)
    {
        // Find matching configuration data using its ID
        TileData config = levelProfile.GetSpecialTiles().Find(t => t.destinationID == destScript.destinationID);
        
        if (config != null)
        {
            List<TileData> newTiles = GenerateAlternativeTiles(config);
            destScript.RefreshTokens(newTiles); // Method inside TokenDestination to swap data & redraw UI
            Debug.Log($"Successfully refreshed tokens for {destScript.destinationID}");
        }
    }

    // --- Core Hex Utilities & Helpers ---

    private bool FindBestCoordinate(Vector2Int origin, TileData config, float minDistance, out Vector2Int bestCoord)
    {
        bestCoord = origin;
        float lowestDiff = float.MaxValue; 
        bool foundValidSpot = false;

        foreach (var coord in spawnedHexes.Keys)
        {
            if (IsTooCloseToExistingSpecialPoints(coord, minDistance)) continue;

            float distFromOrigin = Vector2Int.Distance(origin, coord);
            float diff = Mathf.Abs(distFromOrigin - levelProfile.preferredDistanceBetweenDests);

            if (diff < lowestDiff)
            {
                lowestDiff = diff;
                bestCoord = coord;
                foundValidSpot = true;
            }
        }

        return foundValidSpot;
    }

    private bool IsTooCloseToExistingSpecialPoints(Vector2Int coord, float minDistance)
    {
        foreach (var existing in specialPoints)
        {
            if (Vector2Int.Distance(coord, existing) < minDistance) return true;
        }
        return false;
    }

    private void ConfigureHexVisuals(GameObject hex, TileData config)
    {
        hex.GetComponentInChildren<Renderer>().material.color = config.tileColor;
        hex.name = config.destinationID;
        hex.tag = "Dest";

        foreach (Transform child in hex.transform) child.tag = "Dest";
    }

    private List<TileData> GenerateAlternativeTiles(TileData config)
    {
        List<TileData> generatedTiles = new List<TileData>();
        
        if (levelProfile.GetSpecialTiles().Count <= 1) return generatedTiles;

        int tilesToGive = Random.Range(levelProfile.minTilesAvailable, levelProfile.maxTilesAvailable); 
        int attempts = 0;
        int maxAttempts = 20; 

        while (generatedTiles.Count < tilesToGive && attempts < maxAttempts)
        {
            attempts++;
            TileData randomTile = levelProfile.GetRandomTile();

            if (randomTile != null)
            {
                if (randomTile == config || randomTile.destinationID == config.destinationID) continue;
                generatedTiles.Add(randomTile);
            }
        }

        return generatedTiles;
    }

    void UpdateAvailableSpots(Vector2Int center)
    {
        foreach (var dir in directions)
        {
            Vector2Int neighbor = center + dir;
            if (!spawnedHexes.ContainsKey(neighbor) && !availableSpots.Contains(neighbor))
            {
                availableSpots.Add(neighbor);
            }
        }
    }

    void SpawnHex(Vector2Int coord)
    {
        float x = hexSize * Mathf.Sqrt(3) * (coord.x + coord.y / 2f);
        float z = hexSize * 1.5f * coord.y;

        GameObject hex = Instantiate(hexPrefab, new Vector3(x, 0, z), Quaternion.identity, transform);
        hex.transform.GetChild(0).name = $"Hex_{coord.x}_{coord.y}";
        spawnedHexes.Add(coord, hex);
    }

    void ClearOldGrid(Vector2Int keepCoord)
    {
        if (spawnedHexes.TryGetValue(keepCoord, out GameObject keptObject))
        {
            foreach (var pair in spawnedHexes)
            {
                if (pair.Key != keepCoord) Destroy(pair.Value);
            }
            spawnedHexes.Clear();
            availableSpots.Clear();
            spawnedHexes.Add(keepCoord, keptObject);
        }
        else
        {
            foreach (var obj in spawnedHexes.Values) Destroy(obj);
            spawnedHexes.Clear();
            availableSpots.Clear();
        }
    }

    void SpawnPlayer(Vector2Int targetCoord)
    {
        if (playerPrefab == null || spawnedHexes.Count == 0) return;

        Vector2Int finalSpawnCoord = GetClosestSpawnCoordinate(targetCoord);
        Vector3 hexPos = spawnedHexes[finalSpawnCoord].transform.position;
        Vector3 spawnPos = new Vector3(hexPos.x, hexPos.y + 0.5f, hexPos.z);
        Instantiate(playerPrefab, spawnPos, Quaternion.identity);
    }

    void ClearAllPlayer()
    {
        // Find the GameObject marked with the "Player" tag
        GameObject playerObj = GameObject.FindWithTag("Player");

        if (playerObj != null)
        {
            // Destroy the player object safely
            Destroy(playerObj);
            Debug.Log("Player object found and destroyed.");
        }
        else
        {
            Debug.LogWarning("Could not find a GameObject with the tag 'Player'!");
        }

        // for targets
        GameObject[] enemiesToDestroy = GameObject.FindGameObjectsWithTag("PlayerTargets");

        // 2. Loop through the array and destroy each one
        foreach (GameObject enemy in enemiesToDestroy)
        {
            Destroy(enemy);
        }

        Debug.Log($"Successfully cleared {enemiesToDestroy.Length} objects from the scene.");
    }

    private Vector2Int GetClosestSpawnCoordinate(Vector2Int targetCoord)
    {
        if (spawnedHexes.ContainsKey(targetCoord)) return targetCoord;

        Vector2Int closestCoord = targetCoord;
        float closestDistance = float.MaxValue;

        foreach (Vector2Int spawnedCoord in spawnedHexes.Keys)
        {
            float dist = Vector2Int.Distance(targetCoord, spawnedCoord);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                closestCoord = spawnedCoord;
            }
        }
        return closestCoord;
    }
}