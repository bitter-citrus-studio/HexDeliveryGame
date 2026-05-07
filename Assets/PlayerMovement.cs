using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    public GameObject highlightPrefab;
    private HexAreaHandler gridRef;
    private GameController gc;
    private CameraMovement cm;

    [Header("Movement")]
    public float moveDuration = 0.2f;
    private Vector2Int currentGridPos = Vector2Int.zero;
    [SerializeField] private bool isMoving = false;

    private List<GameObject> activeHighlights = new List<GameObject>();

    private int spacesMoved = 0;

    void Start()
    {
        // Find the GridManager in the scene
        gridRef = Object.FindFirstObjectByType<HexAreaHandler>();
        gc = Object.FindFirstObjectByType<GameController>();
        cm = Object.FindFirstObjectByType<CameraMovement>();

        cm.player = transform;
        
        // The Player script now sets its internal grid position to 0,0 upon spawning
        currentGridPos = Vector2Int.zero;

        // Immediately show move targets for the first time
        UpdateMoveTargets();
        gc.ChangeTravel(0);
    }

    void Update()
    {
        if (isMoving) return;

        if (Input.GetMouseButtonDown(0))
        {
            HandleClick();
        }
    }

    void HandleClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.CompareTag("Hex"))
            {
                Vector2Int clickedCoord = GetCoordFromHex(hit.collider.gameObject);

                if(gc.GetTravel() > 0)

                if (IsNeighbor(currentGridPos, clickedCoord))
                {                    
                    gc.ChangeTravel(-1);
                    Debug.Log("Spaces moved:" + spacesMoved);
                    StartCoroutine(SmoothMove(clickedCoord, hit.collider.transform.position));
                }

                if(gc.GetTravel() <= 0)
                {
                    gc.GameOver();
                }                  
            }
        }
    }

    void UpdateMoveTargets()
    {
        // Clear existing highlights
        foreach (GameObject h in activeHighlights) Destroy(h);
        activeHighlights.Clear();

        Vector2Int[] neighbors = {
            new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(-1, 1),
            new Vector2Int(-1, 0), new Vector2Int(0, -1), new Vector2Int(1, -1)
        };

        foreach (var dir in neighbors)
        {
            Vector2Int neighborCoord = currentGridPos + dir;

            if (gridRef.spawnedHexes.ContainsKey(neighborCoord))
            {
                Vector3 hexPos = gridRef.spawnedHexes[neighborCoord].transform.position;
                // Place highlight slightly above the hex surface to avoid Z-fighting
                GameObject h = Instantiate(highlightPrefab, hexPos + Vector3.up * 0.55f, Quaternion.identity);
                activeHighlights.Add(h);
            }
        }
    }

    IEnumerator SmoothMove(Vector2Int targetCoord, Vector3 targetWorldPos)
    {
        isMoving = true;
        Vector3 startPos = transform.position;
        Vector3 endPos = new Vector3(targetWorldPos.x, transform.position.y, targetWorldPos.z);
        
        float elapsed = 0;
        while (elapsed < moveDuration)
        {
            transform.position = Vector3.Lerp(startPos, endPos, elapsed / moveDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = endPos;
        currentGridPos = targetCoord;
        isMoving = false; 

        if (gridRef.specialPointRoles.TryGetValue(targetCoord, out string role))
        {
            if (role == "DESTINATION")
            {
                // Change the name so it can't be triggered again immediately
                gridRef.specialPointRoles[targetCoord] = "USED_DESTINATION";

                if (gridRef.spawnedHexes.TryGetValue(targetCoord, out GameObject hexObj))
                {
                    hexObj.name = "USED_DESTINATION";
                    // Optional: Change color back to white or a "neutral" color
                    // hexObj.GetComponentInChildren<Renderer>().material.color = Color.white;
                }


                Debug.Log("Arrival! Spawning next area...");
                gridRef.GenerateNextArea(targetCoord);
            }
            else 
            {
                Debug.Log($"Visited a special stop: {role}");
                // You can use the 'role' string to decide what happens (RED_STOP, BLUE_STOP, etc.)
            }
        }
        UpdateMoveTargets();
    }

    bool IsNeighbor(Vector2Int a, Vector2Int b)
    {
        Vector2Int d = b - a;
        // Check if the difference matches any of the 6 hex directions
        return (d == new Vector2Int(1,0) || d == new Vector2Int(0,1) || d == new Vector2Int(-1,1) ||
                d == new Vector2Int(-1,0) || d == new Vector2Int(0,-1) || d == new Vector2Int(1,-1));
    }

    Vector2Int GetCoordFromHex(GameObject hex)
    {
        // hex.name looks like "Hex_-1_2(Clone)"
        string name = hex.name;

        // Remove "(Clone)" if it exists so it doesn't mess up the numbers
        if (name.Contains("(Clone)"))
        {
            name = name.Replace("(Clone)", "").Trim();
        }

        string[] parts = name.Split('_');

        // parts[0] is "Hex", parts[1] is "Q", parts[2] is "R"
        if (parts.Length >= 3)
        {
            int q = int.Parse(parts[1]);
            int r = int.Parse(parts[2]);
            return new Vector2Int(q, r);
        }

        Debug.LogError($"Could not parse hex name: {hex.name}. Make sure it is formatted as Hex_Q_R");
        return Vector2Int.zero; 
    }
}
