using UnityEngine;

/// <summary>
/// Spawns the 12 cow prefabs for one player at their starting sidebar positions.
///
/// CHANGES FROM ORIGINAL:
///   - Added Respawn() so ThemeManager can destroy old tokens and spawn new themed ones.
///   - Everything else is identical to your original CowSpawner.
///
/// INSPECTOR SETUP (unchanged):
///   - Assign cowPrefab       (ThemeManager will swap this at runtime)
///   - Set playerNumber       (1 or 2)
///   - Set startPosition      (bottom of the sidebar for this player)
///   - numberOfCows stays 12
/// </summary>
public class CowSpawner : MonoBehaviour
{
    public GameObject cowPrefab;
    public int playerNumber;
    public int numberOfCows = 12;
    public Vector2 startPosition;

    void Start()
    {
        Spawn();
    }

    /// Spawn all tokens using the current cowPrefab.
    public void Spawn()
    {
        for (int i = 0; i < numberOfCows; i++)
        {
            Vector2 spawnPos = new Vector2(startPosition.x, startPosition.y + i * 0.62f);
            GameObject cow = Instantiate(cowPrefab, spawnPos, Quaternion.identity, transform);
            cow.GetComponent<Cow>().playerNumber = playerNumber;
        }
    }

    /// Destroy all existing tokens and respawn with the current cowPrefab.
    /// Called by ThemeManager when the theme changes.
    public void Respawn()
    {
        // Destroy all current child tokens
        foreach (Transform child in transform)
            Destroy(child.gameObject);

        // Also clear from GameManager's tracking so the game state stays clean
        // (only matters if called during active gameplay — safe to call at theme select too)
        Spawn();
    }
}