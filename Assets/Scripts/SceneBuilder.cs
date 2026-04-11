using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Instantiates the GameUI prefab and wires up GameManager + AudioManager.
/// 
/// SETUP:
/// 1. Run "Tools → Kings New Clothes → Build UI Prefab" once in the editor
/// 2. Add this script to an empty GameObject in your scene
/// 3. Assign the GameUI prefab in the Inspector
/// 4. Hit Play
/// 
/// The art team can edit Assets/Prefabs/GameUI.prefab freely.
/// </summary>
[DefaultExecutionOrder(-100)]
public class SceneBuilder : MonoBehaviour
{
    [Header("Assign after running Tools → Kings New Clothes → Build UI Prefab")]
    public GameObject gameUIPrefab;

    void Awake()
    {
        SetupCamera();

        if (gameUIPrefab == null)
        {
            Debug.LogError("[SceneBuilder] gameUIPrefab is not assigned! Run Tools → Kings New Clothes → Build UI Prefab first.");
            return;
        }

        // Instantiate the prefab
        var uiInstance = Instantiate(gameUIPrefab);
        uiInstance.name = "GameUI";

        // Get UIManager from the prefab instance
        var ui = uiInstance.GetComponentInChildren<UIManager>();
        if (ui == null) { Debug.LogError("[SceneBuilder] UIManager not found in GameUI prefab!"); return; }

        // Wire the king proud pose callback (runtime only — can't store lambdas in prefab)
        var armL = uiInstance.transform.Find("Canvas/StagePanel/ArmL")?.GetComponent<RectTransform>();
        var armR = uiInstance.transform.Find("Canvas/StagePanel/ArmR")?.GetComponent<RectTransform>();
        var body = uiInstance.transform.Find("Canvas/StagePanel/KingBody")?.GetComponent<RectTransform>();

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

        // ESC to quit
        gameObject.AddComponent<AppQuit>();

        // Create GameManager + AudioManager
        var mgrGO = new GameObject("GameManager");
        var gm = mgrGO.AddComponent<GameManager>();
        var rg = mgrGO.AddComponent<RiddleGenerator>();
        mgrGO.AddComponent<AudioManager>();

        rg.apiKey = "";
        rg.model  = "openai/gpt-4o-mini";

        gm.uiManager       = ui;
        gm.riddleGenerator = rg;

        // Apply button sprites at runtime (bypasses prefab bake issues)
        ApplyButtonSprites(uiInstance);

        gm.StartGame();
    }

    void ApplyButtonSprites(GameObject uiRoot)
    {
        var startSprite = Resources.Load<Sprite>("Art/Button_Start");
        var nextSprite  = Resources.Load<Sprite>("Art/Button_Next");

        if (startSprite == null) { Debug.LogWarning("[SceneBuilder] Button_Start sprite not found in Resources/Art/"); return; }
        if (nextSprite  == null) { Debug.LogWarning("[SceneBuilder] Button_Next sprite not found in Resources/Art/");  return; }

        // Apply to intro start button
        var startBtn = uiRoot.transform.Find("Canvas/IntroPanel/StartButton");
        if (startBtn != null)
        {
            var img = startBtn.GetComponent<Image>();
            if (img != null) { img.sprite = startSprite; img.color = Color.white; img.type = Image.Type.Sliced; }
            var lbl = startBtn.GetComponentInChildren<TextMeshProUGUI>();
            if (lbl != null) lbl.color = new Color(0.15f, 0.08f, 0.25f);
        }

        // Store next sprite on UIManager for runtime swap
        var ui = uiRoot.GetComponentInChildren<UIManager>();
        if (ui != null)
        {
            ui.buttonNextSprite  = nextSprite;
            ui.buttonStartSprite = startSprite;
        }
    }

    void SetupCamera()
    {
        var cam = Camera.main;
        if (cam == null) return;
        cam.clearFlags      = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.05f, 0.03f, 0.08f);
        cam.orthographic    = true;
    }
}
