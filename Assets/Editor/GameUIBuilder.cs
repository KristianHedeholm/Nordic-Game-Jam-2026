using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Rebuilds the full GameUI prefab with all art, positions and logic wired up.
/// Run via: Tools → Fashion Royal → Rebuild GameUI Prefab
/// </summary>
public static class GameUIBuilder
{
#if UNITY_EDITOR
    [MenuItem("Tools/Fashion Royal/Rebuild GameUI Prefab")]
    public static void Build()
    {
        var root = new GameObject("GameUI");

        // ── CANVAS ────────────────────────────────────────────────────────
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

        // ── EVENT SYSTEM ──────────────────────────────────────────────────
        var esGO = new GameObject("EventSystem");
        esGO.transform.SetParent(root.transform, false);
        esGO.AddComponent<EventSystem>();
        esGO.AddComponent<InputSystemUIInputModule>();

        // ── STAGE PANEL ───────────────────────────────────────────────────
        var stage = CF(canvasGO, "StagePanel");
        SetImg(stage, null, new Color(0.06f, 0.03f, 0.10f));

        // Layer 1: Bedroom background
        SetImg(CF(stage, "BG_Bedroom"), Spr("BG_Bedroom"), Color.white, false);

        // Layer 2: Player bunny (bottom left)
        var pb = CR(stage, "PlayerBunny", new Vector2(193f, -252.9f), new Vector2(512.8f, 717.98f));
        SetImg(pb, Spr("Player_Bunny"), Color.white, true);

        // Layer 3: Naked king (hidden until reveal)
        var nk = CR(stage, "KingNaked", new Vector2(483.2f, 134.9f), new Vector2(380, 600));
        SetImg(nk, Spr("King_Naked"), Color.white, true);
        nk.SetActive(false);

        // Layer 4: Left curtain
        var clGO = CR(stage, "CurtainLeft", new Vector2(193f, 164.1f), new Vector2(650, 750));
        var clImg = SetImg(clGO, Spr("Curtain_Left"), Color.white, false);
        var clRT = clGO.GetComponent<RectTransform>();
        clRT.anchorMin = clRT.anchorMax = new Vector2(0.5f, 0.5f);

        // Layer 5: Right curtain
        var crGO = CR(stage, "CurtainRight", new Vector2(635.9f, 162.5f), new Vector2(650, 750));
        var crImg = SetImg(crGO, Spr("Curtain_Right"), Color.white, false);
        var crRT = crGO.GetComponent<RectTransform>();
        crRT.anchorMin = crRT.anchorMax = new Vector2(0.5f, 0.5f);

        // Layer 6: King silhouette (semi-transparent, hidden on reveal)
        var sil = CR(stage, "KingSilhouette", new Vector2(483.2f, 134.9f), new Vector2(380, 600));
        SetImg(sil, Spr("King_Silhouette"), new Color(1f, 1f, 1f, 0.55f), true);

        // Curtain animator
        var ca = stage.AddComponent<CurtainAnimator>();
        ca.curtainLeft  = clRT;
        ca.curtainRight = crRT;

        // Placeholder refs for proud pose
        CR(stage, "ArmL",    new Vector2(0,0), new Vector2(1,1));
        CR(stage, "ArmR",    new Vector2(0,0), new Vector2(1,1));
        CR(stage, "KingBody",new Vector2(0,0), new Vector2(1,1));

        // Narrator label
        var narGO = CR(stage, "NarratorLabel", new Vector2(-250, 175), new Vector2(700, 70));
        var narTMP = narGO.AddComponent<TextMeshProUGUI>();
        narTMP.text=""; narTMP.fontSize=24; narTMP.fontStyle=FontStyles.Italic;
        narTMP.alignment=TextAlignmentOptions.Center; narTMP.color=new Color(0.95f,0.9f,1f); narTMP.enableWordWrapping=true;

        // Speech bubble (left of king)
        var bubble = CR(stage, "SpeechBubble", new Vector2(-250, 300), new Vector2(700, 220));
        SetImg(bubble, Spr("Speech_Bubble_New"), Color.white, false);
        var riddleGO = new GameObject("RiddleText"); riddleGO.transform.SetParent(bubble.transform, false);
        var riddleRT = riddleGO.AddComponent<RectTransform>();
        riddleRT.anchorMin=Vector2.zero; riddleRT.anchorMax=Vector2.one;
        riddleRT.offsetMin=new Vector2(30,20); riddleRT.offsetMax=new Vector2(-30,-20);
        var riddleTMP = riddleGO.AddComponent<TextMeshProUGUI>();
        riddleTMP.text="..."; riddleTMP.fontSize=26; riddleTMP.fontStyle=FontStyles.Italic;
        riddleTMP.color=new Color(0.1f,0.05f,0.15f); riddleTMP.alignment=TextAlignmentOptions.Center; riddleTMP.enableWordWrapping=true;

        // Category label (hidden)
        var catGO = CR(stage, "CategoryLabel", new Vector2(0, 520), new Vector2(900, 70));
        var catTMP = catGO.AddComponent<TextMeshProUGUI>();
        catTMP.text=""; catTMP.fontSize=38; catTMP.fontStyle=FontStyles.Bold;
        catTMP.alignment=TextAlignmentOptions.Center; catTMP.color=new Color(0,0,0,0);

        // ── LEFT PANEL: Tag drop zones ─────────────────────────────────────
        string[] holderSprites = { "Place_Garment_tag_holder", "Place_Colour_tag_holder", "Place_Material_tag_holder" };
        string[] holderCats    = { "Clothing", "Color", "Material" };
        var dropZones   = new TagDropZone[3];
        var trackerTMPs = new TextMeshProUGUI[3];

        for (int i = 0; i < 3; i++)
        {
            var h = CR(stage, $"DropZone_{holderCats[i]}", new Vector2(-750, 180 - i * 220), new Vector2(260, 200));
            SetImg(h, Spr(holderSprites[i]), Color.white, true);
            var aGO = new GameObject("AnswerLabel"); aGO.transform.SetParent(h.transform, false);
            var aRT = aGO.AddComponent<RectTransform>(); aRT.anchorMin=Vector2.zero; aRT.anchorMax=Vector2.one; aRT.offsetMin=new Vector2(10,10); aRT.offsetMax=new Vector2(-10,-10);
            var aTMP = aGO.AddComponent<TextMeshProUGUI>();
            aTMP.text=""; aTMP.fontSize=22; aTMP.fontStyle=FontStyles.Bold;
            aTMP.alignment=TextAlignmentOptions.Center; aTMP.color=new Color(0.1f,0.05f,0.2f); aTMP.enableWordWrapping=true;
            var dz = h.AddComponent<TagDropZone>(); dz.category=holderCats[i]; dz.answerLabel=aTMP;
            dropZones[i]=dz; trackerTMPs[i]=aTMP;
        }

        // ── OPTIONS ROW ───────────────────────────────────────────────────
        var optRow = CR(canvasGO, "OptionsRow", new Vector2(200, -420), new Vector2(1400, 220));
        var optHLG = optRow.AddComponent<HorizontalLayoutGroup>();
        optHLG.spacing=16; optHLG.childForceExpandWidth=true; optHLG.childForceExpandHeight=true;
        optHLG.childAlignment=TextAnchor.MiddleCenter; optHLG.padding=new RectOffset(10,10,5,5);

        // ── DRAGGABLE TAG PREFAB ──────────────────────────────────────────
        var tagPrefab = new GameObject("DraggableTagPrefab");
        tagPrefab.transform.SetParent(canvasGO.transform, false);
        tagPrefab.SetActive(false);
        tagPrefab.AddComponent<RectTransform>().sizeDelta = new Vector2(190, 60);
        SetImg(tagPrefab, null, new Color(0.55f,0.55f,0.58f));
        var tLbl = new GameObject("Label"); tLbl.transform.SetParent(tagPrefab.transform,false);
        var tRT=tLbl.AddComponent<RectTransform>(); tRT.anchorMin=Vector2.zero; tRT.anchorMax=Vector2.one; tRT.sizeDelta=Vector2.zero;
        var tTMP=tLbl.AddComponent<TextMeshProUGUI>();
        tTMP.text=""; tTMP.fontSize=22; tTMP.fontStyle=FontStyles.Bold;
        tTMP.enableAutoSizing=true; tTMP.fontSizeMin=12; tTMP.fontSizeMax=22;
        tTMP.alignment=TextAlignmentOptions.Center; tTMP.color=Color.white; tTMP.enableWordWrapping=false;
        tagPrefab.AddComponent<DraggableTag>();
        tagPrefab.AddComponent<CanvasGroup>().blocksRaycasts=true;
        var tagBtn = tagPrefab.AddComponent<Button>();

        // ── OVERLAY PANELS ────────────────────────────────────────────────

        // Intro
        var introPanel = CF(canvasGO, "IntroPanel");
        SetImg(introPanel, Spr("Title_Screen_v2") ?? Spr("Logo_Fashion_Royal"), Color.white, false);
        var introBody = TMP(introPanel, "Body", "", 38, FontStyles.Bold, new Vector2(0,80), new Vector2(900,400));
        introBody.color = Color.white;
        var introBtn = Btn(introPanel, "StartButton", "START", new Vector2(0,-340), new Vector2(320,90), new Color(0.85f,0.6f,0.05f));
        introBtn.GetComponentInChildren<TMP_Text>().color = new Color(0.1f,0.05f,0f);
        introPanel.SetActive(false);

        // Loading
        var loadPanel = CF(canvasGO, "LoadingPanel");
        SetImg(loadPanel, null, new Color(0.06f,0.03f,0.10f,0.85f));
        TMP(loadPanel,"LoadTxt","",38,FontStyles.Italic,Vector2.zero,new Vector2(900,80)).color=new Color(0.9f,0.8f,1f);
        loadPanel.SetActive(false);

        // Reaction
        var reactionPanel = CF(canvasGO, "ReactionPanel");
        SetImg(reactionPanel, null, new Color(0.05f,0.2f,0.07f,0.93f));
        var reactionTMP = TMP(reactionPanel,"ReactionTxt","",48,FontStyles.Bold,Vector2.zero,new Vector2(1200,500));
        reactionTMP.alignment=TextAlignmentOptions.Center; reactionTMP.enableWordWrapping=true;
        reactionPanel.SetActive(false);

        // Reveal (transparent overlay)
        var revealPanel = CF(canvasGO, "RevealPanel");
        SetImg(revealPanel, null, new Color(0,0,0,0));
        var revBubble = CR(revealPanel,"RevealBubble",new Vector2(380,260),new Vector2(700,200));
        SetImg(revBubble, Spr("Speech_Bubble_New"), Color.white, false);
        var revTGO=new GameObject("RevealText"); revTGO.transform.SetParent(revBubble.transform,false);
        var revTRT=revTGO.AddComponent<RectTransform>(); revTRT.anchorMin=Vector2.zero; revTRT.anchorMax=Vector2.one; revTRT.offsetMin=new Vector2(30,20); revTRT.offsetMax=new Vector2(-30,-20);
        var revTMP=revTGO.AddComponent<TextMeshProUGUI>(); revTMP.text=""; revTMP.fontSize=26; revTMP.alignment=TextAlignmentOptions.Center; revTMP.color=new Color(0.1f,0.05f,0.15f); revTMP.enableWordWrapping=true;
        var revBtn = Btn(revealPanel,"ContinueBtn","Continue →",new Vector2(0,-460),new Vector2(240,55),new Color(0.4f,0.2f,0.6f));
        revealPanel.SetActive(false);

        // Judgment (transparent overlay)
        var judgPanel = CF(canvasGO, "JudgmentPanel");
        SetImg(judgPanel, null, new Color(0,0,0,0));
        var jBubble = CR(judgPanel,"JudgBubble",new Vector2(380,260),new Vector2(700,200));
        SetImg(jBubble, Spr("Speech_Bubble_New"), Color.white, false);
        var jTGO=new GameObject("JudgText"); jTGO.transform.SetParent(jBubble.transform,false);
        var jTRT=jTGO.AddComponent<RectTransform>(); jTRT.anchorMin=Vector2.zero; jTRT.anchorMax=Vector2.one; jTRT.offsetMin=new Vector2(30,20); jTRT.offsetMax=new Vector2(-30,-20);
        var jTMP=jTGO.AddComponent<TextMeshProUGUI>(); jTMP.text=""; jTMP.fontSize=26; jTMP.alignment=TextAlignmentOptions.Center; jTMP.color=new Color(0.1f,0.05f,0.15f); jTMP.enableWordWrapping=true;
        var neutral=new Color(0.35f,0.18f,0.55f);
        var flatterBtn = Btn(judgPanel,"FlatterBtn","\"Yes Your Majesty!\"",new Vector2(-300,-460),new Vector2(420,55),neutral);
        var truthBtn   = Btn(judgPanel,"TruthBtn","\"...Why are you wearing nothing?\"",new Vector2(270,-460),new Vector2(360,55),neutral);
        judgPanel.SetActive(false);

        // Win
        var winPanel = CF(canvasGO, "WinPanel");
        SetImg(winPanel, null, new Color(0.03f,0.12f,0.05f,0.96f));
        TMP(winPanel,"Title","YOU LIVE!",100,FontStyles.Bold,new Vector2(0,280),new Vector2(800,140)).color=new Color(0.3f,1f,0.45f);
        var winTMP = TMP(winPanel,"WinText","",32,FontStyles.Normal,new Vector2(0,10),new Vector2(1000,380)); winTMP.alignment=TextAlignmentOptions.Center;
        var winBtn = Btn(winPanel,"PlayAgainBtn","Serve the King Again",new Vector2(0,-310),new Vector2(340,65),new Color(0.1f,0.45f,0.18f));
        winPanel.SetActive(false);

        // Death
        var deathPanel = CF(canvasGO, "DeathPanel");
        SetImg(deathPanel, Spr("Death_Screen"), Color.white, false);
        TMP(deathPanel,"Title","OFF WITH YOUR HEAD!",72,FontStyles.Bold,new Vector2(0,280),new Vector2(1100,120)).color=new Color(1f,0.18f,0.18f);
        var deathTMP = TMP(deathPanel,"DeathText","",32,FontStyles.Normal,new Vector2(0,10),new Vector2(1000,380)); deathTMP.alignment=TextAlignmentOptions.Center;
        var deathBtn = Btn(deathPanel,"PlayAgainBtn","Try Again (Lie This Time)",new Vector2(0,-430),new Vector2(460,65),new Color(0.5f,0.08f,0.08f));
        deathPanel.SetActive(false);

        // ── UI MANAGER ────────────────────────────────────────────────────
        var uiGO = new GameObject("UIManager");
        uiGO.transform.SetParent(root.transform, false);
        var ui = uiGO.AddComponent<UIManager>();

        ui.stagePanel        = stage;
        ui.curtainAnimator   = ca;
        ui.riddleText        = riddleTMP;
        ui.categoryLabel     = catTMP;
        ui.optionsContainer  = optRow.transform;
        ui.optionButtonPrefab = tagBtn;
        ui.draggableTagPrefab = tagPrefab;
        ui.narratorLabel     = narTMP;
        ui.nakedKingGO       = nk;
        ui.silhouetteGO      = sil;

        ui.dropZoneClothing  = dropZones[0];
        ui.dropZoneColor     = dropZones[1];
        ui.dropZoneMaterial  = dropZones[2];
        ui.trackerClothing   = trackerTMPs[0];
        ui.trackerColor      = trackerTMPs[1];
        ui.trackerMaterial   = trackerTMPs[2];

        ui.reactionPanel     = reactionPanel;
        ui.reactionText      = reactionTMP;
        ui.reactionBg        = reactionPanel.GetComponent<Image>();

        ui.introPanel        = introPanel;
        ui.introText         = introBody;
        ui.introStartButton  = introBtn;
        ui.loadingPanel      = loadPanel;
        ui.revealPanel       = revealPanel;
        ui.revealText        = revTMP;
        ui.revealContinueButton = revBtn;
        ui.finalJudgmentPanel = judgPanel;
        ui.finalText         = jTMP;
        ui.flatterButton     = flatterBtn;
        ui.truthButton       = truthBtn;
        ui.winPanel          = winPanel;
        ui.winText           = winTMP;
        ui.winPlayAgainButton = winBtn;
        ui.deathPanel        = deathPanel;
        ui.deathText         = deathTMP;
        ui.deathPlayAgainButton = deathBtn;

        // Save prefab
        string path = "Assets/Prefabs/GameUI.prefab";
        PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Done!", $"GameUI prefab rebuilt at:\n{path}\n\nDrag it onto SceneBuilder and hit Play.", "OK");
        Debug.Log("[GameUIBuilder] Prefab rebuilt successfully.");
    }

