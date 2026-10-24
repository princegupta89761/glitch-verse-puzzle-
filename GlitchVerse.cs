// GLITCHVERSE - Unity 2D, single file, zero setup.
// Drop into Assets/Scripts/, press Play in ANY scene. It builds camera, levels, player, UI by itself.
// Works with the old Input Manager, the new Input System, or both. Unity 2019 to Unity 6.
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

static class GVInput
{
#if ENABLE_LEGACY_INPUT_MANAGER
    public static bool Left { get { return Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow); } }
    public static bool Right { get { return Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow); } }
    public static bool JumpDown { get { return Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow); } }
    public static bool FlipDown { get { return Input.GetKeyDown(KeyCode.E); } }
    public static bool FreezeDown { get { return Input.GetKeyDown(KeyCode.Q); } }
    public static bool SwitchDown { get { return Input.GetKeyDown(KeyCode.R); } }
    public static bool RestartDown { get { return Input.GetKeyDown(KeyCode.Backspace); } }
    public static bool StartDown { get { return Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter); } }
#elif ENABLE_INPUT_SYSTEM
    static Keyboard K { get { return Keyboard.current; } }
    public static bool Left { get { return K != null && (K.aKey.isPressed || K.leftArrowKey.isPressed); } }
    public static bool Right { get { return K != null && (K.dKey.isPressed || K.rightArrowKey.isPressed); } }
    public static bool JumpDown { get { return K != null && (K.spaceKey.wasPressedThisFrame || K.wKey.wasPressedThisFrame || K.upArrowKey.wasPressedThisFrame); } }
    public static bool FlipDown { get { return K != null && K.eKey.wasPressedThisFrame; } }
    public static bool FreezeDown { get { return K != null && K.qKey.wasPressedThisFrame; } }
    public static bool SwitchDown { get { return K != null && K.rKey.wasPressedThisFrame; } }
    public static bool RestartDown { get { return K != null && K.backspaceKey.wasPressedThisFrame; } }
    public static bool StartDown { get { return K != null && (K.enterKey.wasPressedThisFrame || K.numpadEnterKey.wasPressedThisFrame); } }
#else
    public static bool Left { get { return false; } }
    public static bool Right { get { return false; } }
    public static bool JumpDown { get { return false; } }
    public static bool FlipDown { get { return false; } }
    public static bool FreezeDown { get { return false; } }
    public static bool SwitchDown { get { return false; } }
    public static bool RestartDown { get { return false; } }
    public static bool StartDown { get { return false; } }
#endif
}

// Kinematic platform that moves between two points and stops when time is frozen.
public class GVPlatform : MonoBehaviour
{
    public Vector2 a, b, delta;
    public float speed;
    Vector2 target;
    Rigidbody2D rb;

    public void Init(Vector2 A, Vector2 B, float s)
    {
        a = A; b = B; speed = s; target = B;
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        delta = Vector2.zero;
        if (!GlitchVerse.Moving || rb == null) return;
        Vector2 pos = rb.position;
        Vector2 np = Vector2.MoveTowards(pos, target, speed * Time.fixedDeltaTime);
        if ((np - target).sqrMagnitude < 0.0001f) target = ((target - b).sqrMagnitude < 0.0001f) ? a : b;
        delta = np - pos;
        rb.MovePosition(np);
    }
}

public class GlitchVerse : MonoBehaviour
{
    // ---------- tuning ----------
    const int COLS = 25, ROWS = 14;
    const float MOVE = 7f, JUMP = 14f, GRAV = 3f, PLAT_H = 0.45f;

    // ---------- level data ----------
    struct PD
    {
        public float w, ax, ay, bx, by, sp;
        public PD(float w, float ax, float ay, float bx, float by, float sp) { this.w = w; this.ax = ax; this.ay = ay; this.bx = bx; this.by = by; this.sp = sp; }
    }
    class Lv { public string name, hint; public float time; public string[] rows; public PD[] plats; }

    static string Rp(string s, int n) { var sb = new System.Text.StringBuilder(); for (int i = 0; i < n; i++) sb.Append(s); return sb.ToString(); }
    static void Rep(List<string> l, string s, int n) { for (int i = 0; i < n; i++) l.Add(s); }

