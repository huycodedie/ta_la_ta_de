using UnityEngine;

namespace WuxiaGame.UI.Core
{
    /// <summary>
    /// Centralized visual style tokens for mobile Wuxia / Anime RPG presentation.
    /// Provides consistent colors and layout constants without creating gameplay dependencies.
    /// </summary>
    public static class UIStyleConfig
    {
        // Reference portrait canvas size
        public static readonly Vector2 ReferenceResolution = new Vector2(1080f, 1920f);

        // Navigation sizing
        public const float BottomNavHeight = 160f;
        public const float NavIconSize = 56f;
        public const float MainHubButtonSize = 92f;

        // Backgrounds (Dark Ink / Wuxia Slate)
        public static readonly Color PanelBackgroundDark = new Color(0.04f, 0.06f, 0.10f, 0.96f);
        public static readonly Color PanelBackgroundMedium = new Color(0.07f, 0.10f, 0.16f, 0.92f);
        public static readonly Color PanelBackgroundLight = new Color(0.11f, 0.15f, 0.22f, 0.88f);

        // Borders & Accents (Bronze & Antique Gold)
        public static readonly Color BorderGold = new Color(0.85f, 0.68f, 0.28f, 1f);
        public static readonly Color BorderBronze = new Color(0.55f, 0.42f, 0.25f, 0.85f);
        public static readonly Color GoldAccent = new Color(1.00f, 0.85f, 0.25f, 1f);

        // Core Bar Colors (Jade, Crimson, Qi Blue, Rage Orange)
        public static readonly Color JadeGreenHealth = new Color(0.18f, 0.80f, 0.42f, 1f);
        public static readonly Color EnemyHealth = new Color(0.92f, 0.25f, 0.25f, 1f);
        public static readonly Color QiBlueShield = new Color(0.20f, 0.68f, 1.00f, 1f);
        public static readonly Color RageOrange = new Color(1.00f, 0.55f, 0.10f, 1f);
        public static readonly Color CastBarCyan = new Color(0.22f, 0.85f, 1.00f, 1f);

        // Typography Colors
        public static readonly Color TextPrimary = new Color(0.96f, 0.96f, 0.96f, 1f);
        public static readonly Color TextSecondary = new Color(0.75f, 0.78f, 0.85f, 1f);
        public static readonly Color TextMuted = new Color(0.48f, 0.52f, 0.60f, 1f);
        public static readonly Color TextActive = new Color(1.00f, 0.88f, 0.35f, 1f);

        // Navigation Icon Colors
        public static readonly Color IconActive = Color.white;
        public static readonly Color IconMuted = new Color(0.55f, 0.60f, 0.68f, 1f);

        // Button Colors
        public static readonly Color ButtonPrimary = new Color(0.18f, 0.48f, 0.82f, 1f);
        public static readonly Color ButtonSuccess = new Color(0.16f, 0.62f, 0.32f, 1f);
        public static readonly Color ButtonWarning = new Color(0.85f, 0.48f, 0.12f, 1f);
        public static readonly Color ButtonDanger = new Color(0.80f, 0.20f, 0.20f, 1f);
        public static readonly Color ButtonDisabled = new Color(0.28f, 0.30f, 0.35f, 0.70f);

        // Status Effect Badge Colors (Wuxia / Anime Palette)
        public static readonly Color StatusStun = new Color(1.00f, 0.82f, 0.18f, 1f);       // Amber Gold
        public static readonly Color StatusFreeze = new Color(0.35f, 0.85f, 1.00f, 1f);     // Frost Cyan
        public static readonly Color StatusRoot = new Color(0.40f, 0.88f, 0.38f, 1f);       // Bramble Jade
        public static readonly Color StatusAntiCC = new Color(0.85f, 0.45f, 1.00f, 1f);     // Immunity Violet
        public static readonly Color StatusBuff = new Color(0.20f, 0.85f, 0.55f, 1f);       // Emerald Green
        public static readonly Color StatusDebuff = new Color(0.95f, 0.30f, 0.30f, 1f);     // Crimson Red

        // Action Cluster Constants
        public const float NormalSkillButtonSize = 76f;
        public const float UltimateSkillButtonSize = 98f;
        public const float ActionToggleHeight = 36f;

        // Convenient Aliases for Scene Builder and UI Components
        public static readonly Color InkBlackTranslucent = new Color(0.04f, 0.06f, 0.10f, 0.94f);
        public static readonly Color BorderBronzeDark = BorderBronze;
        public static readonly Color CyanHighlight = CastBarCyan;
        public static readonly Color ButtonGold = new Color(0.85f, 0.55f, 0.10f, 1f);
        public static readonly Color ButtonSecondary = new Color(0.20f, 0.45f, 0.75f, 1f);
        public static readonly Color HealthRed = EnemyHealth;
        public static readonly Color HealthJade = JadeGreenHealth;
        public static readonly Color ShieldBlue = QiBlueShield;
        public static readonly Color CastCyan = CastBarCyan;
    }
}
