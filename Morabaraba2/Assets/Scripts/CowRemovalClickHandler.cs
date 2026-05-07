using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles mouse clicks during the removal phase.
///
/// ROOT CAUSE FIX:
/// The previous version tried to find cows by collider hit or by cow.GetPlacedNode().
/// Both failed because SpawnRemoteCow() disables the cow's collider AND never calls
/// ForcePlace(), so placed cows had collider=disabled and placedNode=null.
///
/// NEW APPROACH:
/// We skip cows entirely. We know WHICH nodes are removable (from GameManager).
/// We just check if the click landed near any of those node positions.
/// No colliders needed. No placedNode needed. Always works.
/// </summary>
public class CowRemovalClickHandler : MonoBehaviour
{
    private bool removalModeActive = false;
    private List<int> removableNodes = new List<int>();
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    public void ActivateRemovalMode(List<int> nodes)
    {
        removalModeActive = true;
        removableNodes = new List<int>(nodes);
        Debug.Log("[Removal] Activated. Eligible nodes: " + string.Join(", ", nodes));
    }

    public void DeactivateRemovalMode()
    {
        removalModeActive = false;
        removableNodes.Clear();
    }

    void Update()
    {
        if (!removalModeActive) return;
        if (!Input.GetMouseButtonDown(0)) return;

        Vector3 worldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        worldPos.z = 0;

        int clickedNode = FindClosestRemovableNode(worldPos);

        if (clickedNode >= 0)
        {
            Debug.Log("[Removal] Removing cow on node " + clickedNode);
            GameManager.Instance.RemoveCow(clickedNode);
        }
        else
        {
            Debug.Log("[Removal] No removable node near click.");
        }
    }

    // ── Find which removable node the player clicked ───────────────────────
    // Checks the click position against the world position of each removable
    // node directly. No colliders, no cow scripts involved.
    private int FindClosestRemovableNode(Vector3 clickPos)
    {
        float threshold = 0.6f;   // world units — widen if clicks feel too tight
        int bestNode = -1;
        float bestDist = threshold;

        foreach (int nodeID in removableNodes)
        {
            BoardNode node = GetNodeByID(nodeID);
            if (node == null) continue;

            float dist = Vector3.Distance(clickPos, node.transform.position);
            if (dist < bestDist)
            {
                bestDist = dist;
                bestNode = nodeID;
            }
        }

        return bestNode;
    }

    private BoardNode GetNodeByID(int id)
    {
        foreach (BoardNode n in FindObjectsByType<BoardNode>(FindObjectsSortMode.None))
            if (n.nodeID == id) return n;
        return null;
    }
}