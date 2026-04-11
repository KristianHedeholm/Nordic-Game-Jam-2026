using UnityEngine;

/// <summary>
/// Central game manager. Drives phase transitions and wires up UI.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("References")]
    public UIManager uiManager;
    public RiddleGenerator riddleGenerator;

    public GameState State { get; private set; } = new GameState();

    // King's praise lines after a correct answer
    private static readonly string[] CorrectClothingPraise = {
        "\"EXQUISITE! You recognised my garment instantly! Most impressive!\"",
        "\"*gasps* YES! How did you know?! You must have magnificent taste!\"",
        "\"Extraordinary! The fabric spoke to you, didn't it? I knew it would!\""
    };
    private static readonly string[] CorrectColorPraise = {
        "\"*fans himself* The colour! You got the colour! I'm genuinely moved!\"",
        "\"YES! That is PRECISELY the shade! You have the eye of a true aesthete!\"",
        "\"BRAVO! Not everyone can perceive such a refined hue. You are special!\""
    };
    private static readonly string[] CorrectMaterialPraise = {
        "\"*claps frantically* The material! You felt it through the air, didn't you?!\"",
        "\"MAGNIFICENT! You can practically feel its texture from there! A true gift!\"",
        "\"*tears up* No one has ever... no one has EVER gotten the material right before!\""
    };

    // King's degrading lines before death
    private static readonly string[] WrongInsults = {
        "\"WHAT?! Are you blind?! Or merely STUPID?!\"",
        "\"That is the most OFFENSIVE guess I have ever heard in my royal life!\"",
        "\"*recoils in horror* Did you just... did you even LOOK?!\"",
        "\"Guards, make note — we have a FOOL in the court today.\"",
        "\"I have seen peasants with better taste than you. PEASANTS.\""
    };

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void StartGame()
    {
        State.NewGame();
        Debug.Log($"[Game] New round — King imagines: {State.TargetColor} {State.TargetMaterial} {State.TargetClothing}");
        GoToPhase(GamePhase.Intro);
    }

    public void GoToPhase(GamePhase phase)
    {
        State.Phase = phase;

        switch (phase)
        {
            case GamePhase.Intro:
                uiManager.ShowIntro();
                break;
            case GamePhase.GuessClothing:
                FetchRiddleAndShow("Clothing", State.TargetClothing,
                    GameData.GetOptions(GameData.Clothing, State.TargetClothing), GamePhase.GuessColor,
                    CorrectClothingPraise[Random.Range(0, CorrectClothingPraise.Length)]);
                break;
            case GamePhase.GuessColor:
                FetchRiddleAndShow("Color", State.TargetColor,
                    GameData.GetOptions(GameData.Colors, State.TargetColor), GamePhase.GuessMaterial,
                    CorrectColorPraise[Random.Range(0, CorrectColorPraise.Length)]);
                break;
            case GamePhase.GuessMaterial:
                FetchRiddleAndShow("Material", State.TargetMaterial,
                    GameData.GetOptions(GameData.Materials, State.TargetMaterial), GamePhase.Reveal,
                    CorrectMaterialPraise[Random.Range(0, CorrectMaterialPraise.Length)]);
                break;
            case GamePhase.Reveal:
                uiManager.ShowReveal(State);
                break;
            case GamePhase.FinalJudgment:
                uiManager.ShowFinalJudgment();
                break;
            case GamePhase.WinScreen:
                uiManager.ShowWin();
                break;
            case GamePhase.DeathScreen:
                uiManager.ShowDeath();
                break;
        }
    }

    private void FetchRiddleAndShow(string category, string target, System.Collections.Generic.List<string> options, GamePhase nextPhase, string praiseLine)
    {
        uiManager.ShowLoading();
        StartCoroutine(DoFetch(category, target, options, nextPhase));
    }

    private System.Collections.IEnumerator DoFetch(string category, string target, System.Collections.Generic.List<string> options, GamePhase nextPhase)
    {
        string riddle = null;
        bool done = false;

        if (riddleGenerator != null)
            riddleGenerator.GetRiddle(target, category, r => { riddle = r; done = true; });
        else
            done = true;

        // Wait max 5 seconds for riddle
        float t = 0f;
        while (!done && t < 5f) { yield return null; t += Time.deltaTime; }

        if (string.IsNullOrEmpty(riddle))
            riddle = "I am the finest of its kind,\nCan you guess what fills the King's mind?";

        State.CurrentRiddle = riddle;
        uiManager.ShowGuessPanel(category, riddle, options, chosen =>
        {
            switch (category)
            {
                case "Clothing": State.GuessedClothing = chosen; break;
                case "Color":    State.GuessedColor    = chosen; break;
                case "Material": State.GuessedMaterial = chosen; break;
            }
            uiManager.UpdateAnswerTracker(category, chosen, chosen == target);
            GoToPhase(nextPhase);
        });
    }

    public void GoToFinalQuestion(bool allCorrect)
    {
        State.AllCorrect = allCorrect;
        uiManager.ShowFinalQuestion(allCorrect);
    }

    public void OnPlayerFlatters()  => GoToPhase(GamePhase.WinScreen);
    public void OnPlayerTruth()     => GoToPhase(GamePhase.DeathScreen);

    /// <summary>Full restart including intro.</summary>
    public void OnPlayAgain() => StartGame();

    /// <summary>Skip intro — close curtains and start a new round of guessing.</summary>
    public void OnPlayAgainSkipIntro()
    {
        State.NewGame();
        Debug.Log($"[Game] New round (skip intro) — King imagines: {State.TargetColor} {State.TargetMaterial} {State.TargetClothing}");
        uiManager.ResetTracker();
        uiManager.kingPoseProud?.Invoke(false);
        uiManager.curtainAnimator?.CloseCurtains();
        GoToPhase(GamePhase.GuessClothing);
    }
}
