using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public static class GameUtility
{
    public static void UpdateText(GameObject uiObj, string newText)
    {
        var tmp = uiObj.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp != null)
        {
            tmp.text = newText;
        }
        else
        {
            Debug.LogWarning($"No TextMeshPro found on {uiObj.name}");
        }
    }
}
