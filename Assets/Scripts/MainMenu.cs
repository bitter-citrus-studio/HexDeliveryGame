using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void LoadGameScene()
    {
        // Replace "Game" with the exact name of your scene file
        SceneManager.LoadScene("Game");
    }
}
