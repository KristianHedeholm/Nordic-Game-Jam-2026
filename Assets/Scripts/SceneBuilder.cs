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
        // ── LAYER 1: BEDROOM BACKGROUND (always visible) ──────────────────
        var bgGO = CreateFull(stagePanelGO, "BG_Bedroom");
        SetSprite(bgGO.AddComponent<Image>(), "Art/BG_Bedroom", false);

        // ── LAYER 2: PLAYER BUNNY — bottom left, watching ──────────────────
        var playerGO = CreateRect(stagePanelGO, "PlayerBunny", new Vector2(-369.6f, -252.9f), new Vector2(512.8f, 717.98f));
        SetSprite(playerGO.AddComponent<Image>(), "Art/Player_Bunny", true);

        // ── LAYER 3: NAKED KING — centre, hidden until reveal ──────────────
        var nakedKingGO = CreateRect(stagePanelGO, "KingNaked", new Vector2(483.2f, 134.9f), new Vector2(380, 600));
        SetSprite(nakedKingGO.AddComponent<Image>(), "Art/King_Naked", true);
        nakedKingGO.SetActive(false);

        // ── LAYER 4: LEFT CURTAIN ──────────────────────────────────────────
        var curtainL = CreateRect(stagePanelGO, "CurtainLeft", new Vector2(193f, 164.1f), new Vector2(650, 750));
        var clRT = curtainL.GetComponent<RectTransform>(); clRT.anchorMin = new Vector2(0.5f, 0.5f); clRT.anchorMax = new Vector2(0.5f, 0.5f); clRT.anchoredPosition = new Vector2(193f, 164.1f);
        SetSprite(curtainL.AddComponent<Image>(), "Art/Curtain_Left", false);

        // ── LAYER 5: RIGHT CURTAIN ─────────────────────────────────────────
        var curtainR = CreateRect(stagePanelGO, "CurtainRight", new Vector2(635.9f, 162.5f), new Vector2(650, 750));
        var crRT = curtainR.GetComponent<RectTransform>(); crRT.anchorMin = new Vector2(0.5f, 0.5f); crRT.anchorMax = new Vector2(0.5f, 0.5f); crRT.anchoredPosition = new Vector2(635.9f, 162.5f);
        SetSprite(curtainR.AddComponent<Image>(), "Art/Curtain_Right", false);

        // ── LAYER 6: KING SILHOUETTE — on curtains, hidden on reveal ───────
        var silhouetteGO = CreateRect(stagePanelGO, "KingSilhouette", new Vector2(483.2f, 134.9f), new Vector2(380, 600));
        var silImg = silhouetteGO.AddComponent<Image>();
        SetSprite(silImg, "Art/King_Silhouette", true);
        silImg.color = new Color(1f, 1f, 1f, 0.55f); // semi-transparent blend

        var curtainAnim = stagePanelGO.AddComponent<CurtainAnimator>();
        curtainAnim.curtainLeft  = curtainL.GetComponent<RectTransform>();
        curtainAnim.curtainRight = curtainR.GetComponent<RectTransform>();

        // Invisible refs for proud pose (unused but wired)
        var armL = CreateRect(stagePanelGO, "ArmL", new Vector2(0, 0), new Vector2(1, 1));
        var armR = CreateRect(stagePanelGO, "ArmR", new Vector2(0, 0), new Vector2(1, 1));
        var body = CreateRect(stagePanelGO, "KingBody", new Vector2(0, 0), new Vector2(1, 1));
        var revealBgGO = nakedKingGO; // reuse for UIManager wiring

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

        // ── SPEECH BUBBLE — left of king, tail pointing right toward king ─────
        // King is at x≈483, so bubble sits to the left at x≈-200
        var bubblePanel = CreateRect(stagePanelGO, "SpeechBubble", new Vector2(-250, 300), new Vector2(700, 220));
        var bubbleImg = bubblePanel.AddComponent<Image>();
        var speechBubbleSprite = Resources.Load<Sprite>("Art/Speech_Bubble_New");
        if (speechBubbleSprite != null) { bubbleImg.sprite = speechBubbleSprite; bubbleImg.type = Image.Type.Simple; bubbleImg.preserveAspect = false; bubbleImg.color = Color.white; }
        else bubbleImg.color = new Color(0.97f, 0.95f, 0.88f);
        var riddleGO = new GameObject("RiddleText"); riddleGO.transform.SetParent(bubblePanel.transform, false);
        var riddleRT = riddleGO.AddComponent<RectTransform>(); riddleRT.anchorMin = Vector2.zero; riddleRT.anchorMax = Vector2.one; riddleRT.offsetMin = new Vector2(25,20); riddleRT.offsetMax = new Vector2(-25,-20);
        var riddleTMP = riddleGO.AddComponent<TextMeshProUGUI>();
        riddleTMP.text = "..."; riddleTMP.fontSize = 26; riddleTMP.fontStyle = FontStyles.Italic;
        riddleTMP.color = new Color(0.1f, 0.05f, 0.15f); riddleTMP.alignment = TextAlignmentOptions.Center; riddleTMP.enableWordWrapping = true;

        // ── NARRATOR LABEL — sits below the speech bubble ────────────────────
        var narratorGO = CreateRect(stagePanelGO, "NarratorLabel", new Vector2(-250, 175), new Vector2(700, 70));
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

        // Backgrounds / reveal swap
        ui.revealBackground = null;
        ui.nakedKingGO  = nakedKingGO;
        ui.silhouetteGO = silhouetteGO;

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
        var root = CreateFull(parent, "IntroPanel");
        // Full screen title image — baked logo + curtains
        var bgImg = root.AddComponent<Image>();
        var titleSpr = Resources.Load<Sprite>("Art/Title_Screen_v2");
        if (titleSpr != null) { bgImg.sprite = titleSpr; bgImg.type = Image.Type.Simple; bgImg.preserveAspect = false; bgImg.color = Color.white; }
        else bgImg.color = new Color(0.06f, 0.03f, 0.10f);

        // Tutorial slide background (replaces title bg for slides 1-3)
        // Stored separately — UIManager swaps bg on slide change
        var tutBgGO = CreateFull(root, "TutorialBg");
        var tutBgImg = tutBgGO.AddComponent<Image>();
        var tutSpr = Resources.Load<Sprite>("Art/Tutorial_Screen");
        if (tutSpr != null) { tutBgImg.sprite = tutSpr; tutBgImg.type = Image.Type.Simple; tutBgImg.preserveAspect = false; tutBgImg.color = Color.white; }
        tutBgGO.SetActive(false); // shown when START is clicked

        // Body text — centred in spotlight, white bold text
        var body = AddTMP(root, "Body", "", 38, FontStyles.Bold, new Vector2(0, 80), new Vector2(900, 400));
        body.alignment = TextAlignmentOptions.Center;
        body.color = Color.white;

        // Button — bottom centre matching the yellow NEXT button in the image
        var btn = BuildButton(root, "StartButton", "START", new Vector2(0, -340), new Vector2(320, 90), new Color(0.85f, 0.6f, 0.05f));
        btn.GetComponentInChildren<TMP_Text>().color = new Color(0.1f, 0.05f, 0f);

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

    void SetSprite(Image img, string path, bool preserveAspect)
    {
        var spr = Resources.Load<Sprite>(path);
        if (spr != null) { img.sprite = spr; img.type = Image.Type.Simple; img.preserveAspect = preserveAspect; img.color = Color.white; }
        else img.color = new Color(0.2f, 0.1f, 0.3f);
    }

    void ApplySpeechBubbleSprite(GameObject go)
    {
        var img = go.AddComponent<Image>();
        var spr = Resources.Load<Sprite>("Art/Speech_Bubble_New");
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
