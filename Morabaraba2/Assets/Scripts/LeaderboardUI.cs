using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LeaderboardUI : MonoBehaviour
{
    [Header("Leaderboard Panel")]
    public GameObject leaderboardPanel;

    [Header("Buttons")]
    public Button openLeaderboardButton;      // Button to open the leaderboard
    public Button closeLeaderboardButton;     // Button to close the leaderboard
    public Button refreshButton;              // Optional: manual refresh button

    [Header("Leaderboard Reference")]
    public Leaderboard leaderboardDisplay;    // Reference to your Leaderboard script

    [Header("Optional: Auto-refresh while open")]
    public bool autoRefresh = true;
    public float refreshInterval = 5f;        // Refresh every 5 seconds

    [Header("Loading Text")]
    public TextMeshProUGUI loadingText;       // Optional: shows "Loading..." while fetching

    private float refreshTimer = 0f;
    private bool isLeaderboardOpen = false;

    void Start()
    {
        // Initialize panel as hidden
        if (leaderboardPanel != null)
            leaderboardPanel.SetActive(false);

        // Setup buttons
        if (openLeaderboardButton != null)
            openLeaderboardButton.onClick.AddListener(OpenLeaderboard);

        if (closeLeaderboardButton != null)
            closeLeaderboardButton.onClick.AddListener(CloseLeaderboard);

        if (refreshButton != null)
            refreshButton.onClick.AddListener(RefreshLeaderboard);

        // Find leaderboard if not assigned
        if (leaderboardDisplay == null)
            leaderboardDisplay = FindObjectOfType<Leaderboard>();

        if (leaderboardDisplay == null)
            Debug.LogWarning("[LeaderboardUI] No Leaderboard component found in scene!");
    }

    void Update()
    {
        // Auto-refresh while leaderboard is open
        if (autoRefresh && isLeaderboardOpen)
        {
            refreshTimer += Time.deltaTime;
            if (refreshTimer >= refreshInterval)
            {
                refreshTimer = 0f;
                RefreshLeaderboard();
            }
        }
    }

    /// <summary>
    /// Opens the leaderboard panel and refreshes the data
    /// </summary>
    public void OpenLeaderboard()
    {
        if (leaderboardPanel != null)
        {
            leaderboardPanel.SetActive(true);
            isLeaderboardOpen = true;
            refreshTimer = 0f;
            RefreshLeaderboard();
            Debug.Log("[LeaderboardUI] Leaderboard opened");
        }
        else
        {
            Debug.LogError("[LeaderboardUI] Leaderboard Panel is not assigned!");
        }
    }

    /// <summary>
    /// Closes the leaderboard panel
    /// </summary>
    public void CloseLeaderboard()
    {
        if (leaderboardPanel != null)
        {
            leaderboardPanel.SetActive(false);
            isLeaderboardOpen = false;
            Debug.Log("[LeaderboardUI] Leaderboard closed");
        }
    }

    /// <summary>
    /// Toggles the leaderboard open/closed
    /// </summary>
    public void ToggleLeaderboard()
    {
        if (isLeaderboardOpen)
            CloseLeaderboard();
        else
            OpenLeaderboard();
    }

    /// <summary>
    /// Refreshes the leaderboard data
    /// </summary>
    public void RefreshLeaderboard()
    {
        if (leaderboardDisplay == null)
        {
            leaderboardDisplay = FindObjectOfType<Leaderboard>();
            if (leaderboardDisplay == null)
            {
                Debug.LogError("[LeaderboardUI] Cannot refresh - No Leaderboard component found!");
                return;
            }
        }

        // Show loading text if available
        if (loadingText != null)
        {
            loadingText.gameObject.SetActive(true);
            loadingText.text = "LOADING...";
        }

        // Fetch the latest scores
        StartCoroutine(RefreshLeaderboardCoroutine());
    }

    private System.Collections.IEnumerator RefreshLeaderboardCoroutine()
    {
        yield return StartCoroutine(leaderboardDisplay.FetchTopHighscoresRoutine());

        // Hide loading text
        if (loadingText != null)
        {
            loadingText.gameObject.SetActive(false);
        }

        Debug.Log("[LeaderboardUI] Leaderboard refreshed");
    }

    /// <summary>
    /// Called when the panel is destroyed - cleans up button listeners
    /// </summary>
    void OnDestroy()
    {
        if (openLeaderboardButton != null)
            openLeaderboardButton.onClick.RemoveListener(OpenLeaderboard);

        if (closeLeaderboardButton != null)
            closeLeaderboardButton.onClick.RemoveListener(CloseLeaderboard);

        if (refreshButton != null)
            refreshButton.onClick.RemoveListener(RefreshLeaderboard);
    }
}
