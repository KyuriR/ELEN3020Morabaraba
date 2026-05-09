using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LootLocker.Requests;
using TMPro; 

public class Leaderboard : MonoBehaviour
{
    int leaderboardID = 34448; 
    public TextMeshProUGUI playerNames;
    public TextMeshProUGUI playerScore;

    // Track current player's total score
    private int currentPlayerScore = 0;
    private string currentPlayerID;

    void Start()
    {
        currentPlayerID = PlayerPrefs.GetString("player_id");
        LoadCurrentPlayerScore();
    }

    // Call this method when a player wins
    public void AddWinPoint()
    {
        currentPlayerScore++;
        StartCoroutine(SubmitScoreRoutine(currentPlayerScore));
    }


    public IEnumerator SubmitScoreRoutine(int scoreToUpload)
    {
        // Update player name before submitting score
        yield return UpdatePlayerName();

        bool done = false;
        string playerID = PlayerPrefs.GetString("player_id");

        LootLockerSDKManager.SubmitScore(playerID, scoreToUpload, leaderboardID.ToString(), (response) =>
        {
            if (response.success)
            {
                Debug.Log($"Successfully uploaded score: {scoreToUpload}");
                PlayerPrefs.SetInt("PlayerTotalScore", scoreToUpload);
                done = true;
            }
            else
            {
                Debug.Log($"Failed to upload score: {response.errorData}");
                done = true;
            }
            done = true;
        });
        yield return new WaitWhile(() => done == false);
    }
    
    public IEnumerator FetchTopHighscoresRoutine()
    {
        bool done = false;


        LootLockerSDKManager.GetScoreList(leaderboardID.ToString(), 5, 0, (response) =>
        {
            if (response.success)
            {

                string tempPlayerNames = "Player Names:\n";
                string tempPlayerScores = "Player Scores:\n"; 
                
                LootLockerLeaderboardMember[] members = response.items;

                for (int i = 0; i < members.Length; i++)
                {
                    // Add rank and name on the same line, then line break
                    tempPlayerNames += members[i].rank + ". ";
                    if (members[i].player.name != "  ")
                    {
                        tempPlayerNames += members[i].player.name;
                    }
                    else
                    {
                        tempPlayerNames += members[i].player.id;
                    }
                    tempPlayerNames += "\n"; // Line break after each player name

                    // Add score with line break
                    tempPlayerScores += members[i].score + "\n";
                }

                done = true;
                playerNames.text = tempPlayerNames;
                playerScore.text = tempPlayerScores;
            }
            else            
            {
                Debug.Log("Failed to fetch scores: " + response.errorData);
                done = true;
            }

        });
        yield return new WaitWhile(() => done == false);
    }

    IEnumerator UpdatePlayerName()
    {
        string username = PlayerPrefs.GetString("LoggedInUsername", "");
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
                    Debug.Log($"Updated player name to: {username}");
                }
                done = true;
            });
            yield return new WaitWhile(() => done == false);
        }

    }
    void LoadCurrentPlayerScore()
    {
        // Load previously saved score
        currentPlayerScore = PlayerPrefs.GetInt("PlayerTotalScore", 0);
    }

}
