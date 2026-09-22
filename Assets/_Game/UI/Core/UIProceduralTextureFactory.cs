using System;
using UnityEngine;

namespace WuxiaGame.UI.Core
{
    /// <summary>
    /// Procedural texture and sprite generator for Wuxia / Anime RPG presentation.
    /// Creates 9-sliced panels, beveled bars, metallic frames, martial art silhouettes,
    /// and atmospheric combat environment backdrops at runtime without external asset dependencies.
    /// Pure presentation layer.
    /// </summary>
    public static class UIProceduralTextureFactory
    {
        private static Sprite cachedPanelSprite;
        private static Sprite cachedBarBgSprite;
        private static Sprite cachedBarFillSprite;
        private static Sprite cachedCircleSprite;
        private static Sprite cachedSkillFrameSprite;
        private static Sprite cachedUltFrameSprite;
        private static Sprite cachedCombatBgSprite;
        private static Sprite cachedPlatformSprite;
        private static Sprite cachedHeroStandeeSprite;
        private static Sprite cachedMonsterStandeeSprite;

        public static Sprite GetPanelSprite()
        {
            if (cachedPanelSprite == null)
            {
                int size = 64;
                Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
                Color[] colors = new Color[size * size];

                Color outerBorder = new Color(0.72f, 0.56f, 0.28f, 0.95f); // Antique bronze/gold
                Color innerBorder = new Color(0.35f, 0.28f, 0.18f, 0.90f);
                Color bgTop = new Color(0.08f, 0.11f, 0.16f, 0.95f);       // Deep charcoal ink
                Color bgBottom = new Color(0.03f, 0.04f, 0.07f, 0.98f);

                for (int y = 0; y < size; y++)
                {
                    float tY = (float)y / (size - 1);
                    Color currentBg = Color.Lerp(bgBottom, bgTop, tY);

                    for (int x = 0; x < size; x++)
                    {
                        int dLeft = x;
                        int dRight = size - 1 - x;
                        int dBottom = y;
                        int dTop = size - 1 - y;
                        int distEdge = Mathf.Min(Mathf.Min(dLeft, dRight), Mathf.Min(dBottom, dTop));

                        // Corner chamfer
                        int cornerDist = 0;
                        if ((x < 8 || x >= size - 8) && (y < 8 || y >= size - 8))
                        {
                            int cx = x < 8 ? 8 - x : x - (size - 9);
                            int cy = y < 8 ? 8 - y : y - (size - 9);
                            cornerDist = cx + cy;
                        }

                        if (cornerDist > 9)
                        {
                            colors[y * size + x] = Color.clear;
                        }
                        else if (cornerDist >= 7 || distEdge == 0 || distEdge == 1)
                        {
                            colors[y * size + x] = outerBorder;
                        }
                        else if (distEdge == 2 || distEdge == 3)
                        {
                            colors[y * size + x] = innerBorder;
                        }
                        else
                        {
                            colors[y * size + x] = currentBg;
                        }
                    }
                }

                tex.SetPixels(colors);
                tex.Apply();
                tex.filterMode = FilterMode.Bilinear;
                cachedPanelSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(12, 12, 12, 12));
            }
            return cachedPanelSprite;
        }

        public static Sprite GetBarBgSprite()
        {
            if (cachedBarBgSprite == null)
            {
                int w = 64;
                int h = 32;
                Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
                Color[] colors = new Color[w * h];

                Color borderCol = new Color(0.40f, 0.32f, 0.22f, 0.95f);
                Color innerBg = new Color(0.05f, 0.06f, 0.09f, 0.92f);

                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        int dEdge = Mathf.Min(Mathf.Min(x, w - 1 - x), Mathf.Min(y, h - 1 - y));
                        if (dEdge <= 1)
                        {
                            colors[y * w + x] = borderCol;
                        }
                        else
                        {
                            colors[y * w + x] = innerBg;
                        }
                    }
                }

