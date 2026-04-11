using UnityEngine;

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
}
