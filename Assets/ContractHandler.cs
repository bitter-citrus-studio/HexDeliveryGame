using UnityEngine;

public class ContractHandler : MonoBehaviour
{
    [SerializeField] private ContractType contractType;

    [Header("UI")]
    public GameObject typeTMP;
    public GameObject valueTMP;

    private HexAreaHandler hexAreaHandler;    

    public void SetContractType(ContractType cT)
    {
        contractType = cT;
    }

    private void Start()
    {
        GameUtility.UpdateText(typeTMP, contractType.difficulty.ToString());
        GameUtility.UpdateText(valueTMP, $"{contractType.rewardRange.x} - {contractType.rewardRange.y} \nTravel Tokens");
        hexAreaHandler = Object.FindFirstObjectByType<HexAreaHandler>();
    }

    public void SetType()
    {
        hexAreaHandler.SetContractType(contractType);
    }



}
