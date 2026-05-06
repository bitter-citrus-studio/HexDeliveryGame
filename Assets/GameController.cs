using UnityEngine;

public class GameController : MonoBehaviour
{
    //make a scriptable object for this data....
    int availableTravel = 35;

    [Header("UI")]
    public GameObject travelTMP;

    public void ChangeTravel(int input)
    {
        availableTravel += input;
        string text = "Travel tokens left: " + availableTravel.ToString();
        GameUtility.UpdateText(travelTMP, text);
    }

    public int GetTravel()
    {
        return availableTravel;
    }

    public void GameOver()
    {
        Debug.Log("Game Over!!!!!!!!!");
    }
}