    static Lv[] MakeLevels()
    {
        string D = Rp(".", 25), W = Rp("#", 25);

        var l1 = new List<string>();
        l1.Add(W); l1.Add(Rp(".", 9) + "ooooooo" + Rp(".", 9)); Rep(l1, D, 10);
        l1.Add("..P......^^^^^^^^^....E.."); l1.Add(W);

        var l2 = new List<string>();
        l2.Add(W); Rep(l2, D, 3); l2.Add(Rp(".", 23) + "E.");
        Rep(l2, Rp(".", 22) + "###", 5);
        l2.Add(Rp(".", 9) + "ooooo" + Rp(".", 8) + "###");
        l2.Add(Rp(".", 22) + "###");
        l2.Add("..P" + Rp(".", 19) + "###");
        l2.Add(Rp("#", 7) + Rp(".", 8) + Rp("#", 10));

        var l3 = new List<string>();
        l3.Add(W); Rep(l3, D, 4);
        Rep(l3, Rp(".", 7) + "A" + Rp(".", 13) + "B...", 5);
        l3.Add(Rp(".", 7) + "A" + Rp(".", 6) + "o.o" + Rp(".", 4) + "B...");
        l3.Add(Rp(".", 7) + "A" + Rp(".", 13) + "B...");
        l3.Add("..P....A" + Rp(".", 13) + "B.E.");
        l3.Add(Rp("#", 12) + Rp("B", 7) + Rp("#", 6));

        var l4 = new List<string>();
        l4.Add(W); l4.Add(".....oo.A.oo" + Rp(".", 13));
        Rep(l4, Rp(".", 8) + "A" + Rp(".", 16), 3);
        Rep(l4, Rp(".", 8) + "A" + Rp(".", 13) + "B..", 2);
        Rep(l4, Rp(".", 22) + "B..", 5);
        l4.Add(".P.." + Rp("^", 8) + Rp(".", 10) + "BE.");
        l4.Add(Rp("#", 14) + Rp(".", 7) + Rp("#", 4));

        return new Lv[]
        {
            new Lv { name = "1 - GRAVITY", time = 40, hint = "Press E to flip gravity and walk the ceiling over the spikes", rows = l1.ToArray(), plats = new PD[0] },
            new Lv { name = "2 - TIME", time = 60, hint = "Press Q to freeze the platforms while you board or step off", rows = l2.ToArray(),
                     plats = new PD[] { new PD(3, 7, 13, 12, 13, 2.5f), new PD(3, 18.625f, 13, 18.625f, 5, 2.2f) } },
            new Lv { name = "3 - DIMENSION", time = 60, hint = "Press R to switch dimension: cyan (A) and magenta (B) blocks swap", rows = l3.ToArray(), plats = new PD[0] },
            new Lv { name = "4 - GLITCHVERSE", time = 90, hint = "Use everything: flip, switch, freeze, switch back", rows = l4.ToArray(),
                     plats = new PD[] { new PD(3, 14, 13, 18, 13, 2.8f) } },
        };
    }

    // ---------- state ----------
    enum S { Menu, Play, Dead, Clear, End }
    static GlitchVerse inst;
    public static bool Moving { get { return inst != null && inst.state == S.Play && !inst.frozen; } }

    S state = S.Menu;
    Lv[] levels;
    int lvl, total, lscore, bonus;
    float tleft, time, shake, glitch, msgT, deadT, clearT, jumpBuf, ft, cd;
    bool frozen, grounded;
    int dim, gdir = 1;
    string msg = "";
    string[] grid;

    Camera cam;
    Sprite sqr, spk, dia, ring;
    GameObject levelRoot;
    Rigidbody2D pRb;
    BoxCollider2D pCol;
    Transform pVisual;
    List<Rect> spikeRects = new List<Rect>();
    List<Transform> shards = new List<Transform>();
    List<SpriteRenderer> platRenders = new List<SpriteRenderer>();
    Rect exitRect;
    Transform exitT;

    class DimTile { public bool isA; public SpriteRenderer sr; public Collider2D col; }
    List<DimTile> dimTiles = new List<DimTile>();

