using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Main menu.
/// "Play Online" goes to LobbyScene.
/// "Play Local" skips networking and goes straight to GameScene.
///
/// INSPECTOR SETUP:
///   - "Play Online" button calls StartOnline()
///   - "Play Local" button calls StartLocal()
/// </summary>
public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField playerNameInput;

    public void StartOnline()
    {
        SceneManager.LoadScene("LobbyScene");
    }

    public void StartLocal()
    {
        string p1 = playerNameInput != null ? playerNameInput.text.Trim() : "";
        if (string.IsNullOrEmpty(p1)) p1 = "Player 1";

        PlayerPrefs.SetString("P1", p1);
        PlayerPrefs.SetString("P2", "Player 2");
        PlayerPrefs.SetInt("MyPlayerNumber", 0);

        SceneManager.LoadScene("GameScene");
    }
}
