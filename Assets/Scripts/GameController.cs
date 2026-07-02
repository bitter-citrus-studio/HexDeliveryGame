using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{    
    private bool isPaused = false;
    public int scoreToBeat = 0;
    public int currentScore = 0; 
    public int currentLevel = 1;

    [Header("UI Fields")]
    public GameObject travelTMP;
    public GameObject scoreTMP;
    public GameObject scoreToBeatTMP; 
    
    public GameObject multiplierTMP;    
    public GameObject baseRewardTMP;    
    public GameObject expectedTotalTMP; 

    [Header("MidGamePanel")]
    
    public GameObject midGamePanel;
    public GameObject rewardTMP; 
    public GameObject nextLevelScoreTMP;

    [Header("Oher panels")] 
    public GameObject gameEndPanel;
    public GameObject gameEndScoreTMP;

    public GameObject pausePanel;
    public GameObject gameOverPanel;
    

    [Header("Dependencies")]
    public HexAreaHandler hexHandler;
    public PlayerInventory playerInventory; 
    public ProceduralLevelProfile levelProfile;

    public List<ProceduralLevelProfile> levelProfiles = new List<ProceduralLevelProfile>();

    public int currentHighestSteak = 1;

    public int levelReward;

    void Start()
    {
        playerInventory.ClearInventory();
        StartNewRound();
        ClearDeliveryDisplay();
        AddScore(0, 1, 0);

    }

    void Update()
    {
        if(isPaused) return;
        if (Input.GetButtonDown("Cancel") && !isPaused)
        {
            PauseToggle(true);
        }
    }

    /// <summary>
    /// Adds score and instantly flashes the reward formula on the UI.
    /// </summary>
    public void AddScore(int finalPoints, int multiplier, int baseReward)
    {
        currentScore += finalPoints;

        if(currentScore >= levelProfile.levelMaxScore)
        {
            currentLevel++;
            isPaused = true;
            midGamePanel.SetActive(true);
            if (rewardTMP != null) GameUtility.UpdateText(rewardTMP, $"+{levelReward} Travel Tokens");
            ChangeTravel(levelReward);
            SetLevelProfile(currentLevel);
            if (nextLevelScoreTMP != null) GameUtility.UpdateText(nextLevelScoreTMP, $"Next Score to reach....  {levelProfile.travelTokenReward}");
        }
        
        if (scoreTMP != null)
        {
            GameUtility.UpdateText(scoreTMP, "Score: " + currentScore.ToString());
        }

        // Show the math breakdown elements immediately
        if (multiplierTMP != null) GameUtility.UpdateText(multiplierTMP, $"Multiplier: x{multiplier:F1}");
        if (baseRewardTMP != null) GameUtility.UpdateText(baseRewardTMP, $"Base Value: {baseReward}");
        if (expectedTotalTMP != null) GameUtility.UpdateText(expectedTotalTMP, $"+{finalPoints} Points!");

        // Cancel any pending clears and schedule a fresh one in 2 seconds
        CancelInvoke(nameof(ClearDeliveryDisplay));
        Invoke(nameof(ClearDeliveryDisplay), 2.0f);
    }

    private void ClearDeliveryDisplay()
    {
        //if (multiplierTMP != null) GameUtility.UpdateText(multiplierTMP, "");
        if (baseRewardTMP != null) GameUtility.UpdateText(baseRewardTMP, "");
        if (expectedTotalTMP != null) GameUtility.UpdateText(expectedTotalTMP, "");
    }

    public void ChangeTravel(int input)
    {
        playerInventory.SetTravelTokens(input);
        string text = "Travel tokens left: " + playerInventory.GetTravelTokens().ToString();
        GameUtility.UpdateText(travelTMP, text);
    }

    public int GetTravel()
    {
        return playerInventory.GetTravelTokens();
    }

    public void RestartScene()
    {
        // Get the currently active scene
        Scene currentScene = SceneManager.GetActiveScene();
        
        // Reload it using its name (or currentScene.buildIndex)
        SceneManager.LoadScene(currentScene.name);
    }

    public void GameOver()
    {
        Debug.Log("Game Over!!!!!!!!!");
        SetPaused(true);
        //if (multiplierTMP != null) GameUtility.UpdateText(gameEndScoreTMP, $"Score: {currentScore.ToString()}");
        gameOverPanel.SetActive(true);

    }    

    public void StartNewRound()
    {
        isPaused = false;
        midGamePanel.SetActive(false);
        SetLevelProfile(currentLevel);
        scoreToBeat = levelProfile.levelMaxScore;
        levelReward = levelProfile.travelTokenReward;
        if (scoreToBeatTMP != null) GameUtility.UpdateText(scoreToBeatTMP, $"Score To Beat: {scoreToBeat}");
        hexHandler.SpawnNewLevel(GetLevelProfile());
    }

    public PlayerInventory GetPlayerInventory()
    {
        return playerInventory;
    }

    public ProceduralLevelProfile GetLevelProfile()
    {
        return levelProfile;
    }

    public void SetLevelProfile(int level)
    {
        // Example: Find the profile that matches the target level ID
        ProceduralLevelProfile activeProfile = levelProfiles.Find(profile => profile.levelID == level);

        if (activeProfile != null)
        {
            // Use your matched level profile data here!
            Debug.Log($"Loaded profile: {activeProfile.name}");
            levelProfile = activeProfile;
        }

        if(activeProfile == null)
        {
            //Game End
            GameEnd();
        }

        
    }

    void GameEnd()
    {
        SetPaused(true);
        if (multiplierTMP != null) GameUtility.UpdateText(gameEndScoreTMP, $"Score: {currentScore.ToString()}");
        gameEndPanel.SetActive(true);
    }

    public void PauseToggle(bool status)
    {
        SetPaused(status);
        pausePanel.SetActive(status);
    }

    public bool GetPaused()
    {
        return isPaused;
    }

    public void SetPaused(bool status)
    {
        isPaused = status;
    }
}