using UnityEngine;
using TMPro;

/// <summary>
/// Attach to each themed hint panel prefab.
///
/// PREFAB STRUCTURE:
///   HintPanel_Traditional
///   ├── Image               ← your themed background image
///   ├── TextMeshProUGUI     ← hint message text
///   └── HintPanel.cs        ← this script, hintText wired to the TMP component
///
/// Repeat for HintPanel_Anime and HintPanel_StarWars.
/// </summary>
public class HintPanel : MonoBehaviour
{
    [Header("Wire to the TMP text inside the prefab")]
    public TextMeshProUGUI hintText;

    private static readonly string[] Messages =
    {
        "Hint: Corners often provide more movement options.",
        "Hint: Protect cows that connect multiple lines.",
        "Hint: If all opponent cows are in mills, any cow can be removed.",
        "Hint: Try to build two possible mills at once.",
        "Hint: Keep your cows spread out to avoid getting trapped",
        "Hint: Blocking an opponent's mill can be more important than making your own",

    };

    private static int _nextIndex = 0;

    void Awake()
    {
        if (hintText == null)
            hintText = GetComponentInChildren<TextMeshProUGUI>();
    }

    /// Called by HintSpawner right after instantiating this panel.
    public void Show()
    {
        if (hintText == null) return;
        hintText.text = Messages[_nextIndex];
        _nextIndex = (_nextIndex + 1) % Messages.Length;
    }
}