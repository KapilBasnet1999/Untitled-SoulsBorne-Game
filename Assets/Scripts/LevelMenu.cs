using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;

public class LevelMenu : MonoBehaviour
{
    public Button[] buttons;
    private string[] sceneNames;

    private void Awake()
    {
        // Fetch scene names from Build Settings
        int sceneCount = SceneManager.sceneCountInBuildSettings;
        sceneNames = new string[sceneCount - 1]; // Skip Main Menu at index 0

        for (int i = 1; i < sceneCount; i++) // Start from index 1
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneName = Path.GetFileNameWithoutExtension(path);
            sceneNames[i - 1] = sceneName;
        }

        int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);

        for (int i = 0; i < buttons.Length && i < sceneNames.Length; i++)
        {
            int levelIndex = i + 1;
            buttons[i].interactable = levelIndex <= unlockedLevel;

            int capturedIndex = i;
            buttons[i].onClick.AddListener(() => OpenLevel(capturedIndex + 1));
        }
    }

    public void OpenLevel(int levelId)
    {
        int index = levelId - 1;

        if (index >= 0 && index < sceneNames.Length)
        {
            Debug.Log("Loading: " + sceneNames[index]);
            SceneManager.LoadScene(sceneNames[index]);
        }
        else
        {
            Debug.LogError("Invalid levelId: " + levelId);
        }
    }
}
