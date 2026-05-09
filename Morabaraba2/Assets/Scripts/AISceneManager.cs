using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// INSPECTOR SETUP:
///   playerNameInput      — TMP InputField
///   easyButton           — Easy difficulty
///   mediumButton         — Medium difficulty
///   hardButton           — Hard difficulty
///   traditionalButton    — Traditional theme
///   animeButton          — Anime theme
///   starWarsButton       — Star Wars theme
///   playButton           — starts GameScene
///   backButton           — returns to StartScene
///   selectedLabel        — (optional) shows current picks
/// </summary>
public class AISceneManager : MonoBehaviour
{
    [Header("Player Name")]
    public TMP_InputField playerNameInput;

    [Header("Difficulty Buttons")]
    public Button easyButton;
    public Button mediumButton;
    public Button hardButton;

    [Header("Theme Buttons")]
    public Button traditionalButton;
    public Button animeButton;
    public Button starWarsButton;

    [Header("Navigation")]
    public Button playButton;
    public Button backButton;

    [Header("Optional label showing current picks")]
    public TextMeshProUGUI selectedLabel;

    [Header("Highlight colours")]
    public Color selectedColor = new Color(0.95f, 0.80f, 0.20f);
    public Color deselectedColor = Color.white;

    private int _difficulty = 0;   // 0=Easy 1=Medium 2=Hard
    private int _theme = 0;   // 0=Traditional 1=Anime 2=StarWars

    private static readonly string[] DifficultyNames = { "Easy", "Medium", "Hard" };
    private static readonly string[] ThemeNames = { "Traditional", "Anime", "Star Wars" };

    void Start()
    {
        string saved = PlayerPrefs.GetString("LoggedInUsername", "");
        if (!string.IsNullOrEmpty(saved) && playerNameInput != null)
            playerNameInput.text = saved;

        _difficulty = PlayerPrefs.GetInt("AIDifficulty", 0);
        _theme = PlayerPrefs.GetInt("SelectedTheme", 0);

        if (easyButton != null) easyButton.onClick.AddListener(() => SetDifficulty(0));
        if (mediumButton != null) mediumButton.onClick.AddListener(() => SetDifficulty(1));
        if (hardButton != null) hardButton.onClick.AddListener(() => SetDifficulty(2));
        if (traditionalButton != null) traditionalButton.onClick.AddListener(() => SetTheme(0));
        if (animeButton != null) animeButton.onClick.AddListener(() => SetTheme(1));
        if (starWarsButton != null) starWarsButton.onClick.AddListener(() => SetTheme(2));
        if (playButton != null) playButton.onClick.AddListener(Play);
        if (backButton != null) backButton.onClick.AddListener(GoBack);

        Refresh();
    }

    void SetDifficulty(int index) { _difficulty = index; Refresh(); }
    void SetTheme(int index) { _theme = index; Refresh(); }

    void Play()
    {
        string name = playerNameInput != null ? playerNameInput.text.Trim() : "";
        if (string.IsNullOrEmpty(name)) name = "Player 1";

        PlayerPrefs.SetString("P1", name);
        PlayerPrefs.SetString("P2", "AI");
        PlayerPrefs.SetInt("MyPlayerNumber", 1);
        PlayerPrefs.SetInt("IsAIGame", 1);
        PlayerPrefs.SetInt("AIDifficulty", _difficulty);
        PlayerPrefs.SetInt("SelectedTheme", _theme);
        PlayerPrefs.Save();

        SceneManager.LoadScene("GameScene");
    }

    void GoBack()
    {
        SceneManager.LoadScene("StartScene");
    }

    void Refresh()
    {
        Highlight(easyButton, _difficulty == 0);
        Highlight(mediumButton, _difficulty == 1);
        Highlight(hardButton, _difficulty == 2);

        Highlight(traditionalButton, _theme == 0);
        Highlight(animeButton, _theme == 1);
        Highlight(starWarsButton, _theme == 2);

        if (selectedLabel != null)
            selectedLabel.text = $"Difficulty: {DifficultyNames[_difficulty]}   Theme: {ThemeNames[_theme]}";
    }

    void Highlight(Button btn, bool on)
    {
        if (btn == null) return;
        Image img = btn.GetComponent<Image>();
        if (img != null) img.color = on ? selectedColor : deselectedColor;
    }
}