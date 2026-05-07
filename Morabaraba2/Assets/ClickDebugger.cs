using UnityEngine;

/// <summary>
/// TEMPORARY DIAGNOSTIC SCRIPT — delete after fixing.
/// Attach to any GameObject in GameScene (e.g. your GameManager).
/// Shows exactly what the click is hitting (or not hitting) each time you click.
/// </summary>
public class ClickDebugger : MonoBehaviour
{
    void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        worldPos.z = 0;

        Debug.Log($"[ClickDebug] Click at world position: {worldPos}");

        // ── 1. What did Physics2D hit? ─────────────────────────────────────
        Collider2D hit = Physics2D.OverlapPoint(worldPos);
        if (hit != null)
            Debug.Log($"[ClickDebug] Physics2D hit: '{hit.gameObject.name}' tag='{hit.tag}' layer='{LayerMask.LayerToName(hit.gameObject.layer)}'");
        else
            Debug.Log("[ClickDebug] Physics2D hit: NOTHING");

        // ── 2. All colliders within 1 unit ─────────────────────────────────
        Collider2D[] nearby = Physics2D.OverlapCircleAll(worldPos, 1f);
        if (nearby.Length == 0)
        {
            Debug.Log("[ClickDebug] No colliders within 1 unit of click.");
        }
        else
        {
            Debug.Log($"[ClickDebug] Colliders within 1 unit ({nearby.Length}):");
            foreach (var col in nearby)
                Debug.Log($"  - '{col.gameObject.name}'  tag='{col.tag}'  layer='{LayerMask.LayerToName(col.gameObject.layer)}'  size={col.bounds.size}");
        }

        // ── 3. All Cow components in scene ─────────────────────────────────
        Cow[] allCows = FindObjectsByType<Cow>(FindObjectsSortMode.None);
        Debug.Log($"[ClickDebug] Total Cow components in scene: {allCows.Length}");
        foreach (Cow cow in allCows)
        {
            float dist = Vector3.Distance(worldPos, cow.transform.position);
            Collider2D cowCol = cow.GetComponent<Collider2D>();
            bool hasCollider = cowCol != null;
            bool colEnabled = hasCollider && cowCol.enabled;
            string placedOn = cow.GetPlacedNode() != null ? cow.GetPlacedNode().nodeID.ToString() : "NOT PLACED";

            Debug.Log($"  Cow '{cow.gameObject.name}'  player={cow.playerNumber}  dist={dist:F2}  " +
                      $"collider={hasCollider}  enabled={colEnabled}  placedNode={placedOn}  " +
                      $"pos={cow.transform.position}");
        }
    }
}