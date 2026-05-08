using System.Collections;
using UnityEngine;

public class ThemeManager : MonoBehaviour
{
    public static ThemeManager Instance { get; private set; }

    [Header("Board Prefabs  (0=Traditional  1=Anime  2=StarWars)")]
    public GameObject[] boardPrefabs = new GameObject[3];

    [Header("Player 1 Token Prefabs  (same order)")]
    public GameObject[] player1TokenPrefabs = new GameObject[3];

    [Header("Player 2 Token Prefabs  (same order)")]
    public GameObject[] player2TokenPrefabs = new GameObject[3];

    [Header("Scene References")]
    public Transform boardSpawnPoint;
    public CowSpawner cowSpawner1;
    public CowSpawner cowSpawner2;

    [Header("HintManager")] 
    public HintAppearing hintManager;

    [Header("Default Theme if nothing saved (0=Traditional 1=Anime 2=StarWars)")]
    public int defaultThemeIndex = 0;

    [Header("Background Music Per Theme (optional)")]
    public AudioClip[] themeMusic = new AudioClip[3];
    public AudioSource musicSource;

    public int CurrentThemeIndex { get; private set; } = -1;

    private GameObject _activeBoardInstance;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // Read the theme the player picked in the Lobby settings panel.
        int saved = PlayerPrefs.GetInt("SelectedTheme", defaultThemeIndex);
        ApplyTheme(saved, instant: true);
    }

    // ── Public API ─────────────────────────────────────────────────────────

    public void ApplyTheme(int index, bool instant = false)
    {
        if (index < 0 || index > 2) return;
        if (index == CurrentThemeIndex) return;
        CurrentThemeIndex = index;
        if (instant) SwapAll(index);
        else StartCoroutine(SwapWithFade(index));
    }

    public void ApplyTraditional() => ApplyTheme(0);
    public void ApplyAnime() => ApplyTheme(1);
    public void ApplyStarWars() => ApplyTheme(2);

    // ── Colour helpers ─────────────────────────────────────────────────────

    public Color PieceHighlight(int player)
    {
        switch (CurrentThemeIndex)
        {
            case 0: return player == 1 ? new Color(0.98f, 0.82f, 0.30f) : new Color(0.88f, 0.55f, 0.10f);
            case 1: return player == 1 ? new Color(1.00f, 0.95f, 0.55f) : new Color(0.75f, 0.50f, 1.00f);
            case 2: return player == 1 ? new Color(1.00f, 0.95f, 0.60f) : new Color(0.85f, 0.30f, 0.20f);
            default: return Color.yellow;
        }
    }

    public Color TurnColor(int player)
    {
        switch (CurrentThemeIndex)
        {
            case 0: return player == 1 ? new Color(0.93f, 0.67f, 0.25f) : new Color(0.55f, 0.32f, 0.12f);
            case 1: return player == 1 ? new Color(0.35f, 0.70f, 1.00f) : new Color(1.00f, 0.45f, 0.65f);
            case 2: return player == 1 ? new Color(0.92f, 0.80f, 0.30f) : new Color(0.75f, 0.78f, 0.88f);
            default: return player == 1 ? Color.red : Color.blue;
        }
    }

    public Color MillFlashColor()
    {
        switch (CurrentThemeIndex)
        {
            case 0: return new Color(0.90f, 0.25f, 0.10f);
            case 1: return new Color(1.00f, 0.30f, 0.55f);
            case 2: return new Color(0.90f, 0.15f, 0.10f);
            default: return Color.red;
        }
    }

    public Color MillAlertColor()
    {
        switch (CurrentThemeIndex)
        {
            case 0: return new Color(0.98f, 0.82f, 0.30f);
            case 1: return new Color(1.00f, 0.85f, 0.25f);
            case 2: return new Color(0.90f, 0.15f, 0.10f);
            default: return Color.yellow;
        }
    }

    public Color StatusColor(bool good)
    {
        switch (CurrentThemeIndex)
        {
            case 0: return good ? new Color(0.45f, 0.72f, 0.18f) : new Color(0.80f, 0.20f, 0.10f);
            case 1: return good ? new Color(0.35f, 0.90f, 0.60f) : new Color(1.00f, 0.30f, 0.55f);
            case 2: return good ? new Color(0.15f, 0.85f, 0.45f) : new Color(0.90f, 0.15f, 0.10f);
            default: return good ? Color.green : Color.red;
        }
    }

    public Color NodeDefaultColor()
    {
        switch (CurrentThemeIndex)
        {
            case 0: return new Color(0.18f, 0.10f, 0.04f);
            case 1: return new Color(0.75f, 0.60f, 0.90f);
            case 2: return new Color(0.30f, 0.35f, 0.45f);
            default: return Color.gray;
        }
    }

    public Color NodeHoverColor()
    {
        switch (CurrentThemeIndex)
        {
            case 0: return new Color(0.88f, 0.68f, 0.22f);
            case 1: return new Color(1.00f, 0.75f, 0.90f);
            case 2: return new Color(0.25f, 0.70f, 1.00f);
            default: return Color.yellow;
        }
    }

    public Color NodeValidColor()
    {
        switch (CurrentThemeIndex)
        {
            case 0: return new Color(0.45f, 0.72f, 0.18f);
            case 1: return new Color(0.55f, 0.95f, 0.75f);
            case 2: return new Color(0.15f, 0.85f, 0.45f);
            default: return Color.green;
        }
    }

    // ── Internal ───────────────────────────────────────────────────────────

    private void SwapAll(int index)
    {
        SwapBoard(index);
        SwapTokens(index);
        SwapMusic(index);
        RefreshNodes();
    }

    private IEnumerator SwapWithFade(int index)
    {
        if (_activeBoardInstance != null)
            yield return StartCoroutine(FadeObject(_activeBoardInstance, 0f, 0.2f));
        SwapBoard(index);
        SwapTokens(index);
        SwapMusic(index);
        RefreshNodes();
        if (_activeBoardInstance != null)
            yield return StartCoroutine(FadeObject(_activeBoardInstance, 1f, 0.2f));
    }

    private void SwapBoard(int index)
    {
        if (_activeBoardInstance != null) Destroy(_activeBoardInstance);
        if (boardPrefabs[index] == null) { Debug.LogWarning($"[ThemeManager] boardPrefabs[{index}] not assigned."); return; }
        Vector3 pos = boardSpawnPoint != null ? boardSpawnPoint.position : Vector3.zero;
        Quaternion rot = boardSpawnPoint != null ? boardSpawnPoint.rotation : Quaternion.identity;
        _activeBoardInstance = Instantiate(boardPrefabs[index], pos, rot);
    }

    private void SwapTokens(int index)
    {
        if (cowSpawner1 != null) { if (player1TokenPrefabs[index] != null) cowSpawner1.cowPrefab = player1TokenPrefabs[index]; cowSpawner1.Respawn(); }
        if (cowSpawner2 != null) { if (player2TokenPrefabs[index] != null) cowSpawner2.cowPrefab = player2TokenPrefabs[index]; cowSpawner2.Respawn(); }
    }

    private void SwapMusic(int index)
    {
        if (musicSource == null) return;
        if (index >= themeMusic.Length || themeMusic[index] == null) return;
        if (musicSource.clip == themeMusic[index]) return;
        musicSource.Stop();
        musicSource.clip = themeMusic[index];
        musicSource.loop = true;
        musicSource.Play();
    }

    private void RefreshNodes()
    {
        foreach (BoardNodeThemer node in FindObjectsByType<BoardNodeThemer>(FindObjectsSortMode.None))
            node.RefreshTheme();
    }

    private IEnumerator FadeObject(GameObject go, float targetAlpha, float duration)
    {
        SpriteRenderer[] renderers = go.GetComponentsInChildren<SpriteRenderer>();
        float[] startAlphas = new float[renderers.Length];
        for (int i = 0; i < renderers.Length; i++) startAlphas[i] = renderers[i].color.a;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            for (int i = 0; i < renderers.Length; i++) { Color c = renderers[i].color; c.a = Mathf.Lerp(startAlphas[i], targetAlpha, t); renderers[i].color = c; }
            yield return null;
        }
    }
}