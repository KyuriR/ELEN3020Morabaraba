using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Attach this to any GameObject in your LobbyScene (e.g. the Settings button itself).
///
/// INSPECTOR SETUP:
///   settingsButton      — your existing Settings button
///   settingsPanel       — a Panel you create that starts hidden
///   closeButton         — an X / Close button inside the panel
///   traditionalButton   — theme pick button inside the panel
///   animeButton         — theme pick button inside the panel
///   starWarsButton      — theme pick button inside the panel
///   selectedThemeLabel  — (optional) TMP text that shows which theme is active
///
/// The chosen theme index is saved to PlayerPrefs key "SelectedTheme".
/// ThemeManager in GameScene reads that key on Start() and applies the right theme.
/// </summary>
public class SettingsMenu : MonoBehaviour
{
    [Header("Settings Button (in Lobby)")]
    public Button settingsButton;
    public Button leaderboard;

    [Header("Settings Panel")]
    public GameObject settingsPanel;
    public Button closeButton;

    [Header("Theme Buttons (inside the panel)")]
    public Button traditionalButton;
    public Button animeButton;
    public Button starWarsButton;

    [Header("Optional label showing active theme")]
    public TextMeshProUGUI selectedThemeLabel;

    [Header("Button tint when selected / deselected")]
    public Color selectedColor = new Color(0.95f, 0.80f, 0.20f); // gold
    public Color deselectedColor = Color.white;

    private static readonly string[] ThemeNames = { "Traditional", "Anime", "Star Wars" };

    void Start()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (settingsButton != null) settingsButton.onClick.AddListener(OpenPanel);
        if (closeButton != null) closeButton.onClick.AddListener(ClosePanel);
        if (traditionalButton != null) traditionalButton.onClick.AddListener(() => SelectTheme(0));
        if (animeButton != null) animeButton.onClick.AddListener(() => SelectTheme(1));
        if (starWarsButton != null) starWarsButton.onClick.AddListener(() => SelectTheme(2));

        RefreshHighlights();
    }

    // ── Open / Close ───────────────────────────────────────────────────────

    void OpenPanel()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(true);

        RefreshHighlights();
    }

    void ClosePanel()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    // ── Theme Selection ────────────────────────────────────────────────────

    void SelectTheme(int index)
    {
        PlayerPrefs.SetInt("SelectedTheme", index);
        PlayerPrefs.Save();

        RefreshHighlights();

        Debug.Log($"[SettingsMenu] Theme set to {ThemeNames[index]}");
    }

    // ── Highlight whichever button matches the saved theme ─────────────────

    void RefreshHighlights()
    {
        int saved = PlayerPrefs.GetInt("SelectedTheme", 0);

        SetButtonHighlight(traditionalButton, saved == 0);
        SetButtonHighlight(animeButton, saved == 1);
        SetButtonHighlight(starWarsButton, saved == 2);

        if (selectedThemeLabel != null)
            selectedThemeLabel.text = "Theme: " + ThemeNames[saved];
    }

    void SetButtonHighlight(Button btn, bool isSelected)
    {
        if (btn == null) return;
        Image img = btn.GetComponent<Image>();
        if (img != null)
            img.color = isSelected ? selectedColor : deselectedColor;
    }
}