    // ── HELPERS ───────────────────────────────────────────────────────────

    static Sprite Spr(string name)
    {
        var s = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Resources/Art/{name}.png");
        if (s == null) Debug.LogWarning($"[GameUIBuilder] Sprite not found: {name}");
        return s;
    }

    static GameObject CF(GameObject parent, string name)
    {
        var go = new GameObject(name); go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>(); rt.anchorMin=Vector2.zero; rt.anchorMax=Vector2.one; rt.offsetMin=Vector2.zero; rt.offsetMax=Vector2.zero;
        return go;
    }

    static GameObject CR(GameObject parent, string name, Vector2 pos, Vector2 size)
    {
        var go = new GameObject(name); go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>(); rt.anchoredPosition=pos; rt.sizeDelta=size;
        return go;
    }

    static Image SetImg(GameObject go, Sprite spr, Color color, bool preserveAspect = false)
    {
        var img = go.GetComponent<Image>() ?? go.AddComponent<Image>();
        img.color = color;
        if (spr != null) { img.sprite=spr; img.type=Image.Type.Simple; img.preserveAspect=preserveAspect; }
        return img;
    }

    static TextMeshProUGUI TMP(GameObject parent, string name, string text, int size, FontStyles style, Vector2 pos, Vector2 sz)
    {
        var go = CR(parent, name, pos, sz);
        var tmp = go.AddComponent<TextMeshProUGUI>(); tmp.text=text; tmp.fontSize=size; tmp.fontStyle=style;
        tmp.alignment=TextAlignmentOptions.Center; tmp.color=Color.white; tmp.enableWordWrapping=true; return tmp;
    }

    static Button Btn(GameObject parent, string name, string label, Vector2 pos, Vector2 size, Color color)
    {
        var go = CR(parent, name, pos, size);
        var img = go.AddComponent<Image>(); img.color=color;
        var btn = go.AddComponent<Button>();
        var cb=btn.colors; cb.highlightedColor=color*1.3f; cb.pressedColor=color*0.7f; btn.colors=cb;
        var lGO=new GameObject("Label"); lGO.transform.SetParent(go.transform,false);
        var lRT=lGO.AddComponent<RectTransform>(); lRT.anchorMin=Vector2.zero; lRT.anchorMax=Vector2.one; lRT.sizeDelta=Vector2.zero;
        var tmp=lGO.AddComponent<TextMeshProUGUI>(); tmp.text=label; tmp.fontSize=20; tmp.fontStyle=FontStyles.Bold;
        tmp.alignment=TextAlignmentOptions.Center; tmp.color=Color.white; tmp.enableWordWrapping=true; return btn;
    }
#endif
}
