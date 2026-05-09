using UnityEngine;
using System.Collections.Generic;

public class HexAreaHandler : MonoBehaviour
{
    [Header("Type")]
    [SerializeField] private ContractType currentContractType;
    
    public GameObject hexPrefab;
    public GameObject playerPrefab;

    public int totalHexes = 35;
    public float hexSize = 1f;

    private Vector2Int[] directions = {
        new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(-1, 1),
        new Vector2Int(-1, 0), new Vector2Int(0, -1), new Vector2Int(1, -1)
    };

    public Dictionary<Vector2Int, GameObject> spawnedHexes = new Dictionary<Vector2Int, GameObject>();
    private List<Vector2Int> availableSpots = new List<Vector2Int>();
    public List<Vector2Int> specialPoints = new List<Vector2Int>();
    public Dictionary<Vector2Int, string> specialPointRoles = new Dictionary<Vector2Int, string>();

    //start is a temporary thing until i get the larger loop working, so bug should get fixed
    void Start()
    {
        // GenerateNextArea(Vector2Int.zero);
        // SpawnPlayer(Vector2Int.zero);
    }

    public void CreateRound()
    {
        GenerateNextArea(Vector2Int.zero);
    }

    public void GenerateNextArea(Vector2Int startPoint)
    {
        // 1. Clear everything EXCEPT the current hex the player is standing on
        ClearOldGrid(startPoint);

        specialPoints.Clear();
        specialPointRoles.Clear();

        // 2. Run your existing clump logic starting from the new 'startPoint'
        UpdateAvailableSpots(startPoint);

        for (int i = 1; i < totalHexes; i++)
        {
            if (availableSpots.Count == 0) break;
            int randomIndex = Random.Range(0, availableSpots.Count);
            Vector2Int nextCoord = availableSpots[randomIndex];

            SpawnHex(nextCoord);
            availableSpots.RemoveAt(randomIndex);
            UpdateAvailableSpots(nextCoord);
        }
       
        specialPoints.Clear();

        specialPoints.Add(startPoint);

        // 1. Set the main goal (Grey, very far)
        PlaceSpecialPoint(startPoint, Color.grey, "DESTINATION", 5f);

        PlaceSpecialPoint(startPoint, Color.grey, "DESTINATION", 7f);

        // 2. Add a "Red Stop" (Must be at least 3 units away from the Destination)
        PlaceSpecialPoint(startPoint, Color.red, "RED_STOP", 0.3f);

        // 3. Add a "Blue Stop" (Must be at least 3 units away from everything else)
        PlaceSpecialPoint(startPoint, Color.blue, "BLUE_STOP", 0.3f);
    }

    void PlaceSpecialPoint(Vector2Int origin, Color pointColor, string label, float preferredDist, float minDistanceFromOthers = 3f)
    {
        Vector2Int bestCoord = origin;
        // We start with a huge "difference" so any valid hex will be smaller/better
        float lowestDiff = float.MaxValue; 
        bool foundValidSpot = false;

        foreach (var coord in spawnedHexes.Keys)
        {
            float distFromOrigin = Vector2Int.Distance(origin, coord);
            
            // 1. Check if it's too close to existing points
            bool tooClose = false;
            foreach (var existing in specialPoints)
            {
                if (Vector2Int.Distance(coord, existing) < minDistanceFromOthers)
                {
                    tooClose = true;
                    break;
                }
            }
            if (tooClose) continue;

            // 2. Scoring Logic: How close is this hex to our PREFERRED distance?
            // Math.Abs gives us the difference. Smallest difference wins.
            float diff = Mathf.Abs(distFromOrigin - preferredDist);

            if (diff < lowestDiff)
            {
                lowestDiff = diff;
                bestCoord = coord;
                foundValidSpot = true;
            }
        }

        if (foundValidSpot)
        {            
            specialPoints.Add(bestCoord); // Still keep the list for distance checks
            specialPointRoles.Add(bestCoord, label); // Save the name/role
            GameObject hex = spawnedHexes[bestCoord];
            hex.GetComponentInChildren<Renderer>().material.color = pointColor;
            hex.name = label;
        }
    }

    void UpdateAvailableSpots(Vector2Int center)
    {
        foreach (var dir in directions)
        {
            Vector2Int neighbor = center + dir;
            // Only add if it's not already spawned and not already in the available list
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

        hex.transform.GetChild(0).name = "Hex_" + coord.x + "_" + coord.y;

        spawnedHexes.Add(coord, hex);
    }

    void ClearOldGrid(Vector2Int keepCoord)
    {
        // 1. Grab the reference to the object we want to keep
        if (spawnedHexes.TryGetValue(keepCoord, out GameObject keptObject))
        {
            // 2. Loop through and destroy everything EXCEPT the keepCoord
            foreach (var pair in spawnedHexes)
            {
                if (pair.Key != keepCoord)
                {
                    Destroy(pair.Value);
                }
            }

            // 3. Reset the collections
            spawnedHexes.Clear();
            availableSpots.Clear();

            // 4. Re-add the one we saved
            spawnedHexes.Add(keepCoord, keptObject);
        }
        else
        {
            // Fallback: If keepCoord wasn't in the dict, just clear everything
            foreach (var obj in spawnedHexes.Values) Destroy(obj);
            spawnedHexes.Clear();
            availableSpots.Clear();
        }
    }

    void SpawnPlayer(Vector2Int coord)
    {
        if (playerPrefab != null && spawnedHexes.ContainsKey(coord))
        {
            Vector3 hexPos = spawnedHexes[coord].transform.position;
            spawnedHexes[coord].GetComponentInChildren<Renderer>().material.color = Color.grey;
            // Spawn the player slightly above the hex so they aren't clipping through the floor
            Vector3 spawnPos = new Vector3(hexPos.x, hexPos.y + 0.5f, hexPos.z);
            Instantiate(playerPrefab, spawnPos, Quaternion.identity);
        }
    }

    public void SetContractType(ContractType cT)
    {
        currentContractType = cT;
    }
}