                tex.SetPixels(colors);
                tex.Apply();
                cachedBarBgSprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(4, 4, 4, 4));
            }
            return cachedBarBgSprite;
        }

        public static Sprite GetBarFillSprite()
        {
            if (cachedBarFillSprite == null)
            {
                int w = 32;
                int h = 32;
                Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
                Color[] colors = new Color[w * h];

                for (int y = 0; y < h; y++)
                {
                    float shine = y >= h - 4 ? 0.35f : (y >= h - 8 ? 0.15f : 0f);
                    for (int x = 0; x < w; x++)
                    {
                        float val = Mathf.Clamp01(1f + shine);
                        colors[y * w + x] = new Color(val, val, val, 1f);
                    }
                }

                tex.SetPixels(colors);
                tex.Apply();
                cachedBarFillSprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(2, 2, 2, 2));
            }
            return cachedBarFillSprite;
        }

        public static Sprite GetCircleSprite()
        {
            if (cachedCircleSprite == null)
            {
                int size = 64;
                Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
                Color[] colors = new Color[size * size];
                float radius = size * 0.5f;

                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        float dist = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(radius, radius));
                        if (dist > radius)
                        {
                            colors[y * size + x] = Color.clear;
                        }
                        else if (dist > radius - 2f)
                        {
                            float alpha = Mathf.Clamp01(radius - dist);
                            colors[y * size + x] = new Color(0.85f, 0.70f, 0.30f, alpha);
                        }
                        else if (dist > radius - 4f)
                        {
                            colors[y * size + x] = new Color(0.85f, 0.70f, 0.30f, 1f);
                        }
                        else
                        {
                            colors[y * size + x] = Color.white;
                        }
                    }
                }

                tex.SetPixels(colors);
                tex.Apply();
                cachedCircleSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            }
            return cachedCircleSprite;
        }

        public static Sprite GetSkillSlotBgSprite() => GetCircleSprite();
        public static Sprite GetRadialCooldownSprite() => GetCircleSprite();

        public static Sprite GetSkillFrameSprite(bool isUltimate = false)
        {
            if (isUltimate)
            {
                if (cachedUltFrameSprite == null)
                {
                    cachedUltFrameSprite = CreateCircularSkillFrame(true);
                }
                return cachedUltFrameSprite;
            }
            else
            {
                if (cachedSkillFrameSprite == null)
                {
                    cachedSkillFrameSprite = CreateCircularSkillFrame(false);
                }
                return cachedSkillFrameSprite;
            }
        }

        private static Sprite CreateCircularSkillFrame(bool isUltimate)
        {
            int size = 96;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color[] colors = new Color[size * size];
            float r = size * 0.5f;

            Color goldOuter = isUltimate ? new Color(1.00f, 0.85f, 0.25f, 1f) : new Color(0.72f, 0.56f, 0.28f, 1f);
            Color bronzeMid = isUltimate ? new Color(0.85f, 0.55f, 0.15f, 1f) : new Color(0.40f, 0.32f, 0.20f, 1f);
            Color bgDark = new Color(0.06f, 0.08f, 0.12f, 0.95f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(r, r));
                    if (d > r)
                    {
                        colors[y * size + x] = Color.clear;
                    }
                    else if (d > r - 3.5f)
                    {
                        colors[y * size + x] = goldOuter;
                    }
                    else if (d > r - 7f)
                    {
                        colors[y * size + x] = bronzeMid;
                    }
                    else
                    {
                        colors[y * size + x] = bgDark;
                    }
                }
            }

            tex.SetPixels(colors);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        public static Sprite GetCombatBackgroundSprite()
        {
            if (cachedCombatBgSprite == null)
            {
                int w = 256;
                int h = 512;
                Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
                Color[] colors = new Color[w * h];

                Color skyTop = new Color(0.04f, 0.06f, 0.12f, 1f);      // Midnight indigo
                Color skyMid = new Color(0.08f, 0.14f, 0.22f, 1f);      // Moonlit teal
                Color mistColor = new Color(0.12f, 0.20f, 0.28f, 1f);   // Valley haze
                Color groundDark = new Color(0.05f, 0.07f, 0.10f, 1f);  // Base shadow

                for (int y = 0; y < h; y++)
                {
                    float tY = (float)y / (h - 1);
                    Color skyCol;
                    if (tY > 0.6f)
                    {
                        skyCol = Color.Lerp(skyMid, skyTop, (tY - 0.6f) / 0.4f);
                    }
                    else if (tY > 0.25f)
                    {
                        skyCol = Color.Lerp(mistColor, skyMid, (tY - 0.25f) / 0.35f);
                    }
                    else
                    {
                        skyCol = Color.Lerp(groundDark, mistColor, tY / 0.25f);
                    }

                    // Procedural mountain silhouettes
                    for (int x = 0; x < w; x++)
                    {
                        float nx = (float)x / w;
                        // Mountain 1 (Far peaks)
                        float peak1 = 0.55f + 0.12f * Mathf.Sin(nx * 9f) + 0.06f * Mathf.Cos(nx * 17f);
                        // Mountain 2 (Near peaks)
                        float peak2 = 0.40f + 0.10f * Mathf.Cos(nx * 6f + 1.2f) + 0.04f * Mathf.Sin(nx * 13f);

                        Color finalCol = skyCol;
                        if (tY < peak1 && tY >= peak2)
                        {
                            finalCol = Color.Lerp(new Color(0.06f, 0.10f, 0.16f, 1f), skyCol, 0.35f);
                        }
                        else if (tY < peak2)
                        {
                            finalCol = Color.Lerp(new Color(0.04f, 0.07f, 0.12f, 1f), groundDark, 0.5f);
                        }

                        colors[y * w + x] = finalCol;
                    }
                }

                tex.SetPixels(colors);
                tex.Apply();
                cachedCombatBgSprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f);
            }
            return cachedCombatBgSprite;
        }

        public static Sprite GetPlatformSprite()
        {
            if (cachedPlatformSprite == null)
            {
                int w = 256;
                int h = 48;
                Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
                Color[] colors = new Color[w * h];

                Color stoneTop = new Color(0.32f, 0.38f, 0.44f, 1f);   // Weathered granite
                Color stoneBody = new Color(0.18f, 0.22f, 0.28f, 1f);
                Color stoneBottom = new Color(0.08f, 0.10f, 0.14f, 1f);
                Color goldTrim = new Color(0.75f, 0.60f, 0.30f, 1f);   // Carved cloud trim

                for (int y = 0; y < h; y++)
                {
                    float tY = (float)y / (h - 1);
                    for (int x = 0; x < w; x++)
                    {
                        float tX = (float)x / (w - 1);
                        float edgeFade = Mathf.SmoothStep(0f, 0.15f, tX) * Mathf.SmoothStep(0f, 0.15f, 1f - tX);

                        Color c;
                        if (y >= h - 4)
                        {
                            c = goldTrim;
                        }
                        else if (y >= h - 10)
                        {
                            c = stoneTop;
                        }
                        else
                        {
                            c = Color.Lerp(stoneBottom, stoneBody, tY);
                        }

                        c.a = edgeFade;
                        colors[y * w + x] = c;
                    }
                }

                tex.SetPixels(colors);
                tex.Apply();
                cachedPlatformSprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f);
            }
            return cachedPlatformSprite;
        }

        public static Sprite GetHeroStandeeSprite()
        {
            if (cachedHeroStandeeSprite == null)
            {
                int w = 96;
                int h = 160;
                Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
                Color[] colors = new Color[w * h];

                Color heroAura = new Color(0.20f, 0.70f, 1.00f, 0.30f); // Cyan Qi aura
                Color heroRobe = new Color(0.15f, 0.45f, 0.75f, 0.95f); // Azure Daoist robe
                Color heroTrim = new Color(0.90f, 0.82f, 0.60f, 1.00f); // Gold collar / sash
                Color swordSteel = new Color(0.85f, 0.95f, 1.00f, 1.00f);

                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        float nx = (x - 48f) / 48f;
                        float ny = (float)y / h;

                        Color pixel = Color.clear;

                        // Outer Qi Aura glow
                        float auraDist = Vector2.Distance(new Vector2(x, y), new Vector2(48, 80));
                        if (auraDist < 60f)
                        {
                            float auraIntensity = Mathf.Clamp01((60f - auraDist) / 60f) * 0.4f;
                            pixel = new Color(heroAura.r, heroAura.g, heroAura.b, auraIntensity);
                        }

                        // Head & Daoist Crown (y: 125 to 150)
                        if (y >= 125 && y <= 145 && Mathf.Abs(nx) < 0.22f)
                        {
                            pixel = heroTrim;
                        }
                        // Torso (y: 80 to 125)
                        else if (y >= 80 && y < 125 && Mathf.Abs(nx) < 0.38f)
                        {
                            pixel = (Mathf.Abs(nx) < 0.12f) ? heroTrim : heroRobe;
                        }
                        // Flowing Robe / Hakama Skirt (y: 20 to 80)
                        else if (y >= 20 && y < 80)
                        {
                            float spread = 0.35f + (80f - y) / 80f * 0.35f;
                            if (Mathf.Abs(nx) < spread)
                            {
                                pixel = heroRobe;
                            }
                        }
                        // Slanted Jian / Long Sword on back (x: 55 to 70, y: 50 to 155)
                        float swordLineX = 52f + (y - 50f) * 0.15f;
                        if (Mathf.Abs(x - swordLineX) < 2f && y >= 50 && y <= 155)
                        {
                            pixel = swordSteel;
                        }

                        colors[y * w + x] = pixel;
                    }
                }

                tex.SetPixels(colors);
                tex.Apply();
                cachedHeroStandeeSprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0f), 50f);
            }
            return cachedHeroStandeeSprite;
        }

        public static Sprite GetMonsterStandeeSprite()
        {
            if (cachedMonsterStandeeSprite == null)
            {
                int w = 112;
                int h = 160;
                Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
                Color[] colors = new Color[w * h];

                Color demonAura = new Color(0.95f, 0.20f, 0.20f, 0.35f); // Crimson Demonic aura
                Color beastHide = new Color(0.40f, 0.12f, 0.15f, 0.95f); // Dark fiend hide
                Color beastMane = new Color(0.20f, 0.05f, 0.08f, 1.00f);
                Color eyeGlow = new Color(1.00f, 0.85f, 0.15f, 1.00f);   // Golden fiend eyes

                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        float nx = (x - 56f) / 56f;
                        float ny = (float)y / h;

                        Color pixel = Color.clear;

                        // Demonic outer aura
                        float auraDist = Vector2.Distance(new Vector2(x, y), new Vector2(56, 75));
                        if (auraDist < 65f)
                        {
                            float intensity = Mathf.Clamp01((65f - auraDist) / 65f) * 0.45f;
                            pixel = new Color(demonAura.r, demonAura.g, demonAura.b, intensity);
                        }

                        // Beast Head / Horns (y: 110 to 145)
                        if (y >= 110 && y <= 145 && Mathf.Abs(nx) < 0.45f)
                        {
                            // Eyes
                            if (y >= 120 && y <= 126 && (Mathf.Abs(x - 46) <= 2 || Mathf.Abs(x - 66) <= 2))
                            {
                                pixel = eyeGlow;
                            }
                            else
                            {
                                pixel = beastMane;
                            }
                        }
                        // Muscular Beast Torso (y: 60 to 110)
                        else if (y >= 60 && y < 110 && Mathf.Abs(nx) < 0.55f)
                        {
                            pixel = beastHide;
                        }
                        // Claws & Lower Stance (y: 15 to 60)
                        else if (y >= 15 && y < 60)
                        {
                            float spread = 0.45f + (60f - y) / 60f * 0.35f;
                            if (Mathf.Abs(nx) < spread)
                            {
                                pixel = beastMane;
                            }
                        }

                        colors[y * w + x] = pixel;
                    }
                }

                tex.SetPixels(colors);
                tex.Apply();
                cachedMonsterStandeeSprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0f), 50f);
            }
            return cachedMonsterStandeeSprite;
        }

        public enum WuxiaIconType
        {
            BasicAttack,
            PalmSkill,
            BladeSkill,
            GaleSkill,
            UltimateDragon,
            InventoryBag,
            MindMethodScroll,
            MainBattleTemple,
            SectBanner,
            SettingsGear
        }

        public static Sprite GetIconSprite(string key)
        {
            if (string.IsNullOrEmpty(key)) return GetIconSprite(WuxiaIconType.BasicAttack);
            switch (key.ToLower())
            {
                case "swords":
                case "attack":
                case "basic":
                    return GetIconSprite(WuxiaIconType.BasicAttack);
                case "palm":
                case "strike":
                    return GetIconSprite(WuxiaIconType.PalmSkill);
                case "blade":
                case "sword":
                    return GetIconSprite(WuxiaIconType.BladeSkill);
                case "gale":
                case "wind":
                    return GetIconSprite(WuxiaIconType.GaleSkill);
                case "dragon":
                case "ultimate":
                    return GetIconSprite(WuxiaIconType.UltimateDragon);
                case "chest":
                case "bag":
                case "inventory":
                    return GetIconSprite(WuxiaIconType.InventoryBag);
                case "scroll":
                case "mindmethod":
                    return GetIconSprite(WuxiaIconType.MindMethodScroll);
                case "temple":
                case "battle":
                case "hub":
                    return GetIconSprite(WuxiaIconType.MainBattleTemple);
                case "banner":
                case "sect":
                case "guild":
                    return GetIconSprite(WuxiaIconType.SectBanner);
                case "gear":
                case "settings":
                    return GetIconSprite(WuxiaIconType.SettingsGear);
                default:
                    return GetIconSprite(WuxiaIconType.BasicAttack);
            }
        }

        public static Sprite GetIconSprite(WuxiaIconType iconType)
        {
            int size = 64;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color[] colors = new Color[size * size];
            Color iconCol = Color.white;

            for (int i = 0; i < colors.Length; i++) colors[i] = Color.clear;

            int cx = size / 2;
            int cy = size / 2;

            switch (iconType)
            {
                case WuxiaIconType.BasicAttack: // Crossed swords
                    for (int d = -20; d <= 20; d++)
                    {
                        DrawThickPixel(colors, size, cx + d, cy + d, 2, iconCol);
                        DrawThickPixel(colors, size, cx + d, cy - d, 2, iconCol);
                    }
                    break;

                case WuxiaIconType.PalmSkill: // Buddhist / Wuxia palm crest
                    for (int y = -14; y <= 14; y++)
                    {
                        for (int x = -10; x <= 10; x++)
                        {
                            if (Mathf.Abs(x) + Mathf.Abs(y) <= 18)
                            {
                                DrawThickPixel(colors, size, cx + x, cy + y, 1, iconCol);
                            }
                        }
                    }
                    break;

                case WuxiaIconType.BladeSkill: // Curved crescent blade
                    for (float a = 0; a < Mathf.PI; a += 0.05f)
                    {
                        int bx = cx + (int)(Mathf.Cos(a) * 18f);
                        int by = cy + (int)(Mathf.Sin(a) * 18f);
                        DrawThickPixel(colors, size, bx, by, 3, iconCol);
                    }
                    break;

                case WuxiaIconType.GaleSkill: // Swirling wind spiral
                    for (float a = 0; a < Mathf.PI * 3f; a += 0.1f)
                    {
                        float rad = a * 2.2f;
                        int wx = cx + (int)(Mathf.Cos(a) * rad);
                        int wy = cy + (int)(Mathf.Sin(a) * rad);
                        DrawThickPixel(colors, size, wx, wy, 2, iconCol);
                    }
                    break;

                case WuxiaIconType.UltimateDragon: // Golden Dragon / Flame Core
                    for (int r = 0; r <= 16; r++)
                    {
                        for (float a = 0; a < Mathf.PI * 2f; a += 0.1f)
                        {
                            int dx = cx + (int)(Mathf.Cos(a) * r);
                            int dy = cy + (int)(Mathf.Sin(a) * r);
                            // 8-point flame star
                            float starMod = 1f + 0.35f * Mathf.Cos(a * 4f);
                            if (r <= 12f * starMod)
                            {
                                DrawThickPixel(colors, size, dx, dy, 1, iconCol);
                            }
                        }
                    }
                    break;

                case WuxiaIconType.InventoryBag: // Knapsack / chest silhouette
                    for (int y = -12; y <= 10; y++)
                    {
                        for (int x = -14; x <= 14; x++)
                        {
                            if (Mathf.Abs(x) <= 14 && y <= 8)
                            {
                                DrawThickPixel(colors, size, cx + x, cy + y, 1, iconCol);
                            }
                        }
                    }
                    break;

                case WuxiaIconType.MindMethodScroll: // Ancient parchment scroll
                    for (int y = -16; y <= 16; y++)
                    {
                        for (int x = -10; x <= 10; x++)
                        {
                            DrawThickPixel(colors, size, cx + x, cy + y, 1, iconCol);
                        }
                    }
                    break;

                case WuxiaIconType.MainBattleTemple: // Pagoda / main gate
                    for (int y = -14; y <= 14; y++)
                    {
                        int span = (14 - y) / 2 + 4;
                        for (int x = -span; x <= span; x++)
                        {
                            DrawThickPixel(colors, size, cx + x, cy + y, 1, iconCol);
                        }
                    }
                    break;

                case WuxiaIconType.SectBanner: // Cultivation sect banner
                    for (int y = -16; y <= 16; y++)
                    {
                        DrawThickPixel(colors, size, cx - 12, cy + y, 2, iconCol);
                        if (y >= 0)
                        {
                            for (int x = -12; x <= 12; x++)
                            {
                                DrawThickPixel(colors, size, cx + x, cy + y, 1, iconCol);
                            }
                        }
                    }
                    break;

                case WuxiaIconType.SettingsGear: // Ba Gua / seal octagon
                    for (int r = 0; r <= 16; r++)
                    {
                        for (float a = 0; a < Mathf.PI * 2f; a += 0.1f)
                        {
                            int gx = cx + (int)(Mathf.Cos(a) * r);
                            int gy = cy + (int)(Mathf.Sin(a) * r);
                            if (r >= 10 && r <= 14)
                            {
                                DrawThickPixel(colors, size, gx, gy, 1, iconCol);
                            }
                        }
                    }
                    break;
            }

            tex.SetPixels(colors);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        private static void DrawThickPixel(Color[] colors, int size, int x, int y, int radius, Color color)
        {
            for (int dy = -radius; dy <= radius; dy++)
            {
                int py = y + dy;
                if (py < 0 || py >= size) continue;
                for (int dx = -radius; dx <= radius; dx++)
                {
                    int px = x + dx;
                    if (px < 0 || px >= size) continue;
                    colors[py * size + px] = color;
                }
            }
        }
    }
}
