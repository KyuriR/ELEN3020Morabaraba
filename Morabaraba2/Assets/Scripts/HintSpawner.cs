using UnityEngine;

/// <summary>
/// Shows a themed hint panel every 5 seconds automatically.
/// No button needed — it just runs on a timer.
///
/// INSPECTOR SETUP:
///   1. Add to an empty GameObject called "HintSpawner" in GameScene.
///   2. hintPrefabs[0] — HintPanel_Traditional prefab
///   3. hintPrefabs[1] — HintPanel_Anime prefab
///   4. hintPrefabs[2] — HintPanel_StarWars prefab
///   5. hintParent     — drag your Canvas here so the panel sits in UI space
///   6. hintSpawnPoint — an empty GameObject inside the Canvas at the position
///                       you want the hint to appear (e.g. bottom of screen)
///   7. displayDuration — how many seconds the hint stays visible (default 3)
///   8. interval        — seconds between hints (default 5)
/// </summary>
public class HintSpawner : MonoBehaviour
{
    [Header("Hint Prefabs  (0=Traditional  1=Anime  2=StarWars)")]
    public GameObject[] hintPrefabs = new GameObject[3];

    [Header("UI Placement")]
    [Tooltip("Drag your Canvas here so hints appear in UI space")]
    public Transform hintParent;
    [Tooltip("Empty GameObject inside Canvas marking where the hint appears")]
    public Transform hintSpawnPoint;

    [Header("Timing")]
    [Tooltip("How long each hint stays on screen (seconds)")]
    public float displayDuration = 3f;
    [Tooltip("Seconds between each hint appearing")]
    public float interval = 5f;

    // ── Runtime ────────────────────────────────────────────────────────────
    private GameObject _activePanel;
    private float _timer = 0f;
    private bool _showing = false;
    private float _showTimer = 0f;

    // ── Unity ──────────────────────────────────────────────────────────────
    void Update()
    {
        if (GameManager.Instance == null) return;

        // Don't show hints during removal phase
        if (GameManager.Instance.IsRemovalPhase())
        {
            HidePanel();
            return;
        }

        if (_showing)
        {
            // Count down how long the hint has been visible
            _showTimer += Time.deltaTime;
            if (_showTimer >= displayDuration)
            {
                HidePanel();
                _timer = 0f; // reset interval after hiding
            }
        }
        else
        {
            // Count up to next hint
            _timer += Time.deltaTime;
            if (_timer >= interval)
                ShowPanel();
        }
    }

    // ── Show ───────────────────────────────────────────────────────────────
    private void ShowPanel()
    {
        HidePanel(); // destroy any leftover

        GameObject prefab = GetCurrentPrefab();
        if (prefab == null)
        {
            Debug.LogWarning("[HintSpawner] No prefab assigned for current theme.");
            return;
        }

        Vector3 pos = hintSpawnPoint != null ? hintSpawnPoint.position : Vector3.zero;
        Quaternion rot = hintSpawnPoint != null ? hintSpawnPoint.rotation : Quaternion.identity;

        _activePanel = hintParent != null
            ? Instantiate(prefab, pos, rot, hintParent)
            : Instantiate(prefab, pos, rot);

        HintPanel panel = _activePanel.GetComponent<HintPanel>();
        if (panel != null) panel.Show();

        _showing = true;
        _showTimer = 0f;
    }

    // ── Hide ───────────────────────────────────────────────────────────────
    private void HidePanel()
    {
        if (_activePanel != null)
        {
            Destroy(_activePanel);
            _activePanel = null;
        }
        _showing = false;
    }

    // ── Helpers ────────────────────────────────────────────────────────────
    private GameObject GetCurrentPrefab()
    {
        int idx = ThemeManager.Instance != null ? ThemeManager.Instance.CurrentThemeIndex : 0;
        if (idx < 0 || idx >= hintPrefabs.Length) idx = 0;
        return hintPrefabs[idx];
    }
}