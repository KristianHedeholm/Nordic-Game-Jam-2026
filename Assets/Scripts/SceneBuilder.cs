using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using TMPro;

/// <summary>
/// Builds the full game scene at runtime. No prefabs needed — just attach to an empty GameObject and hit Play.
/// </summary>
[DefaultExecutionOrder(-100)]
public class SceneBuilder : MonoBehaviour
{
    void Awake()
    {
        SetupCamera();
        SetupEventSystem();
        BuildScene();
    }

    void SetupCamera()
    {
        var cam = Camera.main;
        if (cam == null) return;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.05f, 0.03f, 0.08f);
        cam.orthographic = true;
    }

    void SetupEventSystem()
    {
        if (FindFirstObjectByType<EventSystem>() != null) return;
        var es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
        es.AddComponent<InputSystemUIInputModule>();
    }

    void BuildScene()
    {
        // ── CANVAS ───────────────────────────────────────────────────────────
        var canvasGO = new GameObject("Canvas");
        var canvas   = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        // ── STAGE PANEL (always visible background) ───────────────────────
        var stagePanelGO = CreateRectFull(canvasGO, "StageBackground");
        AddImage(stagePanelGO, new Color(0.06f, 0.03f, 0.10f));

        // Floor / stage
        var floor = CreateRect(stagePanelGO, "Floor", new Vector2(0, -350), new Vector2(1920, 220));
        AddImage(floor, new Color(0.25f, 0.15f, 0.05f));

        // Floor highlight
        var floorLine = CreateRect(stagePanelGO, "FloorLine", new Vector2(0, -245), new Vector2(1920, 12));
        AddImage(floorLine, new Color(0.55f, 0.38f, 0.10f));

        // ── STOOL (king stands on it to appear taller) ───────────────────
        var stool = CreateRect(stagePanelGO, "Stool", new Vector2(0, -270), new Vector2(160, 60));
        AddImage(stool, new Color(0.35f, 0.2f, 0.05f));
        var stoolTop = CreateRect(stagePanelGO, "StoolTop", new Vector2(0, -245), new Vector2(180, 18));
        AddImage(stoolTop, new Color(0.5f, 0.3f, 0.08f));

        // ── KING BODY (short & fat) ───────────────────────────────────────
        var skin = new Color(0.95f, 0.75f, 0.55f);

        // Chubby body
        var body = CreateRect(stagePanelGO, "KingBody", new Vector2(0, -110), new Vector2(220, 200));
        AddImage(body, skin);

        // Big fat belly highlight
        var belly = CreateRect(stagePanelGO, "Belly", new Vector2(0, -130), new Vector2(200, 160));
        AddImage(belly, new Color(1f, 0.8f, 0.62f));

        // Short stubby legs
        var legL = CreateRect(stagePanelGO, "LegL", new Vector2(-55, -248), new Vector2(80, 60));
        AddImage(legL, skin);
        var legR = CreateRect(stagePanelGO, "LegR", new Vector2(55, -248), new Vector2(80, 60));
        AddImage(legR, skin);

        // Wide arms (posed proudly)
        var armL = CreateRect(stagePanelGO, "ArmL", new Vector2(-165, -80), new Vector2(110, 80));
        AddImage(armL, skin);
        var armR = CreateRect(stagePanelGO, "ArmR", new Vector2(165, -80), new Vector2(110, 80));
        AddImage(armR, skin);

        // Big round head
        var head = CreateRect(stagePanelGO, "KingHead", new Vector2(0, 60), new Vector2(200, 190));
        AddImage(head, skin);

        // Eyes (beady and smug)
        var eyeL = CreateRect(stagePanelGO, "EyeL", new Vector2(-45, 70), new Vector2(35, 35));
        AddImage(eyeL, new Color(0.1f, 0.1f, 0.3f));
        var eyeR = CreateRect(stagePanelGO, "EyeR", new Vector2(45, 70), new Vector2(35, 35));
        AddImage(eyeR, new Color(0.1f, 0.1f, 0.3f));

        // Eye shine
        var shineL = CreateRect(eyeL, "ShineL", new Vector2(8, 8), new Vector2(10, 10));
        AddImage(shineL, Color.white);
        var shineR = CreateRect(eyeR, "ShineR", new Vector2(8, 8), new Vector2(10, 10));
        AddImage(shineR, Color.white);

        // Smug smile
        var smile = CreateRect(stagePanelGO, "Smile", new Vector2(0, 15), new Vector2(80, 18));
        AddImage(smile, new Color(0.6f, 0.25f, 0.15f));

        // Moustache
        var moustache = CreateRect(stagePanelGO, "Moustache", new Vector2(0, 35), new Vector2(100, 20));
        AddImage(moustache, new Color(0.3f, 0.15f, 0.05f));

        // Blush cheeks
        var blushL = CreateRect(stagePanelGO, "BlushL", new Vector2(-70, 40), new Vector2(45, 25));
        AddImage(blushL, new Color(1f, 0.5f, 0.5f, 0.55f));
        var blushR = CreateRect(stagePanelGO, "BlushR", new Vector2(70, 40), new Vector2(45, 25));
        AddImage(blushR, new Color(1f, 0.5f, 0.5f, 0.55f));

        // ── CROWN — above the curtains so always visible ──────────────────
        // Crown base (above curtain top)
        var crownBase = CreateRect(stagePanelGO, "CrownBase", new Vector2(0, 180), new Vector2(200, 35));
        AddImage(crownBase, new Color(1f, 0.85f, 0.1f));
        var crownL = CreateRect(stagePanelGO, "CrownL", new Vector2(-65, 210), new Vector2(45, 60));
        AddImage(crownL, new Color(1f, 0.85f, 0.1f));
        var crownC = CreateRect(stagePanelGO, "CrownC", new Vector2(0, 225), new Vector2(45, 75));
        AddImage(crownC, new Color(1f, 0.85f, 0.1f));
        var crownR = CreateRect(stagePanelGO, "CrownR", new Vector2(65, 210), new Vector2(45, 60));
        AddImage(crownR, new Color(1f, 0.85f, 0.1f));
        // Jewels on crown
        var jewel1 = CreateRect(crownL, "Jewel", Vector2.zero, new Vector2(20, 20));
        AddImage(jewel1, new Color(0.8f, 0.1f, 0.1f));
        var jewel2 = CreateRect(crownC, "Jewel", Vector2.zero, new Vector2(22, 22));
        AddImage(jewel2, new Color(0.1f, 0.4f, 0.9f));
        var jewel3 = CreateRect(crownR, "Jewel", Vector2.zero, new Vector2(20, 20));
        AddImage(jewel3, new Color(0.1f, 0.8f, 0.2f));

        // ── CURTAINS (cover the king during guessing) ─────────────────────
        var curtainL = CreateRect(stagePanelGO, "CurtainLeft", new Vector2(-560, 0), new Vector2(1000, 900));
        AddImage(curtainL, new Color(0.55f, 0.05f, 0.08f));
        // Curtain fold lines
        for (int i = 0; i < 4; i++)
        {
            var fold = CreateRect(curtainL, $"FoldL{i}", new Vector2(-350 + i * 120, 0), new Vector2(20, 900));
            AddImage(fold, new Color(0.4f, 0.03f, 0.06f, 0.7f));
        }
        var curtainGoldL = CreateRect(curtainL, "GoldTrimL", new Vector2(480, 0), new Vector2(18, 900));
        AddImage(curtainGoldL, new Color(1f, 0.82f, 0.1f));

        var curtainR = CreateRect(stagePanelGO, "CurtainRight", new Vector2(560, 0), new Vector2(1000, 900));
        AddImage(curtainR, new Color(0.55f, 0.05f, 0.08f));
        for (int i = 0; i < 4; i++)
        {
            var fold = CreateRect(curtainR, $"FoldR{i}", new Vector2(350 - i * 120, 0), new Vector2(20, 900));
            AddImage(fold, new Color(0.4f, 0.03f, 0.06f, 0.7f));
        }
        var curtainGoldR = CreateRect(curtainR, "GoldTrimR", new Vector2(-480, 0), new Vector2(18, 900));
        AddImage(curtainGoldR, new Color(1f, 0.82f, 0.1f));

        // Store curtain refs for reveal animation
        var curtainAnim = stagePanelGO.AddComponent<CurtainAnimator>();
        curtainAnim.curtainLeft  = curtainL.GetComponent<RectTransform>();
        curtainAnim.curtainRight = curtainR.GetComponent<RectTransform>();

        // ── ANSWER TRACKER (left side panel) ─────────────────────────────
        var trackerBg = CreateRect(stagePanelGO, "TrackerBg", new Vector2(-780, 200), new Vector2(280, 220));
        AddImage(trackerBg, new Color(0.08f, 0.05f, 0.15f, 0.9f));

        // Tracker title
        var trackerTitle = CreateRect(trackerBg, "TrackerTitle", new Vector2(0, 78), new Vector2(260, 40));
        var ttmp = trackerTitle.AddComponent<TextMeshProUGUI>();
        ttmp.text = "YOUR GUESSES";
        ttmp.fontSize = 18;
        ttmp.fontStyle = FontStyles.Bold;
        ttmp.alignment = TextAlignmentOptions.Center;
        ttmp.color = new Color(1f, 0.85f, 0.3f);

        // Tracker rows
        var tClothing = CreateRect(trackerBg, "TrackerClothing", new Vector2(0, 30), new Vector2(250, 40));
        var tcTmp = tClothing.AddComponent<TextMeshProUGUI>();
        tcTmp.text = "Garment: ?"; tcTmp.fontSize = 22; tcTmp.color = Color.white;
        tcTmp.alignment = TextAlignmentOptions.Center;

        var tColor = CreateRect(trackerBg, "TrackerColor", new Vector2(0, -15), new Vector2(250, 40));
        var tcoTmp = tColor.AddComponent<TextMeshProUGUI>();
        tcoTmp.text = "Color: ?"; tcoTmp.fontSize = 22; tcoTmp.color = Color.white;
        tcoTmp.alignment = TextAlignmentOptions.Center;

        var tMaterial = CreateRect(trackerBg, "TrackerMaterial", new Vector2(0, -60), new Vector2(250, 40));
        var tmTmp = tMaterial.AddComponent<TextMeshProUGUI>();
        tmTmp.text = "Material: ?"; tmTmp.fontSize = 22; tmTmp.color = Color.white;
        tmTmp.alignment = TextAlignmentOptions.Center;

        // ── SPEECH BUBBLE ─────────────────────────────────────────────────
        var bubblePanel = CreateRect(stagePanelGO, "SpeechBubble", new Vector2(420, 240), new Vector2(680, 220));
        AddImage(bubblePanel, new Color(0.97f, 0.95f, 0.88f));

        // Bubble tail
        var tail = CreateRect(bubblePanel, "Tail", new Vector2(-280, -80), new Vector2(60, 60));
        AddImage(tail, new Color(0.97f, 0.95f, 0.88f));

        var riddleText = bubblePanel.AddComponent<TextMeshProUGUI>() == null
            ? bubblePanel.GetComponent<TextMeshProUGUI>()
            : bubblePanel.GetComponent<TextMeshProUGUI>();
        // Add riddle text inside bubble
        var riddleGO = new GameObject("RiddleText");
        riddleGO.transform.SetParent(bubblePanel.transform, false);
        var riddleRT = riddleGO.AddComponent<RectTransform>();
        riddleRT.anchorMin = Vector2.zero; riddleRT.anchorMax = Vector2.one;
        riddleRT.offsetMin = new Vector2(20, 20); riddleRT.offsetMax = new Vector2(-20, -20);
        var riddleTMP = riddleGO.AddComponent<TextMeshProUGUI>();
        riddleTMP.text = "...";
        riddleTMP.fontSize = 26;
        riddleTMP.fontStyle = FontStyles.Italic;
        riddleTMP.color = new Color(0.1f, 0.05f, 0.15f);
        riddleTMP.alignment = TextAlignmentOptions.Center;
        riddleTMP.enableWordWrapping = true;

        // ── CATEGORY LABEL ────────────────────────────────────────────────
        var catGO = CreateRect(stagePanelGO, "CategoryLabel", new Vector2(0, 480), new Vector2(900, 70));
        var catTMP = catGO.AddComponent<TextMeshProUGUI>();
        catTMP.text = "";
        catTMP.fontSize = 38;
        catTMP.fontStyle = FontStyles.Bold;
        catTMP.alignment = TextAlignmentOptions.Center;
        catTMP.color = new Color(1f, 0.85f, 0.3f);

        // ── BOTTOM BUTTONS PANEL ──────────────────────────────────────────
        var bottomPanel = CreateRect(canvasGO, "BottomPanel", new Vector2(0, -460), new Vector2(1800, 110));
        var hlg = bottomPanel.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 16;
        hlg.childForceExpandWidth  = true;
        hlg.childForceExpandHeight = true;
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.padding = new RectOffset(10, 10, 5, 5);

        // ── OVERLAY PANELS ─────────────────────────────────────────────────
        var introPanel    = BuildIntroPanel(canvasGO);
        var loadingPanel  = BuildLoadingPanel(canvasGO);
        var reactionPanel = BuildReactionPanel(canvasGO);
        var revealPanel   = BuildRevealPanel(canvasGO);
        var judgmentPanel = BuildJudgmentPanel(canvasGO);
        var winPanel      = BuildWinPanel(canvasGO);
        var deathPanel    = BuildDeathPanel(canvasGO);

        // ── WIRE UP ───────────────────────────────────────────────────────
        var mgrGO = new GameObject("GameManager");
        var gm = mgrGO.AddComponent<GameManager>();
        var rg = mgrGO.AddComponent<RiddleGenerator>();
        var ui = mgrGO.AddComponent<UIManager>();

        rg.apiKey = "";
        rg.model  = "openai/gpt-4o-mini";

        // Reaction panel
        ui.reactionPanel = reactionPanel.root;
        ui.reactionText  = reactionPanel.text;
        ui.reactionBg    = reactionPanel.root.GetComponent<Image>();

        // Tracker
        ui.trackerClothing = tcTmp;
        ui.trackerColor    = tcoTmp;
        ui.trackerMaterial = tmTmp;

        // Stage UI refs
        ui.stagePanel       = stagePanelGO;
        ui.curtainAnimator  = curtainAnim;
        ui.riddleText       = riddleTMP;
        ui.categoryLabel    = catTMP;
        ui.optionsContainer = bottomPanel.transform;
        ui.optionButtonPrefab = BuildOptionButtonPrefab();

        // Overlay panels
        ui.introPanel         = introPanel.root;
        ui.loadingPanel       = loadingPanel.root;
        ui.revealPanel        = revealPanel.root;
        ui.finalJudgmentPanel = judgmentPanel.root;
        ui.winPanel           = winPanel.root;
        ui.deathPanel         = deathPanel.root;

        ui.introText        = introPanel.text;
        ui.introStartButton = introPanel.btn;
        ui.revealText            = revealPanel.text;
        ui.revealContinueButton  = revealPanel.btn;
        ui.finalText      = judgmentPanel.text;
        ui.flatterButton  = judgmentPanel.flatterBtn;
        ui.truthButton    = judgmentPanel.truthBtn;
        ui.winText            = winPanel.text;
        ui.winPlayAgainButton = winPanel.btn;
        ui.deathText            = deathPanel.text;
        ui.deathPlayAgainButton = deathPanel.btn;

        gm.uiManager       = ui;
        gm.riddleGenerator = rg;

        // Hide overlays, show intro
        introPanel.root.SetActive(false);
        loadingPanel.root.SetActive(false);
        reactionPanel.root.SetActive(false);
        revealPanel.root.SetActive(false);
        judgmentPanel.root.SetActive(false);
        winPanel.root.SetActive(false);
        deathPanel.root.SetActive(false);

        gm.StartGame();
    }

    // ── OVERLAY PANEL BUILDERS ────────────────────────────────────────────

    struct SimplePanel { public GameObject root; public TMP_Text text; public Button btn; }
    struct JudgPanel   { public GameObject root; public TMP_Text text; public Button flatterBtn; public Button truthBtn; }

    SimplePanel BuildIntroPanel(GameObject parent)
    {
        var root = CreateRectFull(parent, "IntroPanel");
        AddImage(root, new Color(0.06f, 0.03f, 0.10f, 0.97f));

        var title = AddTMP(root, "Title", "THE KING'S\nNEW CLOTHES", 80, FontStyles.Bold, new Vector2(0, 150), new Vector2(1400, 250));
        title.color = new Color(1f, 0.85f, 0.3f);

        var body = AddTMP(root, "Body",
            "The King demands your presence!\n\nHe has dressed in the finest garments in all the land...\n<i>or so he believes.</i>\n\nGuess his outfit. Answer wisely.\nYour head depends on it.",
            30, FontStyles.Normal, new Vector2(0, -80), new Vector2(1000, 350));
        body.alignment = TextAlignmentOptions.Center;
        body.color = new Color(0.9f, 0.85f, 1f);

        var btn = BuildButton(root, "StartBtn", "ENTER THE ROYAL COURT", new Vector2(0, -340), new Vector2(560, 90), new Color(0.45f, 0.22f, 0.65f));
        return new SimplePanel { root = root, text = body, btn = btn };
    }

    SimplePanel BuildLoadingPanel(GameObject parent)
    {
        var root = CreateRectFull(parent, "LoadingPanel");
        AddImage(root, new Color(0.06f, 0.03f, 0.10f, 0.85f));
        var txt = AddTMP(root, "LoadTxt", "The King is composing his riddle...", 38, FontStyles.Italic, Vector2.zero, new Vector2(900, 80));
        txt.color = new Color(0.9f, 0.8f, 1f);
        return new SimplePanel { root = root, text = txt, btn = null };
    }

    SimplePanel BuildReactionPanel(GameObject parent)
    {
        var root = CreateRectFull(parent, "ReactionPanel");
        AddImage(root, new Color(0.05f, 0.2f, 0.07f, 0.93f)); // will be overridden at runtime
        var txt = AddTMP(root, "ReactionTxt", "", 48, FontStyles.Bold, Vector2.zero, new Vector2(1200, 500));
        txt.alignment = TextAlignmentOptions.Center;
        txt.color = Color.white;
        txt.enableWordWrapping = true;
        // No button — the whole panel is clickable (added at runtime by UIManager)
        return new SimplePanel { root = root, text = txt, btn = null };
    }

    SimplePanel BuildRevealPanel(GameObject parent)
    {
        var root = CreateRectFull(parent, "RevealPanel");
        AddImage(root, new Color(0.04f, 0.02f, 0.07f, 0.92f));
        var text = AddTMP(root, "RevealTxt", "", 34, FontStyles.Normal, new Vector2(0, 80), new Vector2(1100, 350));
        text.alignment = TextAlignmentOptions.Center;
        var btn = BuildButton(root, "ContinueBtn", "Face the King →", new Vector2(0, -250), new Vector2(400, 80), new Color(0.4f, 0.2f, 0.6f));
        return new SimplePanel { root = root, text = text, btn = btn };
    }

    JudgPanel BuildJudgmentPanel(GameObject parent)
    {
        var root = CreateRectFull(parent, "JudgmentPanel");
        AddImage(root, new Color(0.12f, 0.03f, 0.03f, 0.95f));
        var text = AddTMP(root, "JudgTxt", "", 36, FontStyles.Normal, new Vector2(0, 100), new Vector2(1100, 380));
        text.alignment = TextAlignmentOptions.Center;
        var flatterBtn = BuildButton(root, "FlatterBtn", "\"Your Majesty, you look absolutely divine!\"", new Vector2(-310, -300), new Vector2(560, 90), new Color(0.1f, 0.45f, 0.18f));
        var truthBtn   = BuildButton(root, "TruthBtn",   "\"...You're naked.\"",                          new Vector2(310, -300),  new Vector2(380, 90), new Color(0.5f, 0.08f, 0.08f));
        return new JudgPanel { root = root, text = text, flatterBtn = flatterBtn, truthBtn = truthBtn };
    }

    SimplePanel BuildWinPanel(GameObject parent)
    {
        var root = CreateRectFull(parent, "WinPanel");
        AddImage(root, new Color(0.03f, 0.12f, 0.05f, 0.96f));
        var title = AddTMP(root, "Title", "YOU LIVE!", 100, FontStyles.Bold, new Vector2(0, 280), new Vector2(800, 140));
        title.color = new Color(0.3f, 1f, 0.45f);
        var text = AddTMP(root, "WinTxt", "", 32, FontStyles.Normal, new Vector2(0, 10), new Vector2(1000, 380));
        text.alignment = TextAlignmentOptions.Center;
        var btn = BuildButton(root, "PlayAgainBtn", "Serve the King Again", new Vector2(0, -310), new Vector2(440, 85), new Color(0.1f, 0.45f, 0.18f));
        return new SimplePanel { root = root, text = text, btn = btn };
    }

    SimplePanel BuildDeathPanel(GameObject parent)
    {
        var root = CreateRectFull(parent, "DeathPanel");
        AddImage(root, new Color(0.14f, 0.02f, 0.02f, 0.97f));
        var title = AddTMP(root, "Title", "OFF WITH YOUR HEAD!", 72, FontStyles.Bold, new Vector2(0, 280), new Vector2(1100, 120));
        title.color = new Color(1f, 0.18f, 0.18f);
        var text = AddTMP(root, "DeathTxt", "", 32, FontStyles.Normal, new Vector2(0, 10), new Vector2(1000, 380));
        text.alignment = TextAlignmentOptions.Center;
        var btn = BuildButton(root, "PlayAgainBtn", "Try Again (Lie This Time)", new Vector2(0, -310), new Vector2(460, 85), new Color(0.5f, 0.08f, 0.08f));
        return new SimplePanel { root = root, text = text, btn = btn };
    }

    // ── HELPERS ───────────────────────────────────────────────────────────

    GameObject CreateRectFull(GameObject parent, string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        return go;
    }

    GameObject CreateRect(GameObject parent, string name, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        return go;
    }

    Image AddImage(GameObject go, Color color)
    {
        var img = go.GetComponent<Image>() ?? go.AddComponent<Image>();
        img.color = color;
        return img;
    }

    TextMeshProUGUI AddTMP(GameObject parent, string name, string text, int size, FontStyles style, Vector2 pos, Vector2 sz)
    {
        var go = CreateRect(parent, name, pos, sz);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text; tmp.fontSize = size; tmp.fontStyle = style;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white; tmp.enableWordWrapping = true;
        return tmp;
    }

    Button BuildButton(GameObject parent, string name, string label, Vector2 pos, Vector2 size, Color color)
    {
        var go = CreateRect(parent, name, pos, size);
        AddImage(go, color);
        var btn = go.AddComponent<Button>();
        var cb = btn.colors;
        cb.highlightedColor = color * 1.3f; cb.pressedColor = color * 0.7f;
        btn.colors = cb;
        var tGO = new GameObject("Label");
        tGO.transform.SetParent(go.transform, false);
        var tRT = tGO.AddComponent<RectTransform>();
        tRT.anchorMin = Vector2.zero; tRT.anchorMax = Vector2.one; tRT.sizeDelta = Vector2.zero;
        var tmp = tGO.AddComponent<TextMeshProUGUI>();
        tmp.text = label; tmp.fontSize = 24; tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center; tmp.color = Color.white;
        tmp.enableWordWrapping = true;
        return btn;
    }

    Button BuildOptionButtonPrefab()
    {
        var go = new GameObject("OptionBtnPrefab");
        go.transform.SetParent(transform, false);
        go.SetActive(false);
        go.AddComponent<RectTransform>().sizeDelta = new Vector2(300, 90);
        AddImage(go, new Color(0.28f, 0.14f, 0.44f));
        var btn = go.AddComponent<Button>();
        var cb = btn.colors;
        cb.highlightedColor = new Color(0.5f, 0.28f, 0.75f);
        cb.pressedColor     = new Color(0.18f, 0.08f, 0.28f);
        btn.colors = cb;
        var le = go.AddComponent<LayoutElement>();
        le.preferredWidth = 300; le.preferredHeight = 90;
        var tGO = new GameObject("Label");
        tGO.transform.SetParent(go.transform, false);
        var tRT = tGO.AddComponent<RectTransform>();
        tRT.anchorMin = Vector2.zero; tRT.anchorMax = Vector2.one; tRT.sizeDelta = Vector2.zero;
        var tmp = tGO.AddComponent<TextMeshProUGUI>();
        tmp.text = "Option"; tmp.fontSize = 26; tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center; tmp.color = Color.white;
        return btn;
    }
}
