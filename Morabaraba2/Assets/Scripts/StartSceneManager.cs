using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// INSPECTOR SETUP:
///   humanVsHumanButton  → LobbyScene
///   humanVsAIButton     → AIScene
/// </summary>
public class StartSceneManager : MonoBehaviour
{
    public Button humanVsHumanButton;
    public Button humanVsAIButton;

    void Start()
    {
        if (humanVsHumanButton != null)
            humanVsHumanButton.onClick.AddListener(() =>
            {
                PlayerPrefs.SetInt("IsAIGame", 0);
                PlayerPrefs.Save();
                SceneManager.LoadScene("LobbyScene");
            });

        if (humanVsAIButton != null)
            humanVsAIButton.onClick.AddListener(() =>
            {
                PlayerPrefs.SetInt("IsAIGame", 1);
                PlayerPrefs.Save();
                SceneManager.LoadScene("AI Scene");
            });
    }
}