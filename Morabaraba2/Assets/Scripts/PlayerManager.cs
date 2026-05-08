using System.Collections;
using UnityEngine;
using LootLocker.Requests;

public class PlayerManager : MonoBehaviour
{
    public Leaderboard leaderboard;

    private static PlayerManager instance;
    public static PlayerManager Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<PlayerManager>();
            return instance;
        }
    }

    void Start()
    {
        StartCoroutine(SetupRoutine());
    }

    IEnumerator SetupRoutine()
    {
        yield return LoginRoutine();
        yield return SetPlayerNameRoutine();
        yield return leaderboard.FetchTopHighscoresRoutine();
    }

    IEnumerator LoginRoutine()
    {
        bool done = false;
        LootLockerSDKManager.StartGuestSession((response) =>
        {
            if (response.success)
            {
                Debug.Log("Successfully logged in");
                PlayerPrefs.SetString("player_id", response.player_id.ToString());
            }
            else
            {
                Debug.Log("Failed to log in");
            }
            done = true;
        });
        yield return new WaitWhile(() => done == false);
    }

    IEnumerator SetPlayerNameRoutine()
    {
        // Try LoggedInUsername first, then P1, skip generic fallbacks
        string username = PlayerPrefs.GetString("LoggedInUsername", "").Trim();

        if (string.IsNullOrEmpty(username))
            username = PlayerPrefs.GetString("P1", "").Trim();

        // Don't send if empty or a generic name LootLocker rejects
        string[] blockedNames = { "", "player", "player 1", "player 2", "player1", "player2" };
        if (System.Array.Exists(blockedNames, n => n.Equals(username, System.StringComparison.OrdinalIgnoreCase)))
        {
            Debug.Log($"[PlayerManager] Skipping name set — '{username}' is not a valid player name.");
            yield break;
        }

        bool done = false;
        LootLockerSDKManager.SetPlayerName(username, (response) =>
        {
            if (response.success)
            {
                Debug.Log($"Successfully set player name to: {username}");
                PlayerPrefs.SetString("LootLockerPlayerName", username);
            }
            else
            {
                Debug.LogError($"Failed to set player name: {response.errorData}");
            }
            done = true;
        });
        yield return new WaitWhile(() => done == false);
    }
}