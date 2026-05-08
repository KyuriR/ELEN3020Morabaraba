using System.Collections;
using System.Collections.Generic;
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
            {
                instance = FindObjectOfType<PlayerManager>();
            }
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

        // Set the player name after login
        SetPlayerName();

        yield return leaderboard.FetchTopHighscoresRoutine();
    }

    IEnumerator LoginRoutine()
    {
        bool done = false;
       LootLockerSDKManager.StartGuestSession((response) =>
        {
            if (response.success)
            {
                Debug.Log("successfully logged in");
               PlayerPrefs.SetString("player_id", response.player_id.ToString());
              done = true;
            }
            else
            {
                Debug.Log("failed to log in");
              done = true;
           }
           
       });
        yield return new WaitWhile(() => done == false);
    }

    void SetPlayerName()
    {
        // Get the username from NetworkUI's saved PlayerPrefs
        string username = PlayerPrefs.GetString("P1", "");

        // Fallback to P1 if LoggedInUsername doesn't exist
        if (string.IsNullOrEmpty(username))
        {
            username = PlayerPrefs.GetString("P1", "Player");
        }

        if (!string.IsNullOrEmpty(username))
        {
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
            StartCoroutine(WaitForNameSet(done));
        }
    }

    IEnumerator WaitForNameSet(bool done)
    {
        yield return new WaitWhile(() => done == false);
    }
}
