using UnityEngine;
using System.Collections.Generic;

public class HexAreaHandler : MonoBehaviour
{
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
    public Vector2Int destinationCoord;

    void Start()
    {
        GenerateNextArea(Vector2Int.zero);
        SpawnPlayer(Vector2Int.zero);
    }

    public void GenerateNextArea(Vector2Int startPoint)
    {
        // 1. Clear everything EXCEPT the current hex the player is standing on
        ClearOldGrid(startPoint);

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

        // 3. NEW: After clumping, find the best destination
        SetDestination(startPoint);
    }

    void SetDestination(Vector2Int origin)
    {
        // Find the hex furthest away from where we started
        float maxDist = -1;
        foreach (var coord in spawnedHexes.Keys)
        {
            float dist = Vector2Int.Distance(origin, coord);
            if (dist > maxDist)
            {
                maxDist = dist;
                destinationCoord = coord;
            }
        }

        // Visual feedback: Make the destination hex stand out
        spawnedHexes[destinationCoord].GetComponentInChildren<Renderer>().material.color = Color.grey;
        spawnedHexes[destinationCoord].name = "DESTINATION";
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
}