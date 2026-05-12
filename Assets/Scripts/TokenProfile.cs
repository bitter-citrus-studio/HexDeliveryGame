using UnityEngine;

[CreateAssetMenu(fileName = "NewTokenProfile", menuName = "ContractSystem/TokenProfile")]
public class TokenProfile : ScriptableObject
{
    public string tokenProfileName = "New Token Profile";
    public Token[] tokens;
}