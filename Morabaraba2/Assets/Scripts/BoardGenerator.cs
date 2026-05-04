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
        new Vector2(-3.277f, 3.17f),  new Vector2(-0.007f, 3.17f),  new Vector2(3.262f, 3.17f),
        new Vector2(3.263f, 0.037f),  new Vector2(3.263f, -3.07f),  new Vector2(0f, -3.07f),
        new Vector2(-3.282f, -3.07f), new Vector2(-3.282f, 0.03f),
        // Middle Square
        new Vector2(-2.188f, 2.166f),  new Vector2(-0.005f, 2.166f),new Vector2(2.186f, 2.166f),
        new Vector2(2.186f, 0.033f), new Vector2(2.186f, -2.087f),  new Vector2(-0.01f, -2.087f),
        new Vector2(-2.183f, -2.087f), new Vector2(-2.183f, 0.034f),
        // Inner Square
        new Vector2(-1.114f, 1.117f),  new Vector2(-0.006f, 1.117f),       new Vector2(1.102f, 1.117f),
        new Vector2(1.102f, 0.034f),       new Vector2(1.102f, -1.049f),
        new Vector2(-0.013f,-1.049f),      new Vector2(-1.115f, -1.049f), new Vector2(-1.115f, 0.034f),
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
