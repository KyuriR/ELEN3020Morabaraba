using UnityEngine;

/// <summary>
/// Attach to your BoardNode prefab.
/// Nodes have no colour tinting — they show their sprite as-is at all times.
/// SetHovered and SetValid are kept so nothing else breaks, but they do nothing.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class BoardNodeThemer : MonoBehaviour
{
    void Start()
    {
        // Make sure the node sprite always shows its true colour with no tint
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
 
    }

    // These are called by other scripts — kept so nothing breaks
    public void RefreshTheme() { }
    public void SetHovered(bool hovered) { }
    public void SetValid(bool valid) { }
}