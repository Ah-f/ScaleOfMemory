using UnityEngine;

public static class PixelArtGenerator
{
    const int SIZE = 32;
    static readonly Color T = Color.clear; // transparent

    public static Sprite GenerateSprite(EnemyData data)
    {
        Texture2D tex;
        switch (data.enemyName)
        {
            case "Goblin": tex = DrawGoblin(data.bodyColor); break;
            case "Slime": tex = DrawSlime(data.bodyColor); break;
            case "Mini Slime": tex = DrawMiniSlime(data.bodyColor); break;
            case "Bat": tex = DrawBat(data.bodyColor); break;
            case "Orc": tex = DrawOrc(data.bodyColor); break;
            case "Skeleton": tex = DrawSkeleton(data.bodyColor); break;
            default: tex = DrawSlime(data.bodyColor); break;
        }

        tex.filterMode = FilterMode.Point;
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 16f);
    }

    static Texture2D CreateTex(int w = SIZE, int h = SIZE)
    {
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        var clear = new Color[w * h];
        for (int i = 0; i < clear.Length; i++) clear[i] = Color.clear;
        tex.SetPixels(clear);
        return tex;
    }

    static void P(Texture2D tex, int x, int y, Color c)
    {
        if (x >= 0 && x < tex.width && y >= 0 && y < tex.height)
            tex.SetPixel(x, y, c);
    }

    // Mirror-draw (symmetric on X)
    static void PM(Texture2D tex, int x, int y, Color c, int cx = 15)
    {
        P(tex, cx - x, y, c);
        P(tex, cx + x + 1, y, c);
    }

    static void FillRect(Texture2D tex, int x, int y, int w, int h, Color c)
    {
        for (int i = x; i < x + w; i++)
            for (int j = y; j < y + h; j++)
                P(tex, i, j, c);
    }

    static void FillMirrorRect(Texture2D tex, int ox, int y, int w, int h, Color c, int cx = 15)
    {
        for (int i = 0; i < w; i++)
            for (int j = 0; j < h; j++)
            {
                PM(tex, ox + i, y + j, c, cx);
            }
    }

    static Color Darker(Color c, float f = 0.6f)
    {
        return new Color(c.r * f, c.g * f, c.b * f, c.a);
    }

    static Color Lighter(Color c, float f = 1.4f)
    {
        return new Color(Mathf.Min(1, c.r * f), Mathf.Min(1, c.g * f), Mathf.Min(1, c.b * f), c.a);
    }

    // ===== GOBLIN =====
    static Texture2D DrawGoblin(Color body)
    {
        var tex = CreateTex();
        Color dark = Darker(body);
        Color ear = Lighter(body);
        Color eye = Color.white;
        Color pupil = Color.red;
        Color mouth = new Color(0.3f, 0.1f, 0.1f);

        // Body (torso)
        FillMirrorRect(tex, 0, 4, 4, 10, body);
        // Belly highlight
        FillMirrorRect(tex, 0, 6, 3, 6, Lighter(body, 1.15f));

        // Head
        FillMirrorRect(tex, 0, 14, 5, 7, body);
        // Head top
        FillMirrorRect(tex, 1, 21, 4, 2, body);

        // Ears (pointy)
        PM(tex, 5, 18, ear); PM(tex, 6, 19, ear); PM(tex, 7, 20, ear);
        PM(tex, 5, 19, ear); PM(tex, 6, 20, ear);

        // Eyes
        PM(tex, 2, 18, eye); PM(tex, 2, 19, eye);
        PM(tex, 3, 18, eye); PM(tex, 3, 19, eye);
        PM(tex, 2, 18, pupil); PM(tex, 3, 18, pupil);

        // Mouth
        PM(tex, 0, 15, mouth); PM(tex, 1, 15, mouth);
        PM(tex, 2, 15, mouth);

        // Arms
        P(tex, 9, 10, dark); P(tex, 10, 9, dark); P(tex, 10, 8, dark);
        P(tex, 22, 10, dark); P(tex, 21, 9, dark); P(tex, 21, 8, dark);

        // Legs
        FillRect(tex, 11, 0, 3, 4, dark);
        FillRect(tex, 18, 0, 3, 4, dark);
        // Feet
        FillRect(tex, 10, 0, 4, 1, dark);
        FillRect(tex, 18, 0, 4, 1, dark);

        return tex;
    }

    // ===== SLIME =====
    static Texture2D DrawSlime(Color body)
    {
        var tex = CreateTex();
        Color dark = Darker(body);
        Color light = Lighter(body);
        Color eye = Color.white;
        Color pupil = new Color(0.1f, 0.1f, 0.3f);

        // Body - blob shape
        FillMirrorRect(tex, 0, 2, 6, 8, body);
        FillMirrorRect(tex, 1, 10, 5, 4, body);
        FillMirrorRect(tex, 2, 14, 4, 3, body);
        FillMirrorRect(tex, 3, 17, 3, 2, body);

        // Bottom flat
        FillMirrorRect(tex, 1, 1, 7, 1, body);
        FillMirrorRect(tex, 3, 0, 5, 1, body);

        // Highlight (shine)
        PM(tex, 1, 13, light); PM(tex, 1, 14, light); PM(tex, 2, 15, light);

        // Shadow bottom
        FillMirrorRect(tex, 0, 2, 6, 2, dark);

        // Eyes
        PM(tex, 2, 10, eye); PM(tex, 2, 11, eye); PM(tex, 3, 10, eye); PM(tex, 3, 11, eye);
        PM(tex, 2, 10, pupil); PM(tex, 3, 10, pupil);

        // Mouth (smile)
        PM(tex, 0, 7, dark); PM(tex, 1, 6, dark); PM(tex, 2, 6, dark);

        return tex;
    }

    // ===== MINI SLIME =====
    static Texture2D DrawMiniSlime(Color body)
    {
        var tex = CreateTex(16, 16);
        Color dark = Darker(body);
        Color eye = Color.white;
        Color pupil = new Color(0.1f, 0.1f, 0.3f);

        // Small blob
        FillRect(tex, 4, 1, 8, 6, body);
        FillRect(tex, 5, 7, 6, 3, body);
        FillRect(tex, 6, 10, 4, 2, body);
        FillRect(tex, 3, 1, 1, 4, body);
        FillRect(tex, 12, 1, 1, 4, body);

        // Shadow
        FillRect(tex, 4, 1, 8, 2, dark);

        // Eyes
        P(tex, 6, 6, eye); P(tex, 9, 6, eye);
        P(tex, 6, 5, pupil); P(tex, 9, 5, pupil);

        tex.filterMode = FilterMode.Point;
        return tex;
    }

    // ===== BAT =====
    static Texture2D DrawBat(Color body)
    {
        var tex = CreateTex();
        Color dark = Darker(body);
        Color wing = Darker(body, 0.8f);
        Color eye = Color.red;

        // Body (small center)
        FillMirrorRect(tex, 0, 8, 3, 8, body);
        FillMirrorRect(tex, 0, 16, 2, 3, body);

        // Wings (wide)
        // Inner wing
        FillMirrorRect(tex, 3, 10, 3, 6, wing);
        // Mid wing
        FillMirrorRect(tex, 6, 11, 3, 5, wing);
        // Outer wing
        FillMirrorRect(tex, 9, 12, 3, 4, wing);
        // Wing tips
        FillMirrorRect(tex, 12, 13, 2, 3, wing);

        // Wing membrane details
        PM(tex, 5, 10, dark); PM(tex, 8, 11, dark); PM(tex, 11, 12, dark);

        // Eyes (red, glowing)
        PM(tex, 1, 15, eye); PM(tex, 2, 15, eye);
        PM(tex, 1, 16, Lighter(eye, 1.3f));

        // Ears
        PM(tex, 1, 19, body); PM(tex, 1, 20, dark);
        PM(tex, 3, 19, body); PM(tex, 3, 20, dark);

        // Fangs
        PM(tex, 0, 8, Color.white);
        PM(tex, 1, 8, Color.white);
        P(tex, 15, 8, Color.white);
        P(tex, 16, 8, Color.white);

        // Feet
        PM(tex, 0, 7, dark); PM(tex, 1, 7, dark);

        return tex;
    }

    // ===== ORC =====
    static Texture2D DrawOrc(Color body)
    {
        var tex = CreateTex();
        Color dark = Darker(body);
        Color armor = new Color(0.3f, 0.3f, 0.35f);
        Color eye = Color.yellow;
        Color tusk = Color.white;

        // Legs
        FillRect(tex, 10, 0, 4, 5, dark);
        FillRect(tex, 18, 0, 4, 5, dark);
        FillRect(tex, 9, 0, 5, 1, dark);
        FillRect(tex, 18, 0, 5, 1, dark);

        // Body (wide)
        FillMirrorRect(tex, 0, 5, 6, 12, body);
        // Armor plate
        FillMirrorRect(tex, 0, 8, 5, 6, armor);
        FillMirrorRect(tex, 0, 14, 3, 2, armor);

        // Head (big)
        FillMirrorRect(tex, 0, 17, 5, 7, body);
        FillMirrorRect(tex, 1, 24, 4, 2, body);

        // Eyes (angry)
        PM(tex, 2, 21, eye); PM(tex, 3, 21, eye);
        PM(tex, 2, 22, dark); PM(tex, 3, 22, dark); // brow

        // Tusks
        PM(tex, 0, 17, tusk); PM(tex, 1, 17, tusk);
        PM(tex, 0, 16, tusk);

        // Arms (thick)
        P(tex, 8, 13, body); P(tex, 9, 12, body); P(tex, 9, 11, body);
        P(tex, 10, 10, body); P(tex, 10, 9, dark);
        P(tex, 23, 13, body); P(tex, 22, 12, body); P(tex, 22, 11, body);
        P(tex, 21, 10, body); P(tex, 21, 9, dark);

        // Weapon (club)
        P(tex, 10, 7, dark); P(tex, 10, 6, dark); P(tex, 10, 5, dark);
        P(tex, 9, 5, dark); P(tex, 11, 5, dark);
        P(tex, 9, 4, dark); P(tex, 11, 4, dark);

        return tex;
    }

    // ===== SKELETON =====
    static Texture2D DrawSkeleton(Color body)
    {
        var tex = CreateTex();
        Color bone = new Color(0.9f, 0.87f, 0.78f);
        Color dark = Darker(bone, 0.7f);
        Color eye = new Color(0.2f, 1f, 0.2f); // green glowing
        Color shadow = new Color(0.15f, 0.1f, 0.1f);

        // Legs (thin bones)
        P(tex, 13, 0, bone); P(tex, 13, 1, bone); P(tex, 13, 2, bone);
        P(tex, 14, 3, bone); P(tex, 14, 4, bone); P(tex, 14, 5, bone);
        P(tex, 18, 0, bone); P(tex, 18, 1, bone); P(tex, 18, 2, bone);
        P(tex, 17, 3, bone); P(tex, 17, 4, bone); P(tex, 17, 5, bone);
        // Feet
        P(tex, 12, 0, bone); P(tex, 14, 0, bone);
        P(tex, 17, 0, bone); P(tex, 19, 0, bone);

        // Ribcage
        FillMirrorRect(tex, 0, 6, 2, 8, bone);
        // Ribs
        PM(tex, 2, 7, bone); PM(tex, 2, 9, bone); PM(tex, 2, 11, bone);
        // Spine
        P(tex, 15, 6, bone); P(tex, 16, 6, bone);
        P(tex, 15, 8, bone); P(tex, 16, 8, bone);
        P(tex, 15, 10, bone); P(tex, 16, 10, bone);
        P(tex, 15, 12, bone); P(tex, 16, 12, bone);

        // Head (skull)
        FillMirrorRect(tex, 0, 16, 5, 7, bone);
        FillMirrorRect(tex, 1, 23, 4, 2, bone);
        // Eye sockets
        PM(tex, 2, 20, shadow); PM(tex, 3, 20, shadow);
        PM(tex, 2, 21, shadow); PM(tex, 3, 21, shadow);
        // Glowing eyes
        PM(tex, 2, 20, eye);
        // Nose
        P(tex, 15, 19, shadow); P(tex, 16, 19, shadow);
        // Jaw
        FillMirrorRect(tex, 1, 15, 4, 1, dark);
        PM(tex, 0, 15, bone); PM(tex, 2, 15, bone); PM(tex, 4, 15, bone);

        // Arms (bone)
        P(tex, 9, 13, bone); P(tex, 8, 12, bone); P(tex, 7, 11, bone);
        P(tex, 7, 10, bone); P(tex, 7, 9, bone);
        P(tex, 22, 13, bone); P(tex, 23, 12, bone); P(tex, 24, 11, bone);
        P(tex, 24, 10, bone); P(tex, 24, 9, bone);

        // Sword in hand
        P(tex, 7, 7, dark); P(tex, 7, 6, dark); P(tex, 7, 5, Color.gray);
        P(tex, 7, 4, Color.gray); P(tex, 7, 3, Color.gray);
        P(tex, 6, 8, Color.gray); P(tex, 8, 8, Color.gray); // crossguard

        return tex;
    }

    // ===== PLAYER CHARACTER (BACK VIEW) =====
    public static Sprite GeneratePlayerSprite()
    {
        var tex = CreateTex();
        Color cloak = new Color32(60, 60, 140, 255);
        Color cloakLight = new Color32(80, 80, 170, 255);
        Color cloakDark = new Color32(40, 40, 100, 255);
        Color hair = new Color32(80, 50, 30, 255);
        Color boot = new Color32(60, 40, 30, 255);
        Color gold = new Color32(244, 162, 97, 255);
        Color staff = new Color32(140, 100, 60, 255);
        Color gem = new Color32(100, 220, 255, 255);
        Color skin = new Color32(230, 190, 150, 255);

        // Boots
        FillRect(tex, 11, 0, 4, 2, boot);
        FillRect(tex, 17, 0, 4, 2, boot);

        // Legs (cloak bottom, back folds)
        FillMirrorRect(tex, 0, 2, 5, 4, cloak);
        FillMirrorRect(tex, 2, 2, 3, 3, cloakDark); // fold shadows
        // Center fold line
        P(tex, 15, 3, cloakDark); P(tex, 16, 3, cloakDark);
        P(tex, 15, 4, cloakDark); P(tex, 16, 4, cloakDark);

        // Body (cloak back)
        FillMirrorRect(tex, 0, 6, 5, 8, cloak);
        // Cloak back folds/creases
        FillMirrorRect(tex, 1, 7, 1, 6, cloakDark);
        FillMirrorRect(tex, 3, 8, 1, 4, cloakLight);
        // Center back seam
        P(tex, 15, 7, cloakDark); P(tex, 16, 7, cloakDark);
        P(tex, 15, 9, cloakDark); P(tex, 16, 9, cloakDark);
        P(tex, 15, 11, cloakDark); P(tex, 16, 11, cloakDark);

        // Belt (seen from back)
        FillMirrorRect(tex, 0, 6, 5, 1, gold);

        // Shoulders (slightly wider)
        PM(tex, 5, 13, cloak); PM(tex, 5, 12, cloak);

        // Head (back of head - hair visible)
        FillMirrorRect(tex, 0, 15, 4, 6, hair);
        FillMirrorRect(tex, 1, 21, 3, 2, hair);
        // Hair highlights
        FillMirrorRect(tex, 1, 18, 1, 3, Lighter(hair, 1.3f));
        // Hair detail
        P(tex, 15, 20, Darker(hair, 0.7f)); P(tex, 16, 20, Darker(hair, 0.7f));
        P(tex, 15, 17, Darker(hair, 0.7f)); P(tex, 16, 17, Darker(hair, 0.7f));

        // Ears peeking from sides
        PM(tex, 4, 17, skin); PM(tex, 4, 18, skin);

        // Hood (draped back on shoulders)
        FillMirrorRect(tex, 1, 23, 5, 2, cloak);
        FillMirrorRect(tex, 2, 25, 4, 1, cloak);
        PM(tex, 5, 21, cloak); PM(tex, 5, 22, cloak);
        // Hood inner shadow
        FillMirrorRect(tex, 2, 23, 3, 1, cloakDark);

        // Neck
        P(tex, 15, 14, skin); P(tex, 16, 14, skin);

        // Staff (right hand, held to the side)
        P(tex, 8, 11, skin); P(tex, 8, 10, skin); // hand
        for (int y = 4; y < 28; y++)
            P(tex, 7, y, staff);
        // Staff gem (top)
        P(tex, 6, 27, gem); P(tex, 7, 28, gem); P(tex, 8, 27, gem);
        P(tex, 7, 29, gem);
        // Staff glow
        P(tex, 6, 28, new Color(gem.r, gem.g, gem.b, 0.5f));
        P(tex, 8, 28, new Color(gem.r, gem.g, gem.b, 0.5f));
        P(tex, 7, 30, new Color(gem.r, gem.g, gem.b, 0.3f));

        // Left arm (behind, holding book at side)
        P(tex, 23, 12, cloak); P(tex, 24, 11, cloak); P(tex, 24, 10, skin);
        // Book (partially visible from back)
        FillRect(tex, 24, 7, 3, 4, new Color32(140, 50, 50, 255));
        FillRect(tex, 25, 8, 1, 2, gold); // book clasp

        // Cape/cloak bottom flow
        PM(tex, 5, 5, cloak); PM(tex, 5, 4, cloakDark);

        tex.filterMode = FilterMode.Point;
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 16f);
    }
}
