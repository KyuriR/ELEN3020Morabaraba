using Photon.Pun;
using UnityEditor.Graphs;
using UnityEngine;

/// <summary>
/// Spawns the 12 cow prefabs for one player at their starting sidebar positions.
using Photon.Pun;
using UnityEngine;

public class CowSpawner : MonoBehaviourPunCallbacks
{
    public int playerNumber;
    public int numberOfCows = 12;
    public Vector2 startPosition;

    public string cowPrefabName = "Prefab/TraditionalCow1";

    public override void OnJoinedRoom()
    {
        Spawn();
    }

    public void Spawn()
    {
        if (!PhotonNetwork.InRoom)
        {
            Debug.LogWarning("Not in room yet - cannot spawn cows.");
            return;
        }

        for (int i = 0; i < numberOfCows; i++)
        {
            Vector2 spawnPos = new Vector2(
                startPosition.x,
                startPosition.y + i * 0.62f
            );

            GameObject cow = PhotonNetwork.Instantiate(
                cowPrefabName,
                spawnPos,
                Quaternion.identity
            );

            cow.transform.SetParent(transform);

            Cow cowScript = cow.GetComponent<Cow>();
            cowScript.playerNumber = playerNumber;
        }
    }

    public void Respawn()
    {
        foreach (Transform child in transform)
        {
            PhotonNetwork.Destroy(child.gameObject);
        }

        Spawn();
    }
}