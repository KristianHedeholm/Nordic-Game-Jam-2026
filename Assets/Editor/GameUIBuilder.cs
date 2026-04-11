using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Editor tool: builds the full game UI and saves it as a prefab.
/// Run via menu: Tools → Kings New Clothes → Build UI Prefab
/// After building, the art team can open Assets/Prefabs/GameUI.prefab
/// and swap sprites/colours in the Inspector.
/// </summary>
public static class GameUIBuilder
{
#if UNITY_EDITOR
    [MenuItem("Tools/Kings New Clothes/Build UI Prefab")]
    public static void BuildUIPrefab()
    {
        // Create root
        var root = new GameObject("GameUI");

        // Canvas
        var canvasGO = new GameObject("Canvas");
        canvasGO.transform.SetParent(root.transform, false);
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        // EventSystem
        var esGO = new GameObject("EventSystem");
        esGO.transform.SetParent(root.transform, false);
        esGO.AddComponent<EventSystem>();
        esGO.AddComponent<InputSystemUIInputModule>();

        // ── STAGE PANEL ───────────────────────────────────────────────────
        var stagePanel = CreateFull(canvasGO, "StagePanel");
        var stageBg    = stagePanel.AddComponent<Image>();
        stageBg.color  = new Color(0.06f, 0.03f, 0.10f);

        // Floor
        var floor = CreateRect(stagePanel, "Floor", new Vector2(0, -350), new Vector2(1920, 220));
        SetImage(floor, new Color(0.25f, 0.15f, 0.05f));
        var floorLine = CreateRect(stagePanel, "FloorLine", new Vector2(0, -245), new Vector2(1920, 12));
        SetImage(floorLine, new Color(0.55f, 0.38f, 0.10f));

        // ── KING (short, fat, on stool) ───────────────────────────────────
        var kingRoot = new GameObject("King");
        kingRoot.transform.SetParent(stagePanel.transform, false);
        kingRoot.AddComponent<RectTransform>();

        var stool    = CreateRect(stagePanel, "Stool",    new Vector2(0, -270), new Vector2(160, 60));
        SetImage(stool, new Color(0.35f, 0.2f, 0.05f));
        var stoolTop = CreateRect(stagePanel, "StoolTop", new Vector2(0, -245), new Vector2(180, 18));
        SetImage(stoolTop, new Color(0.5f, 0.3f, 0.08f));

        var skin = new Color(0.95f, 0.75f, 0.55f);
        var body    = CreateRect(stagePanel, "KingBody",  new Vector2(0, -110), new Vector2(220, 200)); SetImage(body, skin);
        var belly   = CreateRect(stagePanel, "KingBelly", new Vector2(0, -130), new Vector2(200, 160)); SetImage(belly, new Color(1f, 0.8f, 0.62f));
        var legL    = CreateRect(stagePanel, "LegL",      new Vector2(-55, -248), new Vector2(80, 60));  SetImage(legL, skin);
        var legR    = CreateRect(stagePanel, "LegR",      new Vector2(55, -248),  new Vector2(80, 60));  SetImage(legR, skin);
        var armL    = CreateRect(stagePanel, "ArmL",      new Vector2(-165, -80), new Vector2(110, 80)); SetImage(armL, skin);
        var armR    = CreateRect(stagePanel, "ArmR",      new Vector2(165, -80),  new Vector2(110, 80)); SetImage(armR, skin);
        var head    = CreateRect(stagePanel, "KingHead",  new Vector2(0, 60),     new Vector2(200, 190));SetImage(head, skin);
        var eyeL    = CreateRect(stagePanel, "EyeL",      new Vector2(-45, 70),   new Vector2(35, 35));  SetImage(eyeL, new Color(0.1f, 0.1f, 0.3f));
        var eyeR    = CreateRect(stagePanel, "EyeR",      new Vector2(45, 70),    new Vector2(35, 35));  SetImage(eyeR, new Color(0.1f, 0.1f, 0.3f));
        CreateRect(eyeL, "Shine", new Vector2(8, 8), new Vector2(10, 10)).AddComponent<Image>().color = Color.white;
        CreateRect(eyeR, "Shine", new Vector2(8, 8), new Vector2(10, 10)).AddComponent<Image>().color = Color.white;
        var smile   = CreateRect(stagePanel, "Smile",      new Vector2(0, 15),    new Vector2(80, 18));  SetImage(smile, new Color(0.6f, 0.25f, 0.15f));
        var moust   = CreateRect(stagePanel, "Moustache",  new Vector2(0, 35),    new Vector2(100, 20)); SetImage(moust, new Color(0.3f, 0.15f, 0.05f));
        var blushL  = CreateRect(stagePanel, "BlushL",     new Vector2(-70, 40),  new Vector2(45, 25));  SetImage(blushL, new Color(1f, 0.5f, 0.5f, 0.55f));
        var blushR  = CreateRect(stagePanel, "BlushR",     new Vector2(70, 40),   new Vector2(45, 25));  SetImage(blushR, new Color(1f, 0.5f, 0.5f, 0.55f));

        // Crown (above curtains)
        var crownBase = CreateRect(stagePanel, "CrownBase", new Vector2(0, 180),   new Vector2(200, 35)); SetImage(crownBase, new Color(1f, 0.85f, 0.1f));
        var crownL_   = CreateRect(stagePanel, "CrownL",    new Vector2(-65, 210), new Vector2(45, 60));  SetImage(crownL_,   new Color(1f, 0.85f, 0.1f));
        var crownC_   = CreateRect(stagePanel, "CrownC",    new Vector2(0, 225),   new Vector2(45, 75));  SetImage(crownC_,   new Color(1f, 0.85f, 0.1f));
        var crownR_   = CreateRect(stagePanel, "CrownR",    new Vector2(65, 210),  new Vector2(45, 60));  SetImage(crownR_,   new Color(1f, 0.85f, 0.1f));
        CreateRect(crownL_, "Jewel", Vector2.zero, new Vector2(20, 20)).AddComponent<Image>().color = new Color(0.8f, 0.1f, 0.1f);
        CreateRect(crownC_, "Jewel", Vector2.zero, new Vector2(22, 22)).AddComponent<Image>().color = new Color(0.1f, 0.4f, 0.9f);
        CreateRect(crownR_, "Jewel", Vector2.zero, new Vector2(20, 20)).AddComponent<Image>().color = new Color(0.1f, 0.8f, 0.2f);

        // ── CURTAINS ──────────────────────────────────────────────────────
        var curtainL = CreateRect(stagePanel, "CurtainLeft",  new Vector2(-560, 0), new Vector2(1000, 900)); SetImage(curtainL, new Color(0.55f, 0.05f, 0.08f));
        for (int i = 0; i < 4; i++) { var f = CreateRect(curtainL, $"Fold{i}", new Vector2(-350 + i * 120, 0), new Vector2(20, 900)); SetImage(f, new Color(0.4f, 0.03f, 0.06f, 0.7f)); }
        SetImage(CreateRect(curtainL, "GoldTrim", new Vector2(480, 0), new Vector2(18, 900)), new Color(1f, 0.82f, 0.1f));

        var curtainR = CreateRect(stagePanel, "CurtainRight", new Vector2(560, 0),  new Vector2(1000, 900)); SetImage(curtainR, new Color(0.55f, 0.05f, 0.08f));
        for (int i = 0; i < 4; i++) { var f = CreateRect(curtainR, $"Fold{i}", new Vector2(350 - i * 120, 0), new Vector2(20, 900)); SetImage(f, new Color(0.4f, 0.03f, 0.06f, 0.7f)); }
        SetImage(CreateRect(curtainR, "GoldTrim", new Vector2(-480, 0), new Vector2(18, 900)), new Color(1f, 0.82f, 0.1f));

        // ── ANSWER TRACKER ────────────────────────────────────────────────
        var trackerBg = CreateRect(stagePanel, "TrackerBg", new Vector2(-780, 200), new Vector2(280, 220));
        SetImage(trackerBg, new Color(0.08f, 0.05f, 0.15f, 0.9f));
        AddTMP(trackerBg, "TrackerTitle",    "YOUR GUESSES", 18, FontStyles.Bold,   new Vector2(0, 78),  new Vector2(260, 40)).color = new Color(1f, 0.85f, 0.3f);
        AddTMP(trackerBg, "TrackerClothing", "Garment: ?",   22, FontStyles.Normal, new Vector2(0, 30),  new Vector2(250, 40));
        AddTMP(trackerBg, "TrackerColor",    "Color: ?",     22, FontStyles.Normal, new Vector2(0, -15), new Vector2(250, 40));
        AddTMP(trackerBg, "TrackerMaterial", "Material: ?",  22, FontStyles.Normal, new Vector2(0, -60), new Vector2(250, 40));

        // ── SPEECH BUBBLE ─────────────────────────────────────────────────
        var bubblePanel = CreateRect(stagePanel, "SpeechBubble", new Vector2(420, 240), new Vector2(680, 220));
        SetImage(bubblePanel, new Color(0.97f, 0.95f, 0.88f));
        SetImage(CreateRect(bubblePanel, "BubbleTail", new Vector2(-280, -80), new Vector2(60, 60)), new Color(0.97f, 0.95f, 0.88f));
        var riddleTMP = CreateTMPFull(bubblePanel, "RiddleText", "...", 26, FontStyles.Italic);
        riddleTMP.color = new Color(0.1f, 0.05f, 0.15f);

        // Category label (top)
        var catLabel = CreateRect(stagePanel, "CategoryLabel", new Vector2(0, 480), new Vector2(900, 70));
        var catTMP = catLabel.AddComponent<TextMeshProUGUI>();
        catTMP.text = ""; catTMP.fontSize = 38; catTMP.fontStyle = FontStyles.Bold;
        catTMP.alignment = TextAlignmentOptions.Center; catTMP.color = new Color(1f, 0.85f, 0.3f);

        // ── OPTIONS ROW ───────────────────────────────────────────────────
        var bottomPanel = CreateRect(canvasGO, "OptionsRow", new Vector2(0, -460), new Vector2(1800, 110));
        var hlg = bottomPanel.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 16; hlg.childForceExpandWidth = true; hlg.childForceExpandHeight = true;
        hlg.childAlignment = TextAnchor.MiddleCenter; hlg.padding = new RectOffset(10, 10, 5, 5);

        // ── OPTION BUTTON PREFAB ──────────────────────────────────────────
        var optBtnGO = new GameObject("OptionButtonPrefab");
        optBtnGO.transform.SetParent(canvasGO.transform, false);
        optBtnGO.SetActive(false);
        optBtnGO.AddComponent<RectTransform>().sizeDelta = new Vector2(300, 90);
        SetImage(optBtnGO, new Color(0.28f, 0.14f, 0.44f));
        var optBtn = optBtnGO.AddComponent<Button>();
        var optLE  = optBtnGO.AddComponent<LayoutElement>();
        optLE.preferredWidth = 300; optLE.preferredHeight = 90;
        var optLbl = CreateTMPFull(optBtnGO, "Label", "Option", 26, FontStyles.Bold);
        optLbl.color = Color.white;
        StyleButton(optBtn, new Color(0.28f, 0.14f, 0.44f));

        // ── OVERLAY PANELS ────────────────────────────────────────────────

        // Intro
        var introPanel = CreateFull(canvasGO, "IntroPanel");
        SetImage(introPanel, new Color(0.06f, 0.03f, 0.10f, 0.97f));
        var introTitle = AddTMP(introPanel, "Title", "FASHION ROYAL", 90, FontStyles.Bold, new Vector2(0, 150), new Vector2(1400, 200));
        // Team name at the bottom
        var teamLabel = AddTMP(introPanel, "TeamName", "by Invisible Tailors", 24, FontStyles.Italic, new Vector2(0, -470), new Vector2(800, 50));
        teamLabel.color = new Color(0.7f, 0.6f, 0.9f);
        introTitle.color = new Color(1f, 0.85f, 0.3f);
        var introBody = AddTMP(introPanel, "Body", "", 30, FontStyles.Normal, new Vector2(0, -80), new Vector2(1000, 350));
        introBody.alignment = TextAlignmentOptions.Center; introBody.color = new Color(0.9f, 0.85f, 1f);
        var introBtn = CreateButton(introPanel, "StartButton", "ENTER THE ROYAL COURT", new Vector2(0, -340), new Vector2(560, 90), new Color(0.45f, 0.22f, 0.65f));

        // Loading
        var loadPanel = CreateFull(canvasGO, "LoadingPanel");
        SetImage(loadPanel, new Color(0.06f, 0.03f, 0.10f, 0.85f));
        var loadTxt = AddTMP(loadPanel, "LoadingText", "The King is composing his riddle...", 38, FontStyles.Italic, Vector2.zero, new Vector2(900, 80));
        loadTxt.color = new Color(0.9f, 0.8f, 1f);

        // Reaction
        var reactionPanel = CreateFull(canvasGO, "ReactionPanel");
        SetImage(reactionPanel, new Color(0.05f, 0.2f, 0.07f, 0.93f));
        var reactionTxt = AddTMP(reactionPanel, "ReactionText", "", 48, FontStyles.Bold, Vector2.zero, new Vector2(1200, 500));
        reactionTxt.alignment = TextAlignmentOptions.Center; reactionTxt.color = Color.white;

        // Reveal (transparent)
        var revealPanel = CreateFull(canvasGO, "RevealPanel");
        SetImage(revealPanel, new Color(0, 0, 0, 0));
        var revealBubble = CreateRect(revealPanel, "RevealBubble", new Vector2(380, 260), new Vector2(700, 200));
        SetImage(revealBubble, new Color(0.97f, 0.95f, 0.88f));
        SetImage(CreateRect(revealBubble, "Tail", new Vector2(-290, -75), new Vector2(55, 55)), new Color(0.97f, 0.95f, 0.88f));
        var revealTxt = CreateTMPFull(revealBubble, "RevealText", "", 26, FontStyles.Normal);
        revealTxt.color = new Color(0.1f, 0.05f, 0.15f);
        var revealBtn = CreateButton(revealPanel, "ContinueButton", "Continue →", new Vector2(0, -460), new Vector2(320, 65), new Color(0.4f, 0.2f, 0.6f));

        // Judgment (transparent)
        var judgPanel = CreateFull(canvasGO, "JudgmentPanel");
        SetImage(judgPanel, new Color(0, 0, 0, 0));
        var judgBubble = CreateRect(judgPanel, "JudgBubble", new Vector2(380, 260), new Vector2(700, 200));
        SetImage(judgBubble, new Color(0.97f, 0.95f, 0.88f));
        SetImage(CreateRect(judgBubble, "Tail", new Vector2(-290, -75), new Vector2(55, 55)), new Color(0.97f, 0.95f, 0.88f));
        var judgTxt = CreateTMPFull(judgBubble, "JudgText", "", 26, FontStyles.Normal);
        judgTxt.color = new Color(0.1f, 0.05f, 0.15f);
        var neutralBtn = new Color(0.35f, 0.18f, 0.55f); // same purple for both
        var flatterBtn = CreateButton(judgPanel, "FlatterButton", "\"Yes Your Majesty!\"",              new Vector2(-320, -460), new Vector2(520, 65), neutralBtn);
        var truthBtn   = CreateButton(judgPanel, "TruthButton",   "\"...Why are you wearing nothing?\"", new Vector2(280, -460),  new Vector2(460, 65), neutralBtn);

        // Win
        var winPanel = CreateFull(canvasGO, "WinPanel");
        SetImage(winPanel, new Color(0.03f, 0.12f, 0.05f, 0.96f));
        var winTitle = AddTMP(winPanel, "Title", "YOU LIVE!", 100, FontStyles.Bold, new Vector2(0, 280), new Vector2(800, 140));
        winTitle.color = new Color(0.3f, 1f, 0.45f);
        var winTxt = AddTMP(winPanel, "WinText", "", 32, FontStyles.Normal, new Vector2(0, 10), new Vector2(1000, 380));
        winTxt.alignment = TextAlignmentOptions.Center;
        var winBtn = CreateButton(winPanel, "PlayAgainButton", "Serve the King Again", new Vector2(0, -310), new Vector2(440, 85), new Color(0.1f, 0.45f, 0.18f));

        // Death
        var deathPanel = CreateFull(canvasGO, "DeathPanel");
        SetImage(deathPanel, new Color(0.14f, 0.02f, 0.02f, 0.97f));
        var deathTitle = AddTMP(deathPanel, "Title", "OFF WITH YOUR HEAD!", 72, FontStyles.Bold, new Vector2(0, 280), new Vector2(1100, 120));
        deathTitle.color = new Color(1f, 0.18f, 0.18f);
        var deathTxt = AddTMP(deathPanel, "DeathText", "", 32, FontStyles.Normal, new Vector2(0, 10), new Vector2(1000, 380));
        deathTxt.alignment = TextAlignmentOptions.Center;
        var deathBtn = CreateButton(deathPanel, "PlayAgainButton", "Try Again (Lie This Time)", new Vector2(0, -310), new Vector2(460, 85), new Color(0.5f, 0.08f, 0.08f));

        // ── WIRE UI MANAGER ───────────────────────────────────────────────
        var uiGO = new GameObject("UIManager");
        uiGO.transform.SetParent(root.transform, false);
        var ui = uiGO.AddComponent<UIManager>();

        ui.stagePanel        = stagePanel;
        ui.curtainAnimator   = stagePanel.AddComponent<CurtainAnimator>();
        ui.curtainAnimator.curtainLeft  = curtainL.GetComponent<RectTransform>();
        ui.curtainAnimator.curtainRight = curtainR.GetComponent<RectTransform>();

        ui.riddleText        = riddleTMP;
        ui.categoryLabel     = catTMP;
        ui.optionsContainer  = bottomPanel.transform;
        ui.optionButtonPrefab = optBtn;

        ui.trackerClothing   = trackerBg.transform.Find("TrackerClothing").GetComponent<TextMeshProUGUI>();
        ui.trackerColor      = trackerBg.transform.Find("TrackerColor").GetComponent<TextMeshProUGUI>();
        ui.trackerMaterial   = trackerBg.transform.Find("TrackerMaterial").GetComponent<TextMeshProUGUI>();

        ui.reactionPanel     = reactionPanel;
        ui.reactionText      = reactionTxt;
        ui.reactionBg        = reactionPanel.GetComponent<Image>();

        ui.introPanel        = introPanel;
        ui.introText         = introBody;
        ui.introStartButton  = introBtn;

        ui.loadingPanel      = loadPanel;

        ui.revealPanel           = revealPanel;
        ui.revealText            = revealTxt;
        ui.revealContinueButton  = revealBtn;

        ui.finalJudgmentPanel = judgPanel;
        ui.finalText          = judgTxt;
        ui.flatterButton      = flatterBtn;
        ui.truthButton        = truthBtn;

        ui.winPanel           = winPanel;
        ui.winText            = winTxt;
        ui.winPlayAgainButton = winBtn;

        ui.deathPanel           = deathPanel;
        ui.deathText            = deathTxt;
        ui.deathPlayAgainButton = deathBtn;

        // Proud pose callback
        var armLRT = armL.GetComponent<RectTransform>();
        var armRRT = armR.GetComponent<RectTransform>();
        var bodyRT = body.GetComponent<RectTransform>();
        ui.kingPoseProud = (proud) =>
        {
            if (proud)
            {
                armLRT.anchoredPosition = new Vector2(-165, 20);  armLRT.localRotation = Quaternion.Euler(0, 0, 45f);
                armRRT.anchoredPosition = new Vector2(165, 20);   armRRT.localRotation = Quaternion.Euler(0, 0, -45f);
                bodyRT.sizeDelta = new Vector2(250, 200);
            }
            else
            {
                armLRT.anchoredPosition = new Vector2(-165, -80); armLRT.localRotation = Quaternion.identity;
                armRRT.anchoredPosition = new Vector2(165, -80);  armRRT.localRotation = Quaternion.identity;
                bodyRT.sizeDelta = new Vector2(220, 200);
            }
        };

        // Hide all overlays
        introPanel.SetActive(false); loadPanel.SetActive(false); reactionPanel.SetActive(false);
        revealPanel.SetActive(false); judgPanel.SetActive(false); winPanel.SetActive(false); deathPanel.SetActive(false);

        // ── SAVE PREFAB ───────────────────────────────────────────────────
        string prefabPath = "Assets/Prefabs/GameUI.prefab";
        PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        Object.DestroyImmediate(root);
        AssetDatabase.Refresh();

        Debug.Log($"[GameUIBuilder] Prefab saved to {prefabPath}");
        EditorUtility.DisplayDialog("Done!", $"GameUI prefab saved to:\n{prefabPath}\n\nDrop it into your scene and hit Play.", "OK");
    }

