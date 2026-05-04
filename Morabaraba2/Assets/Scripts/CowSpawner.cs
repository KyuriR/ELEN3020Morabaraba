using UnityEngine;

/// <summary>
/// Spawns the 12 cow prefabs for one player at their starting sidebar positions.
/// Place two of these in the scene, one per player.
///
/// INSPECTOR SETUP:
///   - Assign cowPrefab
///   - Set playerNumber (1 or 2)
///   - Set startPosition to the bottom of the sidebar for this player
/// </summary>
public class CowSpawner : MonoBehaviour
{
    public GameObject cowPrefab;
    public int playerNumber;
    public int numberOfCows = 12;
    public Vector2 startPosition;

    void Start()
    {
        for (int i = 0; i < numberOfCows; i++)
        {
            Vector2 spawnPos = new Vector2(startPosition.x, startPosition.y + i * 0.62f);
            GameObject cow = Instantiate(cowPrefab, spawnPos, Quaternion.identity, transform);
            Cow cowScript = cow.GetComponent<Cow>();
            cowScript.playerNumber = playerNumber;
        }
    }
}
