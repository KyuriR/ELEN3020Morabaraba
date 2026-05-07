using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles mouse clicks during the removal phase.
/// When active, clicking on an eligible opponent cow removes it.
/// Activated and deactivated by GameManager.
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
        Debug.Log("[Removal] Deactivated.");
    }

    void Update()
    {
        // No logging here — this runs every frame
        if (!removalModeActive) return;
        if (!Input.GetMouseButtonDown(0)) return;

        Vector3 worldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        worldPos.z = 0;

        // Primary: raycast hit on a Cow-tagged collider
        Collider2D hit = Physics2D.OverlapPoint(worldPos);
        if (hit != null && hit.CompareTag("Cow"))
        {
            TryRemoveCow(hit.GetComponent<Cow>());
            return;
        }

        // Fallback: proximity check across all placed cows
        TryRemoveClosestCow(worldPos);
    }

    private void TryRemoveCow(Cow cow)
    {
        if (cow == null) return;

        BoardNode node = cow.GetPlacedNode();
        if (node == null)
        {
            Debug.Log("[Removal] Cow has no placed node.");
            return;
        }

        if (removableNodes.Contains(node.nodeID))
        {
            Debug.Log("[Removal] Removing cow on node " + node.nodeID);
            GameManager.Instance.RemoveCow(node.nodeID);
        }
        else
        {
            Debug.Log("[Removal] Node " + node.nodeID + " is not removable (protected by mill).");
            SoundManager.PlayInvalidMove();
        }
    }

    private void TryRemoveClosestCow(Vector3 clickPosition)
    {
        // Re-find cows each time so we never use a stale list
        Cow[] allCows = FindObjectsByType<Cow>(FindObjectsSortMode.None);
        float threshold = 0.6f;
        Cow closestCow = null;
        float closestDist = threshold;

        foreach (Cow cow in allCows)
        {
            // Only consider cows that are actually placed on the board
            if (cow.GetPlacedNode() == null) continue;

            float dist = Vector3.Distance(clickPosition, cow.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closestCow = cow;
            }
        }

        if (closestCow != null)
            TryRemoveCow(closestCow);
        else
            Debug.Log("[Removal] No cow found near click position.");
    }
}