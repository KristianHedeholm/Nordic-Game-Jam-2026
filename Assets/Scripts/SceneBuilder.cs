using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using TMPro;

/// <summary>
/// AUTO-BUILDS the entire UI scene at runtime.
/// Attach this to an empty GameObject called "SceneBuilder" in your scene.
/// Hit Play and the full game appears.
/// </summary>
[DefaultExecutionOrder(-100)]
public class SceneBuilder : MonoBehaviour
{
    void Awake()
    {
        BuildScene();
    }

    void BuildScene()
    {
        // ── EVENT SYSTEM (required for button clicks) ────────────────────────
        if (FindFirstObjectByType<EventSystem>() == null)
        {
            var esGO = new GameObject("EventSystem");
            esGO.AddComponent<EventSystem>();
            esGO.AddComponent<InputSystemUIInputModule>();
        }

        // ── CAMERA SETUP ─────────────────────────────────────────────────────
        var cam = Camera.main;
        if (cam != null)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.06f, 0.04f, 0.10f);
            cam.orthographic = true;
        }

        // ── ROOT CANVAS ──────────────────────────────────────────────────────
        var canvasGO = new GameObject("Canvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGO.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        // ── BACKGROUND ───────────────────────────────────────────────────────
        var bg = MakeImage(canvasGO, "Background", new Color(0.08f, 0.05f, 0.10f), Vector2.zero, new Vector2(1920, 1080));

        // ── PANELS ───────────────────────────────────────────────────────────
        var introPanel    = BuildIntroPanel(canvasGO);
        var loadingPanel  = BuildLoadingPanel(canvasGO);
        var guessPanel    = BuildGuessPanel(canvasGO);
        var revealPanel   = BuildRevealPanel(canvasGO);
        var judgmentPanel = BuildJudgmentPanel(canvasGO);
        var winPanel      = BuildWinPanel(canvasGO);
        var deathPanel    = BuildDeathPanel(canvasGO);

        // ── GAME OBJECTS ─────────────────────────────────────────────────────
        var mgrGO = new GameObject("GameManager");
        var gm    = mgrGO.AddComponent<GameManager>();
        var rg    = mgrGO.AddComponent<RiddleGenerator>();
        var ui    = mgrGO.AddComponent<UIManager>();

        // Wire API key
        rg.apiKey = "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";
        rg.model  = "openai/gpt-4o-mini";

        // Wire UIManager panels
        ui.introPanel         = introPanel.root;
        ui.loadingPanel       = loadingPanel.root;
        ui.guessPanel         = guessPanel.root;
        ui.revealPanel        = revealPanel.root;
        ui.finalJudgmentPanel = judgmentPanel.root;
        ui.winPanel           = winPanel.root;
        ui.deathPanel         = deathPanel.root;

        ui.introText          = introPanel.text;
        ui.introStartButton   = introPanel.btn;

        ui.riddleText         = guessPanel.riddle;
        ui.categoryLabel      = guessPanel.label;
        ui.optionsContainer   = guessPanel.optionsContainer;
        ui.optionButtonPrefab = MakeOptionButtonPrefab();

        ui.revealText             = revealPanel.text;
        ui.revealContinueButton   = revealPanel.btn;

        ui.finalText      = judgmentPanel.text;
        ui.flatterButton  = judgmentPanel.flatterBtn;
        ui.truthButton    = judgmentPanel.truthBtn;

        ui.winText            = winPanel.text;
        ui.winPlayAgainButton = winPanel.btn;

        ui.deathText            = deathPanel.text;
        ui.deathPlayAgainButton = deathPanel.btn;

        gm.uiManager       = ui;
        gm.riddleGenerator = rg;

        // Hide all panels first, then GameManager.Start() will show intro
        introPanel.root.SetActive(false);
        loadingPanel.root.SetActive(false);
        guessPanel.root.SetActive(false);
        revealPanel.root.SetActive(false);
        judgmentPanel.root.SetActive(false);
        winPanel.root.SetActive(false);
        deathPanel.root.SetActive(false);

        // Manually kick off the game now that everything is wired
        gm.StartGame();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // PANEL BUILDERS
    // ─────────────────────────────────────────────────────────────────────────

    struct SimplePanel { public GameObject root; public TMP_Text text; public Button btn; }
    struct GuessInfo   { public GameObject root; public TMP_Text riddle; public TMP_Text label; public Transform optionsContainer; }
    struct JudgPanel   { public GameObject root; public TMP_Text text; public Button flatterBtn; public Button truthBtn; }

    SimplePanel BuildIntroPanel(GameObject parent)
    {
        var root = MakePanel(parent, "IntroPanel", new Color(0.1f, 0.07f, 0.14f, 0.97f));
        var title = MakeText(root, "Title", "THE KING'S NEW CLOTHES", 72, FontStyles.Bold, new Vector2(0, 200), new Vector2(1400, 120));
        title.color = new Color(1f, 0.85f, 0.3f);
        var body  = MakeText(root, "Body", "", 32, FontStyles.Normal, new Vector2(0, 0), new Vector2(1100, 400));
        body.alignment = TextAlignmentOptions.Center;
        var btn   = MakeButton(root, "StartButton", "BEGIN AUDIENCE", new Vector2(0, -280), new Vector2(400, 80));
        return new SimplePanel { root = root, text = body, btn = btn };
    }

    SimplePanel BuildLoadingPanel(GameObject parent)
    {
        var root = MakePanel(parent, "LoadingPanel", new Color(0.06f, 0.04f, 0.10f, 0.98f));
        var txt  = MakeText(root, "LoadingText", "The King is composing his riddle...", 40, FontStyles.Italic, Vector2.zero, new Vector2(900, 100));
        txt.color = new Color(0.9f, 0.8f, 1f);
        return new SimplePanel { root = root, text = txt, btn = null };
    }

    GuessInfo BuildGuessPanel(GameObject parent)
    {
        var root  = MakePanel(parent, "GuessPanel", new Color(0.08f, 0.05f, 0.12f, 0.97f));

        // Decorative crown label
        var crown = MakeText(root, "Crown", "♛", 80, FontStyles.Normal, new Vector2(0, 380), new Vector2(200, 100));
        crown.color = new Color(1f, 0.85f, 0.3f);

        var label  = MakeText(root, "CategoryLabel", "What is the King's clothing?", 38, FontStyles.Bold, new Vector2(0, 280), new Vector2(1000, 80));
        label.color = new Color(1f, 0.85f, 0.3f);

        // Riddle box
        var riddleBox = MakeImage(root, "RiddleBox", new Color(0.15f, 0.08f, 0.22f, 0.9f), new Vector2(0, 100), new Vector2(1000, 200));
        var riddle = MakeText(riddleBox, "RiddleText", "\"...\"", 34, FontStyles.Italic, Vector2.zero, new Vector2(920, 180));
        riddle.alignment = TextAlignmentOptions.Center;
        riddle.color = new Color(0.95f, 0.9f, 1f);

        // Options row
        var optionsGO = new GameObject("OptionsRow");
        optionsGO.transform.SetParent(root.transform, false);
        var rt = optionsGO.AddComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(0, -180);
        rt.sizeDelta = new Vector2(1400, 100);
        var hlg = optionsGO.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 20;
        hlg.childForceExpandWidth  = false;
        hlg.childForceExpandHeight = false;
        hlg.childAlignment = TextAnchor.MiddleCenter;

        return new GuessInfo { root = root, riddle = riddle, label = label, optionsContainer = optionsGO.transform };
    }

    SimplePanel BuildRevealPanel(GameObject parent)
    {
        var root = MakePanel(parent, "RevealPanel", new Color(0.05f, 0.03f, 0.08f, 0.98f));
        // King placeholder (naked silhouette — art team will replace)
        var kingBox = MakeImage(root, "KingPlaceholder", new Color(0.2f, 0.12f, 0.3f), new Vector2(0, 120), new Vector2(300, 500));
        var kingLbl = MakeText(kingBox, "KingLabel", "👑\n(King)\n[naked]", 28, FontStyles.Italic, Vector2.zero, new Vector2(280, 480));
        kingLbl.alignment = TextAlignmentOptions.Center;
        kingLbl.color = new Color(0.8f, 0.7f, 1f);

        var text = MakeText(root, "RevealText", "", 30, FontStyles.Normal, new Vector2(0, -220), new Vector2(1000, 200));
        text.alignment = TextAlignmentOptions.Center;
        var btn  = MakeButton(root, "ContinueBtn", "Face the King →", new Vector2(0, -380), new Vector2(360, 70));
        return new SimplePanel { root = root, text = text, btn = btn };
    }

    JudgPanel BuildJudgmentPanel(GameObject parent)
    {
        var root = MakePanel(parent, "JudgmentPanel", new Color(0.12f, 0.04f, 0.04f, 0.98f));
        var crown = MakeText(root, "Crown", "♛", 80, FontStyles.Normal, new Vector2(0, 350), new Vector2(200, 100));
        crown.color = new Color(1f, 0.3f, 0.3f);
        var text = MakeText(root, "JudgText", "", 36, FontStyles.Normal, new Vector2(0, 80), new Vector2(1100, 400));
        text.alignment = TextAlignmentOptions.Center;

        var flatterBtn = MakeButton(root, "FlatterBtn", "\"Your Majesty, you look divine!\"", new Vector2(-320, -280), new Vector2(560, 90));
        ColorButton(flatterBtn, new Color(0.1f, 0.5f, 0.2f));

        var truthBtn   = MakeButton(root, "TruthBtn", "\"...You're naked.\"", new Vector2(320, -280), new Vector2(440, 90));
        ColorButton(truthBtn, new Color(0.5f, 0.1f, 0.1f));

        return new JudgPanel { root = root, text = text, flatterBtn = flatterBtn, truthBtn = truthBtn };
    }

    SimplePanel BuildWinPanel(GameObject parent)
    {
        var root = MakePanel(parent, "WinPanel", new Color(0.04f, 0.12f, 0.06f, 0.98f));
        var title = MakeText(root, "Title", "YOU LIVE!", 90, FontStyles.Bold, new Vector2(0, 280), new Vector2(800, 120));
        title.color = new Color(0.4f, 1f, 0.5f);
        var text = MakeText(root, "WinText", "", 32, FontStyles.Normal, new Vector2(0, 20), new Vector2(1000, 400));
        text.alignment = TextAlignmentOptions.Center;
        var btn = MakeButton(root, "PlayAgainBtn", "Serve the King Again", new Vector2(0, -300), new Vector2(420, 80));
        ColorButton(btn, new Color(0.1f, 0.5f, 0.2f));
        return new SimplePanel { root = root, text = text, btn = btn };
    }

    SimplePanel BuildDeathPanel(GameObject parent)
    {
        var root = MakePanel(parent, "DeathPanel", new Color(0.15f, 0.02f, 0.02f, 0.98f));
        var title = MakeText(root, "Title", "OFF WITH YOUR HEAD!", 72, FontStyles.Bold, new Vector2(0, 280), new Vector2(1100, 120));
        title.color = new Color(1f, 0.2f, 0.2f);
        var text = MakeText(root, "DeathText", "", 32, FontStyles.Normal, new Vector2(0, 20), new Vector2(1000, 400));
        text.alignment = TextAlignmentOptions.Center;
        var btn = MakeButton(root, "PlayAgainBtn", "Try Again (Lie This Time)", new Vector2(0, -300), new Vector2(460, 80));
        ColorButton(btn, new Color(0.5f, 0.1f, 0.1f));
        return new SimplePanel { root = root, text = text, btn = btn };
    }

    // ─────────────────────────────────────────────────────────────────────────
    // HELPERS
    // ─────────────────────────────────────────────────────────────────────────

    GameObject MakePanel(GameObject parent, string name, Color color)
    {
        var go = MakeImage(parent, name, color, Vector2.zero, new Vector2(1920, 1080));
        // Add rounded feel with child border
        return go;
    }

    GameObject MakeImage(GameObject parent, string name, Color color, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        var img = go.AddComponent<Image>();
        img.color = color;
        return go;
    }

    TMP_Text MakeText(GameObject parent, string name, string content, int fontSize, FontStyles style, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = content;
        tmp.fontSize = fontSize;
        tmp.fontStyle = style;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.enableWordWrapping = true;
        return tmp;
    }

    Button MakeButton(GameObject parent, string name, string label, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        var img = go.AddComponent<Image>();
        img.color = new Color(0.4f, 0.2f, 0.6f);
        var btn = go.AddComponent<Button>();
        var cb  = btn.colors;
        cb.highlightedColor = new Color(0.55f, 0.3f, 0.8f);
        cb.pressedColor     = new Color(0.25f, 0.1f, 0.4f);
        btn.colors = cb;

        var txtGO = new GameObject("Label");
        txtGO.transform.SetParent(go.transform, false);
        var trt = txtGO.AddComponent<RectTransform>();
        trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
        trt.sizeDelta = Vector2.zero;
        var tmp = txtGO.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 26;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.fontStyle = FontStyles.Bold;
        return btn;
    }

    void ColorButton(Button btn, Color color)
    {
        btn.GetComponent<Image>().color = color;
    }

    Button MakeOptionButtonPrefab()
    {
        var go = new GameObject("OptionButtonPrefab");
        go.transform.SetParent(transform, false); // attach to SceneBuilder, not canvas
        go.SetActive(false);
        var rt  = go.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(240, 80);
        var img = go.AddComponent<Image>();
        img.color = new Color(0.25f, 0.15f, 0.40f);
        var btn = go.AddComponent<Button>();
        var cb  = btn.colors;
        cb.highlightedColor = new Color(0.5f, 0.3f, 0.75f);
        cb.pressedColor     = new Color(0.15f, 0.08f, 0.25f);
        btn.colors = cb;
        var txtGO = new GameObject("Label");
        txtGO.transform.SetParent(go.transform, false);
        var trt = txtGO.AddComponent<RectTransform>();
        trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
        trt.sizeDelta = Vector2.zero;
        var tmp = txtGO.AddComponent<TextMeshProUGUI>();
        tmp.text = "Option";
        tmp.fontSize = 24;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        tmp.fontStyle = FontStyles.Bold;
        return btn;
    }
}
