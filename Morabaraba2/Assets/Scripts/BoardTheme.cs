using UnityEngine;

/// <summary>
/// Defines all visual properties for a Morabaraba theme.
/// Create via: Right-click in Project > Create > Morabaraba > Board Theme
/// </summary>
[CreateAssetMenu(fileName = "NewBoardTheme", menuName = "Morabaraba/Board Theme")]
public class BoardTheme : ScriptableObject
{
    [Header("Theme Info")]
    public string themeName = "New Theme";

    // ── Board ──────────────────────────────────────────────────────────────
    [Header("Board")]
    public Sprite boardSprite;                          // Background board image
    public Color boardTint = Color.white;         // Tint applied to board sprite
    public Color boardLineColor = Color.black;

    // ── Nodes (intersection points) ────────────────────────────────────────
    [Header("Board Nodes")]
    public Sprite nodeSprite;
    public Color nodeDefaultColor = new Color(0.2f, 0.2f, 0.2f);
    public Color nodeHoverColor = Color.yellow;
    public Color nodeValidColor = Color.green;
    public Color nodeOccupiedColor = Color.gray;

    // ── Player 1 Pieces ────────────────────────────────────────────────────
    [Header("Player 1 Pieces")]
    public Sprite player1Sprite;
    public Color player1Color = Color.white;
    public Color player1HighlightColor = new Color(1f, 0.9f, 0.3f);   // Drag / selected
    public Sprite player1HighlightSprite;                               // Optional swap on select

    // ── Player 2 Pieces ────────────────────────────────────────────────────
    [Header("Player 2 Pieces")]
    public Sprite player2Sprite;
    public Color player2Color = Color.black;
    public Color player2HighlightColor = new Color(0.3f, 0.9f, 1f);
    public Sprite player2HighlightSprite;

    // ── Mill / Removal ─────────────────────────────────────────────────────
    [Header("Mill & Removal")]
    public Color millFlashColor = Color.red;         // Removable cows flash this
    public Color protectedCowColor = new Color(1f, 0.4f, 0.4f, 0.5f);

    // ── Turn UI ────────────────────────────────────────────────────────────
    [Header("Turn UI Colors")]
    public Color player1TurnColor = Color.red;         // Replaces hardcoded Color.red
    public Color player2TurnColor = Color.blue;        // Replaces hardcoded Color.blue
    public Color millAlertColor = Color.yellow;      // "Formed a mill!" text color
    public Color goodStatusColor = Color.green;       // Network status: connected
    public Color badStatusColor = Color.red;         // Network status: error

    // ── Background / Environment ───────────────────────────────────────────
    [Header("Scene Background")]
    public Sprite backgroundSprite;
    public Color ambientColor = Color.white;

    // ── Audio ──────────────────────────────────────────────────────────────
    [Header("Audio")]
    public AudioClip backgroundMusic;
    public AudioClip placementSound;
    public AudioClip removalSound;
    public AudioClip invalidMoveSound;
    public AudioClip millFormedSound;
}