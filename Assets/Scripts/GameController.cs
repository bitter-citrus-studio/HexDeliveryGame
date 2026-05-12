using UnityEngine;

public class GameController : MonoBehaviour
{
    //make a scriptable object for this data....
    public int availableTravel = 35;

    public Transform ui_ContractMainPanel;
    public GameObject ui_Contract;

    public ContractType[] possibleContractTypes;

    [Header("UI")]
    public GameObject travelTMP;

    void Start()
    {
        StartNewRound();
    }

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

    public void StartNewRound()
    {
        for (int i = 0; i < 3; i++)
        {
            GenerateContractUI();
        }
    }

    public void GenerateContractUI()
    {
        GameObject contractGO = Instantiate(ui_Contract, ui_ContractMainPanel);
        int typeIndex = GameUtility.GetRandomValue(0, possibleContractTypes.Length);
        contractGO.GetComponent<ContractHandler>().SetContractType(possibleContractTypes[typeIndex]);

    }
}
