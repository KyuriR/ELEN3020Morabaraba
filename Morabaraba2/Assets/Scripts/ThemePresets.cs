#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor-only helper.  Select a BoardTheme asset, then use the menu
/// "Morabaraba > Apply Preset > ..." to fill all colour values automatically.
/// Run this once per theme asset, then swap in your own sprites.
/// </summary>
public static class ThemePresets
{
    // ══════════════════════════════════════════════════════════════════════
    // TRADITIONAL — carved wood, cattle-horn pieces, Highveld earth
    // ══════════════════════════════════════════════════════════════════════
    [MenuItem("Morabaraba/Apply Preset/Traditional")]
    public static void ApplyTraditional()
    {
        BoardTheme t = GetSelected(); if (t == null) return;

        t.themeName = "Traditional";

        // Board
        t.boardTint      = new Color(0.55f, 0.36f, 0.18f);   // Teak brown
        t.boardLineColor = new Color(0.22f, 0.12f, 0.04f);   // Dark walnut

        // Nodes
        t.nodeDefaultColor  = new Color(0.18f, 0.10f, 0.04f);
        t.nodeHoverColor    = new Color(0.88f, 0.68f, 0.22f); // Warm gold
        t.nodeValidColor    = new Color(0.45f, 0.72f, 0.18f); // Grass green
        t.nodeOccupiedColor = new Color(0.30f, 0.20f, 0.08f);

        // Player 1 — bone / light cattle-horn
     
        t.player1HighlightColor = new Color(0.98f, 0.82f, 0.30f);

        // Player 2 — dark ebony
      
        t.player2HighlightColor = new Color(0.88f, 0.55f, 0.10f);

        // Mill
        t.millFlashColor = new Color(0.90f, 0.25f, 0.10f);

        // Turn UI
        t.player1TurnColor = new Color(0.93f, 0.67f, 0.25f);  // Ochre
        t.player2TurnColor = new Color(0.55f, 0.32f, 0.12f);  // Deep wood
        t.millAlertColor   = new Color(0.98f, 0.82f, 0.30f);  // Gold

        // Network status
        t.goodStatusColor = new Color(0.45f, 0.72f, 0.18f);
        t.badStatusColor  = new Color(0.80f, 0.20f, 0.10f);

        // Environment
        t.ambientColor = new Color(1.0f, 0.93f, 0.75f);   // Warm afternoon sun

        Save(t);
    }

    // ══════════════════════════════════════════════════════════════════════
    // ANIME — sakura pastels, neon glows, crystal-clear pieces
    // ══════════════════════════════════════════════════════════════════════
    [MenuItem("Morabaraba/Apply Preset/Anime")]
    public static void ApplyAnime()
    {
        BoardTheme t = GetSelected(); if (t == null) return;

        t.themeName = "Anime";

        // Board
        t.boardTint      = new Color(0.95f, 0.90f, 0.97f);   // Soft lavender-white
        t.boardLineColor = new Color(0.72f, 0.52f, 0.88f);   // Purple grid

        // Nodes
        t.nodeDefaultColor  = new Color(0.75f, 0.60f, 0.90f);
        t.nodeHoverColor    = new Color(1.00f, 0.75f, 0.90f); // Sakura pink
        t.nodeValidColor    = new Color(0.55f, 0.95f, 0.75f); // Mint
        t.nodeOccupiedColor = new Color(0.60f, 0.45f, 0.75f);

        // Player 1 — sky crystal / light
       // t.player1Color          = new Color(0.65f, 0.88f, 1.00f);
        t.player1HighlightColor = new Color(1.00f, 0.95f, 0.55f); // Sparkle gold

        // Player 2 — sakura rose / warm
       // t.player2Color          = new Color(1.00f, 0.65f, 0.80f);
        t.player2HighlightColor = new Color(0.75f, 0.50f, 1.00f); // Neon violet

        // Mill
        t.millFlashColor = new Color(1.00f, 0.30f, 0.55f);   // Hot pink

        // Turn UI
        t.player1TurnColor = new Color(0.35f, 0.70f, 1.00f);  // Sky blue
        t.player2TurnColor = new Color(1.00f, 0.45f, 0.65f);  // Rose
        t.millAlertColor   = new Color(1.00f, 0.85f, 0.25f);  // Sparkle yellow

        // Network status
        t.goodStatusColor = new Color(0.35f, 0.90f, 0.60f);
        t.badStatusColor  = new Color(1.00f, 0.30f, 0.55f);

        // Environment
        t.ambientColor = new Color(0.95f, 0.88f, 1.00f);   // Dreamy purple-white

        Save(t);
    }

    // ══════════════════════════════════════════════════════════════════════
    // STAR WARS — deep space blacks, Rebel gold, Imperial grey, Force blue
    // ══════════════════════════════════════════════════════════════════════
    [MenuItem("Morabaraba/Apply Preset/StarWars")]
    public static void ApplyStarWars()
    {
        BoardTheme t = GetSelected(); if (t == null) return;

        t.themeName = "Star Wars";

        // Board
        t.boardTint      = new Color(0.08f, 0.08f, 0.12f);   // Deep space black
        t.boardLineColor = new Color(0.90f, 0.75f, 0.30f);   // Hyperdrive gold

        // Nodes
        t.nodeDefaultColor  = new Color(0.30f, 0.35f, 0.45f); // Steel grey
        t.nodeHoverColor    = new Color(0.25f, 0.70f, 1.00f); // Lightsaber blue
        t.nodeValidColor    = new Color(0.15f, 0.85f, 0.45f); // Holocron green
        t.nodeOccupiedColor = new Color(0.20f, 0.22f, 0.30f);

        // Player 1 — Rebel Alliance (warm gold / tan)
      //  t.player1Color          = new Color(0.92f, 0.80f, 0.35f); // Rebel gold
        t.player1HighlightColor = new Color(1.00f, 0.95f, 0.60f); // Bright gold glow

        // Player 2 — Galactic Empire (cold chrome / grey-white)
       // t.player2Color          = new Color(0.75f, 0.78f, 0.85f); // Imperial chrome
        t.player2HighlightColor = new Color(0.85f, 0.30f, 0.20f); // Red Sith glow

        // Mill — Sith lightning red
        t.millFlashColor = new Color(0.90f, 0.15f, 0.10f);

        // Turn UI
        t.player1TurnColor = new Color(0.92f, 0.80f, 0.30f);  // Rebel gold
        t.player2TurnColor = new Color(0.75f, 0.78f, 0.88f);  // Imperial silver
        t.millAlertColor   = new Color(0.90f, 0.15f, 0.10f);  // Sith red

        // Network status
        t.goodStatusColor = new Color(0.15f, 0.85f, 0.45f);   // Holocron green
        t.badStatusColor  = new Color(0.90f, 0.15f, 0.10f);   // Sith red

        // Environment
        t.ambientColor = new Color(0.15f, 0.18f, 0.28f);   // Starfield blue-black

        Save(t);
    }

    // ── Helpers ────────────────────────────────────────────────────────────
    private static BoardTheme GetSelected()
    {
        BoardTheme t = Selection.activeObject as BoardTheme;
        if (t == null)
            Debug.LogWarning("[ThemePresets] Select a BoardTheme asset first.");
        return t;
    }

    private static void Save(BoardTheme t)
    {
        EditorUtility.SetDirty(t);
        AssetDatabase.SaveAssets();
        Debug.Log($"[ThemePresets] Applied preset to '{t.themeName}'.");
    }
}
#endif
