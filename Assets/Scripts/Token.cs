using UnityEngine;

[CreateAssetMenu(fileName = "NewToken", menuName = "ContractSystem/Token")]
public class Token : ScriptableObject
{
    public string tokenName;
    public Color tokenColour = Color.white;
    public GameObject tokenPrefab;
}