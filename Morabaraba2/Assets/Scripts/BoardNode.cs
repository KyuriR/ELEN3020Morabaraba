using UnityEngine;

/// <summary>
/// Attached to each of the 24 intersection points on the board.
/// Stores the node's ID and whether it is currently occupied.
/// </summary>
public class BoardNode : MonoBehaviour
{
    public int nodeID;
    public bool isOccupied = false;
}