    GUIStyle st;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Boot()
    {
        if (inst == null) new GameObject("GlitchVerse").AddComponent<GlitchVerse>();
    }

    // ---------- helpers ----------
    Vector2 Vel
    {
        get
        {
#if UNITY_6000_0_OR_NEWER
            return pRb.linearVelocity;
#else
            return pRb.velocity;
#endif
        }
        set
        {
#if UNITY_6000_0_OR_NEWER
            pRb.linearVelocity = value;
#else
            pRb.velocity = value;
#endif
        }
    }

    static Sprite MakeSprite(System.Func<float, float, bool> f)
    {
        var tex = new Texture2D(32, 32, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        var px = new Color[32 * 32];
        for (int y = 0; y < 32; y++)
            for (int x = 0; x < 32; x++)
                px[y * 32 + x] = f((x + 0.5f) / 32f, (y + 0.5f) / 32f) ? Color.white : Color.clear;
        tex.SetPixels(px); tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32f);
    }

    SpriteRenderer SR(string n, Transform par, Sprite sp, Color c, int order, Vector2 pos, Vector2 scale)
    {
        var go = new GameObject(n);
        go.transform.SetParent(par, false);
        go.transform.localPosition = new Vector3(pos.x, pos.y, 0);
        go.transform.localScale = new Vector3(scale.x, scale.y, 1);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sp; sr.color = c; sr.sortingOrder = order;
        return sr;
    }