    // ── HELPERS ───────────────────────────────────────────────────────────

    static GameObject CreateFull(GameObject parent, string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        return go;
    }

    static GameObject CreateRect(GameObject parent, string name, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = pos; rt.sizeDelta = size;
        return go;
    }

    static Image SetImage(GameObject go, Color color)
    {
        var img = go.GetComponent<Image>() ?? go.AddComponent<Image>();
        img.color = color; return img;
    }

    static TextMeshProUGUI AddTMP(GameObject parent, string name, string text, int size, FontStyles style, Vector2 pos, Vector2 sz)
    {
        var go = CreateRect(parent, name, pos, sz);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text; tmp.fontSize = size; tmp.fontStyle = style;
        tmp.alignment = TextAlignmentOptions.Center; tmp.color = Color.white; tmp.enableWordWrapping = true;
        return tmp;
    }

    static TextMeshProUGUI CreateTMPFull(GameObject parent, string name, string text, int size, FontStyles style)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(30, 20); rt.offsetMax = new Vector2(-30, -20);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text; tmp.fontSize = size; tmp.fontStyle = style;
        tmp.alignment = TextAlignmentOptions.Center; tmp.color = Color.white; tmp.enableWordWrapping = true;
        return tmp;
    }

    static Button CreateButton(GameObject parent, string name, string label, Vector2 pos, Vector2 size, Color color)
    {
        var go = CreateRect(parent, name, pos, size);
        SetImage(go, color);
        var btn = go.AddComponent<Button>();
        StyleButton(btn, color);
        var lGO = new GameObject("Label");
        lGO.transform.SetParent(go.transform, false);
        var lRT = lGO.AddComponent<RectTransform>();
        lRT.anchorMin = Vector2.zero; lRT.anchorMax = Vector2.one; lRT.sizeDelta = Vector2.zero;
        var tmp = lGO.AddComponent<TextMeshProUGUI>();
        tmp.text = label; tmp.fontSize = 24; tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center; tmp.color = Color.white; tmp.enableWordWrapping = true;
        return btn;
    }

    static void StyleButton(Button btn, Color color)
    {
        var cb = btn.colors;
        cb.highlightedColor = color * 1.3f; cb.pressedColor = color * 0.7f;
        btn.colors = cb;
    }
#endif
}
