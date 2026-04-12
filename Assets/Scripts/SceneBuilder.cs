using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Instantiates the GameUI prefab and wires up runtime-only logic.
/// Assign the GameUI prefab in the Inspector.
/// All visual editing is done directly in the prefab — no code changes needed.
/// </summary>
[DefaultExecutionOrder(-100)]
public class SceneBuilder : MonoBehaviour
{
    [Header("Assign GameUI prefab here")]
    public GameObject gameUIPrefab;

    void Awake()
    {
        SetupCamera();
        SetupEventSystem();

        if (gameUIPrefab == null)
        {
            Debug.LogError("[SceneBuilder] gameUIPrefab not assigned! Drag Assets/Prefabs/GameUI onto this field.");
            return;
        }

        var uiInstance = Instantiate(gameUIPrefab);
        uiInstance.name = "GameUI";

        var ui = uiInstance.GetComponentInChildren<UIManager>();
        if (ui == null) { Debug.LogError("[SceneBuilder] UIManager not found in prefab!"); return; }

        // ── RUNTIME-ONLY WIRING ───────────────────────────────────────────
        // Button sprites
        ui.buttonStartSprite = Resources.Load<Sprite>("Art/Button_Start");
        ui.buttonNextSprite  = Resources.Load<Sprite>("Art/Button_Next");

        // King proud pose (lambda — can't live in prefab)
        var armL = FindDeep(uiInstance, "ArmL")?.GetComponent<RectTransform>();
        var armR = FindDeep(uiInstance, "ArmR")?.GetComponent<RectTransform>();
        var body = FindDeep(uiInstance, "KingBody")?.GetComponent<RectTransform>();

        ui.kingPoseProud = (proud) =>
        {
            if (armL == null || armR == null || body == null) return;
            if (proud)
            {
                armL.anchoredPosition = new Vector2(-165, 20);  armL.localRotation = Quaternion.Euler(0, 0, 45f);
                armR.anchoredPosition = new Vector2(165, 20);   armR.localRotation = Quaternion.Euler(0, 0, -45f);
                body.sizeDelta = new Vector2(250, 200);
            }
            else
            {
                armL.anchoredPosition = new Vector2(-165, -80); armL.localRotation = Quaternion.identity;
                armR.anchoredPosition = new Vector2(165, -80);  armR.localRotation = Quaternion.identity;
                body.sizeDelta = new Vector2(220, 200);
            }
        };

        // Naked king / silhouette swap (if present in prefab)
        ui.nakedKingGO  = FindDeep(uiInstance, "KingNaked");
        ui.silhouetteGO = FindDeep(uiInstance, "KingSilhouette");

        // ── GAME SYSTEMS ──────────────────────────────────────────────────
        var mgrGO = new GameObject("GameManager");
        var gm = mgrGO.AddComponent<GameManager>();
        var rg = mgrGO.AddComponent<RiddleGenerator>();
        mgrGO.AddComponent<AudioManager>();
        gameObject.AddComponent<AppQuit>();

        // apiKey and model fields may not exist on Diamond-based RiddleGenerator
        var apiKeyField = rg.GetType().GetField("apiKey");
        if (apiKeyField != null) apiKeyField.SetValue(rg, "");
        var modelField = rg.GetType().GetField("model");
        if (modelField != null) modelField.SetValue(rg, "openai/gpt-4o-mini");

        gm.uiManager       = ui;
        gm.riddleGenerator = rg;
        gm.StartGame();
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
        if (FindAnyObjectByType<EventSystem>() != null) return;
        var es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
        es.AddComponent<InputSystemUIInputModule>();
    }

    GameObject FindDeep(GameObject root, string name)
    {
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            if (t.name == name) return t.gameObject;
        return null;
    }
}