    // ---------- lifecycle ----------
    void Awake()
    {
        inst = this;
        levels = MakeLevels();
        sqr = MakeSprite((u, v) => true);
        spk = MakeSprite((u, v) => { float lu = (u * 2f) % 1f; float h = 1f - Mathf.Abs(lu * 2f - 1f); return v < 0.85f * h; });
        dia = MakeSprite((u, v) => Mathf.Abs(u - 0.5f) + Mathf.Abs(v - 0.5f) < 0.45f);
        ring = MakeSprite((u, v) => { float x = (u - 0.5f) / 0.5f, y = (v - 0.5f) / 0.5f; return x * x + y * y < 1f; });

        cam = Camera.main;
        if (cam == null)
        {
            var cg = new GameObject("Main Camera"); cg.tag = "MainCamera";
            cam = cg.AddComponent<Camera>();
            if (FindFirstAudio() == false) cg.AddComponent<AudioListener>();
        }
        cam.orthographic = true;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.05f, 0.07f, 0.19f);
    }

    bool FindFirstAudio() { return Object.FindObjectsOfType<AudioListener>().Length > 0; }

    void StartGame() { total = 0; lvl = 0; LoadLevel(0); }

    void Cleanup()
    {
        if (levelRoot != null) { levelRoot.SetActive(false); Destroy(levelRoot); }
    }

    void LoadLevel(int i)
    {
        Cleanup();
        var L = levels[i];
        grid = L.rows;
        levelRoot = new GameObject("Level");
        spikeRects.Clear(); shards.Clear(); dimTiles.Clear(); platRenders.Clear();
        Vector2 spawn = new Vector2(1.5f, 1.5f);

        for (int r = 0; r < ROWS; r++)
        {
            for (int c = 0; c < COLS; c++)
            {
                char ch = grid[r][c];
                Vector2 pos = new Vector2(c + 0.5f, ROWS - r - 0.5f);
                if (ch == '#')
                {
                    var sr = SR("Wall", levelRoot.transform, sqr, new Color(0.17f, 0.17f, 0.37f), 0, pos, Vector2.one);
                    sr.gameObject.AddComponent<BoxCollider2D>();
                }
                else if (ch == 'A' || ch == 'B')
                {
                    var sr = SR("Dim" + ch, levelRoot.transform, sqr, Color.white, 1, pos, Vector2.one);
                    var col = sr.gameObject.AddComponent<BoxCollider2D>();
                    dimTiles.Add(new DimTile { isA = ch == 'A', sr = sr, col = col });
                }
                else if (ch == '^')
                {
                    SR("Spike", levelRoot.transform, spk, new Color(1f, 0.29f, 0.29f), 2, pos, Vector2.one);
                    spikeRects.Add(new Rect(c + 0.15f, ROWS - r - 1f, 0.7f, 0.55f));
                }
                else if (ch == 'o')
                {
                    var sr = SR("Shard", levelRoot.transform, dia, new Color(1f, 0.88f, 0.29f), 3, pos, new Vector2(0.55f, 0.55f));
                    shards.Add(sr.transform);
                }
                else if (ch == 'E')
                {
                    var sr = SR("Exit", levelRoot.transform, ring, new Color(0.49f, 1f, 0.54f, 0.85f), 2, pos, new Vector2(0.7f, 0.95f));
                    SR("ExitCore", sr.transform, ring, new Color(0.05f, 0.07f, 0.19f, 0.9f), 3, Vector2.zero, new Vector2(0.65f, 0.65f));
                    exitT = sr.transform;
                    exitRect = new Rect(pos.x - 0.3f, pos.y - 0.45f, 0.6f, 0.9f);
                }
                else if (ch == 'P') spawn = pos;
            }
        }

        // invisible side walls
        for (int s = 0; s < 2; s++)
        {
            var w = new GameObject("Edge"); w.transform.SetParent(levelRoot.transform, false);
            w.transform.position = new Vector3(s == 0 ? -0.5f : COLS + 0.5f, ROWS / 2f, 0);
            var bc = w.AddComponent<BoxCollider2D>(); bc.size = new Vector2(1f, 60f);
        }

        // moving platforms
        foreach (var pd in L.plats)
        {
            Vector2 A = new Vector2(pd.ax + pd.w / 2f, ROWS - pd.ay - PLAT_H / 2f);
            Vector2 B = new Vector2(pd.bx + pd.w / 2f, ROWS - pd.by - PLAT_H / 2f);
            var sr = SR("Platform", levelRoot.transform, sqr, new Color(1f, 0.7f, 0.28f), 4, A, new Vector2(pd.w, PLAT_H));
            sr.gameObject.AddComponent<BoxCollider2D>();
            var rb = sr.gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            sr.gameObject.AddComponent<GVPlatform>().Init(A, B, pd.sp);
            platRenders.Add(sr);
        }

        // player
        var pg = new GameObject("Player"); pg.transform.SetParent(levelRoot.transform, false);
        pg.transform.position = spawn;
        pRb = pg.AddComponent<Rigidbody2D>();
        pRb.gravityScale = GRAV; pRb.freezeRotation = true;
        pRb.interpolation = RigidbodyInterpolation2D.Interpolate;
        pRb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        pCol = pg.AddComponent<BoxCollider2D>(); pCol.size = new Vector2(0.68f, 0.88f);
        pCol.sharedMaterial = new PhysicsMaterial2D("gv") { friction = 0f, bounciness = 0f };
        var vis = SR("Visual", pg.transform, sqr, new Color(0.1f, 0.88f, 1f), 10, Vector2.zero, new Vector2(0.68f, 0.88f));
        pVisual = vis.transform;
        SR("EyeL", pVisual, sqr, Color.white, 11, new Vector2(-0.2f, 0.2f), new Vector2(0.22f, 0.25f));
        SR("EyeR", pVisual, sqr, Color.white, 11, new Vector2(0.2f, 0.2f), new Vector2(0.22f, 0.25f));

        dim = 0; gdir = 1; frozen = false; ft = 0; cd = 0; lscore = 0; jumpBuf = 0; grounded = false;
        tleft = L.time; msg = L.hint; msgT = 4f; state = S.Play;
        ApplyDim();
    }

    // ---------- gameplay ----------
    void ApplyDim()
    {
        foreach (var t in dimTiles)
        {
            bool on = t.isA == (dim == 0);
            t.col.enabled = on;
            Color baseC = t.isA ? new Color(0.1f, 0.88f, 1f) : new Color(1f, 0.24f, 0.85f);
            baseC.a = on ? 0.9f : 0.12f;
            t.sr.color = baseC;
        }
        cam.backgroundColor = dim == 0 ? new Color(0.05f, 0.07f, 0.19f) : new Color(0.17f, 0.04f, 0.19f);
    }

    void Flip()
    {
        gdir = -gdir;
        pRb.gravityScale = GRAV * gdir;
        pVisual.localScale = new Vector3(0.68f, 0.88f * gdir, 1f);
        shake = 0.25f; glitch = 0.2f;
    }

    void TrySwitch()
    {
        int target = 1 - dim;
        char ch = target == 0 ? 'A' : 'B';
        Bounds b = pCol.bounds;
        int c0 = Mathf.FloorToInt(b.min.x), c1 = Mathf.FloorToInt(b.max.x - 0.001f);
        int rTop = ROWS - 1 - Mathf.FloorToInt(b.max.y - 0.001f), rBot = ROWS - 1 - Mathf.FloorToInt(b.min.y);
        for (int r = rTop; r <= rBot; r++)
            for (int c = c0; c <= c1; c++)
                if (r >= 0 && r < ROWS && c >= 0 && c < COLS && grid[r][c] == ch)
                {
                    msg = "BLOCKED: you would be stuck inside a wall"; msgT = 1.2f; return;
                }
        dim = target; ApplyDim(); glitch = 0.35f; shake = 0.15f;
    }

    void Die(string m)
    {
        if (state != S.Play) return;
        state = S.Dead; deadT = 1.2f; msg = m; msgT = 9f; shake = 0.4f; glitch = 0.5f;
        pRb.simulated = false; pVisual.gameObject.SetActive(false);
    }

    void Win()
    {
        if (state != S.Play) return;
        bonus = Mathf.CeilToInt(tleft) * 10;
        total += lscore + bonus;
        state = S.Clear; clearT = 1.8f;
        pRb.simulated = false;
    }

    static Rect BR(Bounds b) { return new Rect(b.min.x, b.min.y, b.size.x, b.size.y); }

    void CheckTriggers()
    {
        Rect pr = BR(pCol.bounds);
        Vector2 pp = pRb.position;
        if (pp.y < -3f || pp.y > ROWS + 3f) { Die("FELL INTO THE VOID"); return; }
        foreach (var s in spikeRects) if (pr.Overlaps(s)) { Die("GLITCHED BY SPIKES"); return; }
        for (int i = shards.Count - 1; i >= 0; i--)
        {
            Vector2 sp = shards[i].position;
            if (pr.Overlaps(new Rect(sp.x - 0.25f, sp.y - 0.3f, 0.5f, 0.6f)))
            {
                Destroy(shards[i].gameObject); shards.RemoveAt(i); lscore += 100;
            }
        }
        if (pr.Overlaps(exitRect)) Win();
    }

    void PlayUpdate()
    {
        float dt = Time.deltaTime;
        tleft -= dt;
        if (tleft <= 0f) { tleft = 0f; Die("TIME UP"); return; }
        cd -= dt;
        if (frozen) { ft -= dt; if (ft <= 0f) { frozen = false; cd = 5f; } }
        if (GVInput.JumpDown) jumpBuf = 0.15f;
        if (GVInput.RestartDown) { LoadLevel(lvl); return; }
        if (GVInput.FlipDown) Flip();
        if (GVInput.FreezeDown && cd <= 0f && !frozen) { frozen = true; ft = 3f; glitch = 0.2f; }
        if (GVInput.SwitchDown) TrySwitch();
        CheckTriggers();
    }

    void FixedUpdate()
    {
        if (state != S.Play || pRb == null) return;
        float dir = (GVInput.Right ? 1f : 0f) - (GVInput.Left ? 1f : 0f);
        Vector2 v = Vel;
        v.x = dir * MOVE;

        grounded = false; GVPlatform plat = null;
        Bounds b = pCol.bounds;
        var hits = Physics2D.BoxCastAll(b.center, new Vector2(b.size.x * 0.9f, b.size.y), 0f, new Vector2(0f, -gdir), 0.08f);
        foreach (var h in hits)
        {
            if (h.collider == pCol || h.collider.isTrigger) continue;
            grounded = true;
            var gp = h.collider.GetComponent<GVPlatform>();
            if (gp != null) plat = gp;
        }
        if (v.y * gdir > 0.1f) grounded = false;

        if (jumpBuf > 0f && grounded) { v.y = -gdir * JUMP; jumpBuf = 0f; grounded = false; }
        jumpBuf -= Time.fixedDeltaTime;
        Vel = v;
        if (grounded && plat != null) pRb.position += new Vector2(plat.delta.x, 0f);
    }

    void Update()
    {
        float dt = Time.deltaTime;
        time += dt; shake = Mathf.Max(0f, shake - dt); glitch = Mathf.Max(0f, glitch - dt); msgT -= dt;
        cam.orthographicSize = Mathf.Max(7f, 12.5f / Mathf.Max(0.1f, cam.aspect));
        Vector3 basePos = new Vector3(12.5f, 7f, -10f);
        cam.transform.position = basePos + (shake > 0f ? (Vector3)(Random.insideUnitCircle * shake * 0.5f) : Vector3.zero);

        switch (state)
        {
            case S.Menu:
            case S.End:
                if (GVInput.StartDown) StartGame();
                break;
            case S.Play: PlayUpdate(); break;
            case S.Dead:
                deadT -= dt;
                if (deadT <= 0f) LoadLevel(lvl);
                break;
            case S.Clear:
                clearT -= dt;
                if (clearT <= 0f)
                {
                    lvl++;
                    if (lvl >= levels.Length) { state = S.End; Cleanup(); }
                    else LoadLevel(lvl);
                }
                break;
        }

        // animation
        for (int i = 0; i < shards.Count; i++)
        {
            if (shards[i] == null) continue;
            shards[i].localScale = new Vector3(0.55f, 0.55f + Mathf.Sin(time * 5f + i) * 0.08f, 1f);
        }
        if (exitT != null) exitT.localScale = new Vector3(0.7f, 0.95f, 1f) * (1f + Mathf.Sin(time * 4f) * 0.06f);
        foreach (var pr in platRenders)
            if (pr != null) pr.color = frozen ? new Color(0.56f, 0.85f, 1f) : new Color(1f, 0.7f, 0.28f);
    }

    // ---------- UI (IMGUI: no Canvas / EventSystem needed) ----------
    void Lbl(Rect r, string t, int size, Color c, TextAnchor a)
    {
        st.fontSize = Mathf.RoundToInt(size * Screen.height / 540f);
        st.alignment = a;
        st.normal.textColor = new Color(0, 0, 0, 0.6f);
        GUI.Label(new Rect(r.x + 1.5f, r.y + 1.5f, r.width, r.height), t, st);
        st.normal.textColor = c;
        GUI.Label(r, t, st);
    }

    void OnGUI()
    {
        if (st == null) { st = new GUIStyle(GUI.skin.label); st.fontStyle = FontStyle.Bold; st.wordWrap = false; }
        float W = Screen.width, H = Screen.height, s = H / 540f;
        Color cy = new Color(0.1f, 0.88f, 1f), mg = new Color(1f, 0.24f, 0.85f), yl = new Color(1f, 0.88f, 0.29f), gr = new Color(0.49f, 1f, 0.54f);

        if (state == S.Menu || state == S.End)
        {
            GUI.color = new Color(0.04f, 0.05f, 0.14f, 1f); GUI.DrawTexture(new Rect(0, 0, W, H), Texture2D.whiteTexture); GUI.color = Color.white;
            Lbl(new Rect(0, H * 0.18f, W, 80 * s), "GLITCHVERSE", 64, cy, TextAnchor.MiddleCenter);
            Lbl(new Rect(0, H * 0.18f + 70 * s, W, 40 * s), "The World Is Broken", 24, mg, TextAnchor.MiddleCenter);
            if (state == S.End)
            {
                Lbl(new Rect(0, H * 0.48f, W, 40 * s), "ALL LEVELS CLEARED!", 30, gr, TextAnchor.MiddleCenter);
                Lbl(new Rect(0, H * 0.48f + 40 * s, W, 40 * s), "Final score: " + total, 28, yl, TextAnchor.MiddleCenter);
            }
            else Lbl(new Rect(0, H * 0.48f, W, 40 * s), "Flip gravity  -  Freeze time  -  Switch dimensions", 20, new Color(0.8f, 0.82f, 1f), TextAnchor.MiddleCenter);
            if (GUI.Button(new Rect(W / 2 - 110 * s, H * 0.68f, 220 * s, 50 * s), state == S.End ? "PLAY AGAIN (Enter)" : "START (Enter)")) StartGame();
            Lbl(new Rect(0, H - 40 * s, W, 30 * s), "A/D move | Space jump | E gravity | Q freeze | R dimension | Backspace restart", 15, Color.white, TextAnchor.MiddleCenter);
            return;
        }

        // HUD bar
        GUI.color = new Color(0, 0, 0, 0.55f); GUI.DrawTexture(new Rect(0, 0, W, 30 * s), Texture2D.whiteTexture); GUI.color = Color.white;
        Lbl(new Rect(10 * s, 0, 300 * s, 30 * s), "LEVEL " + levels[lvl].name, 16, Color.white, TextAnchor.MiddleLeft);
        Lbl(new Rect(W / 2 - 100 * s, 0, 200 * s, 30 * s), "SCORE " + (total + lscore), 16, yl, TextAnchor.MiddleCenter);
        Lbl(new Rect(W - 130 * s, 0, 120 * s, 30 * s), "TIME " + Mathf.CeilToInt(tleft), 16, tleft < 10f ? new Color(1f, 0.3f, 0.3f) : Color.white, TextAnchor.MiddleRight);
        Lbl(new Rect(W / 2 + 110 * s, 0, 200 * s, 30 * s), "DIM " + (dim == 0 ? "A" : "B"), 15, dim == 0 ? cy : mg, TextAnchor.MiddleLeft);
        string fs = frozen ? "FREEZE " + ft.ToString("0.0") + "s" : (cd > 0f ? "Freeze " + Mathf.CeilToInt(cd) + "s" : "Freeze READY");
        Lbl(new Rect(W / 2 - 330 * s, 0, 200 * s, 30 * s), fs, 15, frozen ? new Color(0.56f, 0.85f, 1f) : (cd > 0f ? Color.gray : gr), TextAnchor.MiddleRight);

        if (msgT > 0f && state == S.Play) Lbl(new Rect(0, 40 * s, W, 30 * s), msg, 18, Color.white, TextAnchor.MiddleCenter);
        if (frozen) { GUI.color = new Color(0.47f, 0.78f, 1f, 0.12f); GUI.DrawTexture(new Rect(0, 0, W, H), Texture2D.whiteTexture); GUI.color = Color.white; }

        if (glitch > 0f)
            for (int i = 0; i < 7; i++)
            {
                GUI.color = new Color(Random.value > 0.5f ? 0.1f : 1f, Random.value > 0.5f ? 0.88f : 0.24f, 1f, 0.28f);
                GUI.DrawTexture(new Rect(Random.Range(-20f, 20f) * glitch * 3f, Random.value * H, W, Random.Range(3f, 18f)), Texture2D.whiteTexture);
            }
        GUI.color = Color.white;

        if (state == S.Dead)
        {
            GUI.color = new Color(0, 0, 0, 0.5f); GUI.DrawTexture(new Rect(0, H * 0.38f, W, 110 * s), Texture2D.whiteTexture); GUI.color = Color.white;
            Lbl(new Rect(0, H * 0.38f + 10 * s, W, 50 * s), msg, 34, new Color(1f, 0.3f, 0.3f), TextAnchor.MiddleCenter);
            Lbl(new Rect(0, H * 0.38f + 60 * s, W, 30 * s), "Restarting level...", 16, Color.white, TextAnchor.MiddleCenter);
        }
        if (state == S.Clear)
        {
            GUI.color = new Color(0, 0, 0, 0.6f); GUI.DrawTexture(new Rect(0, H * 0.33f, W, 130 * s), Texture2D.whiteTexture); GUI.color = Color.white;
            Lbl(new Rect(0, H * 0.33f + 10 * s, W, 60 * s), "LEVEL CLEAR!", 40, gr, TextAnchor.MiddleCenter);
            Lbl(new Rect(0, H * 0.33f + 75 * s, W, 40 * s), "Time bonus +" + bonus + "    Shards +" + lscore, 20, yl, TextAnchor.MiddleCenter);
        }
    }
}
