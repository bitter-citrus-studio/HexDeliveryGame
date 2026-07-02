using UnityEngine;
using UnityEngine.UI;

public class TilePlayableButton : MonoBehaviour
{
    [SerializeField] private Image buttonImage;
    
    private TileData assignedTile;
    private PlayerInventory inventory;    
    
    public void SetupButton(TileData tile, PlayerInventory playerInventory)
    {
        assignedTile = tile;
        inventory = playerInventory;

        if (buttonImage != null)
        {
            buttonImage.color = tile.tileColor;
        }
    }

    public void OnButtonClick()
    {
        inventory.AddTile(assignedTile);
        Destroy(gameObject); 
    }
}