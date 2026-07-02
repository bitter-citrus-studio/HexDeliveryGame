using System.Collections.Generic;
using UnityEngine;

public class PlayerStackVisuals : MonoBehaviour
{
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private GameObject tileVisualPrefab; // Simple 3D mesh block
    [SerializeField] private Transform stackHoldPoint;    // Transform empty position anchor on the player
    [SerializeField] private float tileHeightOffset = 0.2f; 

    private List<GameObject> spawnedVisuals = new List<GameObject>();

    private void OnEnable()
    {
        playerInventory.OnInventoryChanged.AddListener(UpdateVisualStack);
        UpdateVisualStack(); // Initial sync
    }

    private void OnDisable()
    {
        playerInventory.OnInventoryChanged.RemoveListener(UpdateVisualStack);
    }

    private void UpdateVisualStack()
    {
        foreach (GameObject visual in spawnedVisuals)
        {
            Destroy(visual);
        }
        spawnedVisuals.Clear();

        for (int i = 0; i < playerInventory.stackedTiles.Count; i++)
        {
            TileData tile = playerInventory.stackedTiles[i];
            Vector3 spawnPosition = stackHoldPoint.position + (Vector3.up * (i * tileHeightOffset));

            GameObject newTileVisual = Instantiate(tileVisualPrefab, spawnPosition, stackHoldPoint.rotation, stackHoldPoint);

            float sizeRandom = Random.Range(-0.1f , 0.1f);
            newTileVisual.transform.localScale = new Vector3(1 + sizeRandom, 1, 1 + sizeRandom);
            
            // Render color
            var renderer = newTileVisual.GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = tile.tileColor;
            }

            spawnedVisuals.Add(newTileVisual);
        }
    }
}