using UnityEngine;

/// <summary>
/// Instantiates all 24 board nodes at their correct positions.
/// Attach to an empty GameObject in GameScene.
/// Assign nodePrefab in the Inspector.
/// </summary>
public class BoardGenerator : MonoBehaviour
{
    public GameObject nodePrefab;

    private Vector2[] nodePositions =
    {
        // Outer Square (clockwise)
        new Vector2(-3.3f, 3.25f),  new Vector2(-0.02f, 3.25f),  new Vector2(3.27f, 3.25f),
        new Vector2(3.27f, 0.065f),  new Vector2(3.27f, -3.13f),  new Vector2(-0.01f, -3.13f),
        new Vector2(-3.29f, -3.13f), new Vector2(-3.29f, 0.064f),
        // Middle Square
        new Vector2(-2.173f, 2.175f),  new Vector2(-0.02f, 2.175f),new Vector2(2.141f, 2.175f),
        new Vector2(2.141f, 0.065f), new Vector2(2.141f, -2.05f),  new Vector2(-0.01f, -2.05f),
        new Vector2(-2.17f, -2.05f), new Vector2(-2.17f, 0.064f),
        // Inner Square
        new Vector2(-1.08f, 1.12f),  new Vector2(-0.02f, 1.12f),       new Vector2(1.05f, 1.12f),
        new Vector2(1.05f, 0.065f),       new Vector2(1.05f, -0.99f),
        new Vector2(-0.01f,-0.99f),      new Vector2(-1.08f, -0.99f), new Vector2(-1.08f, 0.064f),
    };

    void Start()
    {
        for (int i = 0; i < nodePositions.Length; i++)
        {
            GameObject node = Instantiate(nodePrefab, nodePositions[i], Quaternion.identity, transform);
            BoardNode nodeScript = node.GetComponent<BoardNode>();
            nodeScript.nodeID = i;
        }
    }
}
