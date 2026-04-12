using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using TMPro;

/// <summary>
/// Builds the full game UI at runtime — stage, drag/drop tag holders, overlays.
/// Just attach to an empty GameObject and hit Play.
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
        cam.clearFlags      = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.05f, 0.03f, 0.08f);
        cam.orthographic    = true;
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

        // ── STAGE PANEL ───────────────────────────────────────────────────────
        var stagePanelGO = CreateFull(canvasGO, "StagePanel");
        AddImage(stagePanelGO, new Color(0.06f, 0.03f, 0.10f));

        // Floor
        AddImage(CreateRect(stagePanelGO, "Floor",     new Vector2(0, -350), new Vector2(1920, 220)), new Color(0.25f, 0.15f, 0.05f));
        AddImage(CreateRect(stagePanelGO, "FloorLine", new Vector2(0, -245), new Vector2(1920, 12)),  new Color(0.55f, 0.38f, 0.10f));

        // Geometric king removed — bunny king sprite used instead
        // Proud pose arms still needed for animation references (invisible placeholders)
        var armL = CreateRect(stagePanelGO, "ArmL", new Vector2(-165, -80), new Vector2(1, 1));
        var armR = CreateRect(stagePanelGO, "ArmR", new Vector2(165, -80),  new Vector2(1, 1));
        var body = CreateRect(stagePanelGO, "KingBody", new Vector2(0, -110), new Vector2(1, 1));

        // ── STAGE BACKGROUND (shown during guessing) ──────────────────────────
        var stageImgGO = CreateFull(stagePanelGO, "StageBackground");
        var stageImg = stageImgGO.AddComponent<Image>();
        var stageSpr = Resources.Load<Sprite>("Art/Stage_Background");
        if (stageSpr != null) { stageImg.sprite = stageSpr; stageImg.type = Image.Type.Simple; stageImg.preserveAspect = false; stageImg.color = Color.white; }
        else stageImg.color = new Color(0.06f, 0.03f, 0.10f);

        // ── REVEAL BACKGROUND (shown behind curtains when they open) ──────────
        var revealBgGO = CreateFull(stagePanelGO, "RevealBackground");
        var revealBgImg = revealBgGO.AddComponent<Image>();
        var revealBgSpr = Resources.Load<Sprite>("Art/Reveal_Background");
        if (revealBgSpr != null) { revealBgImg.sprite = revealBgSpr; revealBgImg.type = Image.Type.Simple; revealBgImg.preserveAspect = false; revealBgImg.color = Color.white; }
        else revealBgImg.color = new Color(0.06f, 0.03f, 0.10f);
        revealBgGO.SetActive(false); // hidden until reveal

        // ── BUNNY KING (replaces geometric king) ─────────────────────────────
        var bunnyGO = CreateRect(stagePanelGO, "KingBunny", new Vector2(0, -80), new Vector2(400, 630));
        var bunnyImg = bunnyGO.AddComponent<Image>();
        var bunnySpr = Resources.Load<Sprite>("Art/King_Bunny");
        if (bunnySpr != null) { bunnyImg.sprite = bunnySpr; bunnyImg.type = Image.Type.Simple; bunnyImg.preserveAspect = true; bunnyImg.color = Color.white; }
        else bunnyImg.color = new Color(0.95f, 0.75f, 0.55f);

        // ── CURTAINS (sprite-based, animate open on reveal) ───────────────────
        var curtainL = CreateRect(stagePanelGO, "CurtainLeft",  new Vector2(-480, 0), new Vector2(960, 1080));
        var clImg = curtainL.AddComponent<Image>();
        var clSpr = Resources.Load<Sprite>("Art/Curtain_Left");
        if (clSpr != null) { clImg.sprite = clSpr; clImg.type = Image.Type.Simple; clImg.preserveAspect = false; clImg.color = Color.white; }
        else clImg.color = new Color(0.55f, 0.05f, 0.08f);

        var curtainR = CreateRect(stagePanelGO, "CurtainRight", new Vector2(480, 0), new Vector2(960, 1080));
        var crImg = curtainR.AddComponent<Image>();
        var crSpr = Resources.Load<Sprite>("Art/Curtain_Right");
        if (crSpr != null) { crImg.sprite = crSpr; crImg.type = Image.Type.Simple; crImg.preserveAspect = false; crImg.color = Color.white; }
        else crImg.color = new Color(0.55f, 0.05f, 0.08f);

        var curtainAnim = stagePanelGO.AddComponent<CurtainAnimator>();
        curtainAnim.curtainLeft  = curtainL.GetComponent<RectTransform>();
        curtainAnim.curtainRight = curtainR.GetComponent<RectTransform>();

        // ── LEFT PANEL: Tag drop zones ────────────────────────────────────────
        string[] holderSprites = { "Place_Garment_tag_holder", "Place_Colour_tag_holder", "Place_Material_tag_holder" };
        string[] holderCats    = { "Clothing", "Color", "Material" };
        var dropZones   = new TagDropZone[3];
        var trackerTMPs = new TextMeshProUGUI[3];

        for (int i = 0; i < 3; i++)
        {
            var holder = CreateRect(stagePanelGO, $"DropZone_{holderCats[i]}", new Vector2(-750, 180 - i * 220), new Vector2(260, 200));
            var hImg   = holder.AddComponent<Image>();
            var hSpr   = Resources.Load<Sprite>($"Art/{holderSprites[i]}");
            if (hSpr != null) { hImg.sprite = hSpr; hImg.type = Image.Type.Simple; hImg.preserveAspect = true; hImg.color = Color.white; }
            else hImg.color = new Color(0.2f, 0.12f, 0.3f);

            var aGO = new GameObject("AnswerLabel"); aGO.transform.SetParent(holder.transform, false);
            var aRT = aGO.AddComponent<RectTransform>(); aRT.anchorMin = Vector2.zero; aRT.anchorMax = Vector2.one; aRT.offsetMin = new Vector2(10,10); aRT.offsetMax = new Vector2(-10,-10);
            var aTMP = aGO.AddComponent<TextMeshProUGUI>();
            aTMP.text = ""; aTMP.fontSize = 22; aTMP.fontStyle = FontStyles.Bold;
            aTMP.alignment = TextAlignmentOptions.Center; aTMP.color = new Color(0.1f, 0.05f, 0.2f); aTMP.enableWordWrapping = true;

            var dz = holder.AddComponent<TagDropZone>();
            dz.category = holderCats[i]; dz.answerLabel = aTMP;
            dropZones[i] = dz; trackerTMPs[i] = aTMP;
        }

        // ── SPEECH BUBBLE — top center ────────────────────────────────────────
        var bubblePanel = CreateRect(stagePanelGO, "SpeechBubble", new Vector2(200, 400), new Vector2(800, 200));
        var bubbleImg = bubblePanel.AddComponent<Image>();
        var speechBubbleSprite = Resources.Load<Sprite>("Art/Speech bubble");
        if (speechBubbleSprite != null) { bubbleImg.sprite = speechBubbleSprite; bubbleImg.type = Image.Type.Simple; bubbleImg.preserveAspect = false; bubbleImg.color = Color.white; }
        else bubbleImg.color = new Color(0.97f, 0.95f, 0.88f);
        // No separate tail — speech bubble sprite includes it
        var riddleGO = new GameObject("RiddleText"); riddleGO.transform.SetParent(bubblePanel.transform, false);
        var riddleRT = riddleGO.AddComponent<RectTransform>(); riddleRT.anchorMin = Vector2.zero; riddleRT.anchorMax = Vector2.one; riddleRT.offsetMin = new Vector2(25,20); riddleRT.offsetMax = new Vector2(-25,-20);
        var riddleTMP = riddleGO.AddComponent<TextMeshProUGUI>();
        riddleTMP.text = "..."; riddleTMP.fontSize = 26; riddleTMP.fontStyle = FontStyles.Italic;
        riddleTMP.color = new Color(0.1f, 0.05f, 0.15f); riddleTMP.alignment = TextAlignmentOptions.Center; riddleTMP.enableWordWrapping = true;

        // ── NARRATOR LABEL — sits below the speech bubble, visible on reveal ──
        var narratorGO = CreateRect(stagePanelGO, "NarratorLabel", new Vector2(200, 195), new Vector2(800, 70));
        var narratorTMP = narratorGO.AddComponent<TextMeshProUGUI>();
        narratorTMP.text = "";
        narratorTMP.fontSize = 24;
        narratorTMP.fontStyle = FontStyles.Italic;
        narratorTMP.alignment = TextAlignmentOptions.Center;
        narratorTMP.color = new Color(0.95f, 0.9f, 1f);
        narratorTMP.enableWordWrapping = true;

        // Category label removed — not needed
        TextMeshProUGUI catTMP = null;

        // ── SCATTER AREA (bottom centre) ──────────────────────────────────────
        var bottomPanel = CreateRect(canvasGO, "OptionsRow", new Vector2(200, -420), new Vector2(1400, 220));

        // ── DRAGGABLE TAG PREFAB ──────────────────────────────────────────────
        var tagPrefabGO = new GameObject("DraggableTagPrefab");
        tagPrefabGO.transform.SetParent(transform, false); tagPrefabGO.SetActive(false);
        tagPrefabGO.AddComponent<RectTransform>().sizeDelta = new Vector2(190, 60);
        var tImg = tagPrefabGO.AddComponent<Image>();
        // Match the dark grey inner area of the holder sprite
        tImg.color = new Color(0.55f, 0.55f, 0.58f);
        var tLblGO = new GameObject("Label"); tLblGO.transform.SetParent(tagPrefabGO.transform, false);
        var tLblRT = tLblGO.AddComponent<RectTransform>(); tLblRT.anchorMin = Vector2.zero; tLblRT.anchorMax = Vector2.one; tLblRT.sizeDelta = Vector2.zero;
        var tLblTMP = tLblGO.AddComponent<TextMeshProUGUI>();
        tLblTMP.text = ""; tLblTMP.fontSize = 22; tLblTMP.fontStyle = FontStyles.Bold;
        tLblTMP.enableAutoSizing = true;
        tLblTMP.fontSizeMax = 19;
        tLblTMP.fontSizeMin = 18;
        tLblTMP.alignment = TextAlignmentOptions.Center; tLblTMP.color = new Color(1f, 1f, 1f); tLblTMP.enableWordWrapping = false;
        tagPrefabGO.AddComponent<DraggableTag>();
        tagPrefabGO.AddComponent<CanvasGroup>().blocksRaycasts = true;

        // ── OVERLAY PANELS ────────────────────────────────────────────────────
        var introPanel    = BuildIntroPanel(canvasGO);
        var loadingPanel  = BuildLoadingPanel(canvasGO);
        var reactionPanel = BuildReactionPanel(canvasGO);
        var revealPanel   = BuildRevealPanel(canvasGO);
        var judgmentPanel = BuildJudgmentPanel(canvasGO);
        var winPanel      = BuildWinPanel(canvasGO);
        var deathPanel    = BuildDeathPanel(canvasGO);

        // ── WIRE UP ───────────────────────────────────────────────────────────
        var mgrGO = new GameObject("GameManager");
        var gm = mgrGO.AddComponent<GameManager>();
        var rg = mgrGO.AddComponent<RiddleGenerator>();
        var ui = mgrGO.AddComponent<UIManager>();
        var diamond = mgrGO.AddComponent<Diamond>();
        mgrGO.AddComponent<AudioManager>();
        gameObject.AddComponent<AppQuit>();

        // Stage
        ui.stagePanel      = stagePanelGO;
        ui.curtainAnimator = curtainAnim;
        ui.riddleText      = riddleTMP;
        ui.categoryLabel   = catTMP;
        ui.optionsContainer = bottomPanel.transform;
        ui.optionButtonPrefab = null; // unused — drag/drop only
        ui.draggableTagPrefab = tagPrefabGO;

        // Backgrounds
        ui.revealBackground = revealBgGO;

        // Narrator
        ui.narratorLabel = narratorTMP;

        // Drop zones
        ui.dropZoneClothing = dropZones[0];
        ui.dropZoneColor    = dropZones[1];
        ui.dropZoneMaterial = dropZones[2];

        // Tracker (reuse drop zone answer labels)
        ui.trackerClothing = trackerTMPs[0];
        ui.trackerColor    = trackerTMPs[1];
        ui.trackerMaterial = trackerTMPs[2];

        // Overlays
        ui.reactionPanel = reactionPanel.root; ui.reactionText = reactionPanel.text; ui.reactionBg = reactionPanel.root.GetComponent<Image>();
        ui.introPanel = introPanel.root; ui.introText = introPanel.text; ui.introStartButton = introPanel.btn;
        ui.loadingPanel = loadingPanel.root;
        ui.revealPanel = revealPanel.root; ui.revealText = revealPanel.text; ui.revealContinueButton = revealPanel.btn;
        ui.finalJudgmentPanel = judgmentPanel.root; ui.finalText = judgmentPanel.text; ui.flatterButton = judgmentPanel.flatterBtn; ui.truthButton = judgmentPanel.truthBtn;
        ui.winPanel = winPanel.root; ui.winText = winPanel.text; ui.winPlayAgainButton = winPanel.btn;
        ui.deathPanel = deathPanel.root; ui.deathText = deathPanel.text; ui.deathPlayAgainButton = deathPanel.btn;

        // Button sprites
        ui.buttonStartSprite = Resources.Load<Sprite>("Art/Button_Start");
        ui.buttonNextSprite  = Resources.Load<Sprite>("Art/Button_Next");

        // King proud pose
        var armLRT = armL.GetComponent<RectTransform>();
        var armRRT = armR.GetComponent<RectTransform>();
        var bodyRT = body.GetComponent<RectTransform>();
        ui.kingPoseProud = (proud) => {
            if (proud) {
                armLRT.anchoredPosition = new Vector2(-165, 20);  armLRT.localRotation = Quaternion.Euler(0,0,45f);
                armRRT.anchoredPosition = new Vector2(165, 20);   armRRT.localRotation = Quaternion.Euler(0,0,-45f);
                bodyRT.sizeDelta = new Vector2(250, 200);
            } else {
                armLRT.anchoredPosition = new Vector2(-165, -80); armLRT.localRotation = Quaternion.identity;
                armRRT.anchoredPosition = new Vector2(165, -80);  armRRT.localRotation = Quaternion.identity;
                bodyRT.sizeDelta = new Vector2(220, 200);
            }
        };

        // Hide all overlays
        introPanel.root.SetActive(false); loadingPanel.root.SetActive(false); reactionPanel.root.SetActive(false);
        revealPanel.root.SetActive(false); judgmentPanel.root.SetActive(false); winPanel.root.SetActive(false); deathPanel.root.SetActive(false);

        gm.uiManager = ui; 
        gm.riddleGenerator = rg;
        gm.diamond = diamond;
        gm.StartGame();
    }

    // ── OVERLAY BUILDERS ─────────────────────────────────────────────────────

    struct SimplePanel { public GameObject root; public TMP_Text text; public Button btn; }
    struct JudgPanel   { public GameObject root; public TMP_Text text; public Button flatterBtn; public Button truthBtn; }

    SimplePanel BuildIntroPanel(GameObject parent)
    {
        var root = CreateFull(parent, "IntroPanel"); AddImage(root, new Color(0.06f, 0.03f, 0.10f, 0.97f));
        var title = AddTMP(root, "Title", "FASHION ROYAL", 90, FontStyles.Bold, new Vector2(0, 200), new Vector2(1800, 200));
        title.color = new Color(1f, 0.85f, 0.3f);
        var body = AddTMP(root, "Body", "", 30, FontStyles.Normal, new Vector2(0, -40), new Vector2(1000, 350));
        body.alignment = TextAlignmentOptions.Center; body.color = new Color(0.9f, 0.85f, 1f);
        var team = AddTMP(root, "TeamName", "by Invisible Tailors", 24, FontStyles.Italic, new Vector2(0, -440), new Vector2(800, 50));
        team.color = new Color(0.7f, 0.6f, 0.9f);
        // Button size matches SVG natural ratio (321x125 → 2x = 642x250, but display at 480x188 to avoid stretch)
        var btn = BuildButton(root, "StartButton", "ENTER THE ROYAL COURT", new Vector2(0, -310), new Vector2(320, 125), new Color(0.45f, 0.22f, 0.65f));
        return new SimplePanel { root = root, text = body, btn = btn };
    }

    SimplePanel BuildLoadingPanel(GameObject parent)
    {
        var root = CreateFull(parent, "LoadingPanel"); AddImage(root, new Color(0.06f, 0.03f, 0.10f, 0.85f));
        var txt = AddTMP(root, "LoadTxt", "The King is composing his riddle...", 38, FontStyles.Italic, Vector2.zero, new Vector2(900, 80));
        txt.color = new Color(0.9f, 0.8f, 1f);
        return new SimplePanel { root = root, text = txt, btn = null };
    }

    SimplePanel BuildReactionPanel(GameObject parent)
    {
        var root = CreateFull(parent, "ReactionPanel"); AddImage(root, new Color(0.05f, 0.2f, 0.07f, 0.93f));
        var txt = AddTMP(root, "ReactionTxt", "", 48, FontStyles.Bold, Vector2.zero, new Vector2(1200, 500));
        txt.alignment = TextAlignmentOptions.Center; txt.color = Color.white; txt.enableWordWrapping = true;
        return new SimplePanel { root = root, text = txt, btn = null };
    }

    SimplePanel BuildRevealPanel(GameObject parent)
    {
        var root = CreateFull(parent, "RevealPanel"); AddImage(root, new Color(0,0,0,0));
        var bubble = CreateRect(root, "RevealBubble", new Vector2(380, 260), new Vector2(700, 200));
        ApplySpeechBubbleSprite(bubble);
        var tGO = new GameObject("RevealText"); tGO.transform.SetParent(bubble.transform, false);
        var tRT = tGO.AddComponent<RectTransform>(); tRT.anchorMin = Vector2.zero; tRT.anchorMax = Vector2.one; tRT.offsetMin = new Vector2(30,20); tRT.offsetMax = new Vector2(-30,-20);
        var tmp = tGO.AddComponent<TextMeshProUGUI>(); tmp.text=""; tmp.fontSize=26; tmp.alignment=TextAlignmentOptions.Center; tmp.color=new Color(0.1f,0.05f,0.15f); tmp.enableWordWrapping=true;
        var btn = BuildButton(root, "ContinueBtn", "Continue →", new Vector2(0,-460), new Vector2(240,55), new Color(0.4f,0.2f,0.6f));
        return new SimplePanel { root=root, text=tmp, btn=btn };
    }

    JudgPanel BuildJudgmentPanel(GameObject parent)
    {
        var root = CreateFull(parent, "JudgmentPanel"); AddImage(root, new Color(0,0,0,0));
        var bubble = CreateRect(root, "JudgBubble", new Vector2(380,260), new Vector2(700,200));
        ApplySpeechBubbleSprite(bubble);
        var tGO = new GameObject("JudgText"); tGO.transform.SetParent(bubble.transform, false);
        var tRT = tGO.AddComponent<RectTransform>(); tRT.anchorMin=Vector2.zero; tRT.anchorMax=Vector2.one; tRT.offsetMin=new Vector2(30,20); tRT.offsetMax=new Vector2(-30,-20);
        var tmp = tGO.AddComponent<TextMeshProUGUI>(); tmp.text=""; tmp.fontSize=26; tmp.alignment=TextAlignmentOptions.Center; tmp.color=new Color(0.1f,0.05f,0.15f); tmp.enableWordWrapping=true;
        var neutral = new Color(0.35f, 0.18f, 0.55f);
        var flatterBtn = BuildButton(root, "FlatterBtn", "\"Yes Your Majesty!\"",              new Vector2(-300,-460), new Vector2(420,55), neutral);
        var truthBtn   = BuildButton(root, "TruthBtn",   "\"...Why are you wearing nothing?\"", new Vector2(270,-460),  new Vector2(360,55), neutral);
        return new JudgPanel { root=root, text=tmp, flatterBtn=flatterBtn, truthBtn=truthBtn };
    }

    SimplePanel BuildWinPanel(GameObject parent)
    {
        var root = CreateFull(parent, "WinPanel"); AddImage(root, new Color(0.03f,0.12f,0.05f,0.96f));
        var title = AddTMP(root, "Title", "YOU LIVE!", 100, FontStyles.Bold, new Vector2(0,280), new Vector2(800,140)); title.color = new Color(0.3f,1f,0.45f);
        var text = AddTMP(root, "WinText", "", 32, FontStyles.Normal, new Vector2(0,10), new Vector2(1000,380)); text.alignment = TextAlignmentOptions.Center;
        var btn = BuildButton(root, "PlayAgainBtn", "Serve the King Again", new Vector2(0,-310), new Vector2(340,65), new Color(0.1f,0.45f,0.18f));
        return new SimplePanel { root=root, text=text, btn=btn };
    }

    SimplePanel BuildDeathPanel(GameObject parent)
    {
        var root = CreateFull(parent, "DeathPanel"); AddImage(root, new Color(0.14f,0.02f,0.02f,0.97f));
        var title = AddTMP(root, "Title", "OFF WITH YOUR HEAD!", 72, FontStyles.Bold, new Vector2(0,280), new Vector2(1100,120)); title.color = new Color(1f,0.18f,0.18f);
        var text = AddTMP(root, "DeathText", "", 32, FontStyles.Normal, new Vector2(0,10), new Vector2(1000,380)); text.alignment = TextAlignmentOptions.Center;
        var btn = BuildButton(root, "PlayAgainBtn", "Try Again (Lie This Time)", new Vector2(0,-310), new Vector2(360,65), new Color(0.5f,0.08f,0.08f));
        return new SimplePanel { root=root, text=text, btn=btn };
    }

    // ── HELPERS ───────────────────────────────────────────────────────────────

    GameObject CreateFull(GameObject parent, string name)
    {
        var go = new GameObject(name); go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>(); rt.anchorMin=Vector2.zero; rt.anchorMax=Vector2.one; rt.offsetMin=Vector2.zero; rt.offsetMax=Vector2.zero;
        return go;
    }

    GameObject CreateRect(GameObject parent, string name, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name); go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>(); rt.anchoredPosition=pos; rt.sizeDelta=size;
        return go;
    }

    void ApplySpeechBubbleSprite(GameObject go)
    {
        var img = go.AddComponent<Image>();
        var spr = Resources.Load<Sprite>("Art/Speech bubble");
        if (spr != null) { img.sprite = spr; img.type = Image.Type.Simple; img.preserveAspect = false; img.color = Color.white; }
        else img.color = new Color(0.97f, 0.95f, 0.88f);
    }

    Image AddImage(GameObject go, Color color)
    {
        var img = go.GetComponent<Image>() ?? go.AddComponent<Image>(); img.color = color; return img;
    }

    TextMeshProUGUI AddTMP(GameObject parent, string name, string text, int size, FontStyles style, Vector2 pos, Vector2 sz)
    {
        var go = CreateRect(parent, name, pos, sz);
        var tmp = go.AddComponent<TextMeshProUGUI>(); tmp.text=text; tmp.fontSize=size; tmp.fontStyle=style;
        tmp.alignment=TextAlignmentOptions.Center; tmp.color=Color.white; tmp.enableWordWrapping=true; return tmp;
    }

    Button BuildButton(GameObject parent, string name, string label, Vector2 pos, Vector2 size, Color color)
    {
        var go = CreateRect(parent, name, pos, size); AddImage(go, color);
        var btn = go.AddComponent<Button>(); btn.onClick.AddListener(() => AudioManager.Instance?.PlayButtonClick());
        var cb = btn.colors; cb.highlightedColor=color*1.3f; cb.pressedColor=color*0.7f; btn.colors=cb;
        var lGO = new GameObject("Label"); lGO.transform.SetParent(go.transform, false);
        var lRT = lGO.AddComponent<RectTransform>(); lRT.anchorMin=Vector2.zero; lRT.anchorMax=Vector2.one; lRT.sizeDelta=Vector2.zero;
        var tmp = lGO.AddComponent<TextMeshProUGUI>(); tmp.text=label; tmp.fontSize=20; tmp.fontStyle=FontStyles.Bold;
        tmp.alignment=TextAlignmentOptions.Center; tmp.color=Color.white; tmp.enableWordWrapping=true; return btn;
    }
}
