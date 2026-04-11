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
                FetchRiddleAndShow("Clothing", State.TargetClothing, GameData.Clothing, GamePhase.GuessColor);
                break;

            case GamePhase.GuessColor:
                FetchRiddleAndShow("Color", State.TargetColor, GameData.Colors, GamePhase.GuessMaterial);
                break;

            case GamePhase.GuessMaterial:
                FetchRiddleAndShow("Material", State.TargetMaterial, GameData.Materials, GamePhase.Reveal);
                break;

            case GamePhase.Reveal:
                uiManager.ShowReveal(State.GuessedColor, State.GuessedClothing, State.GuessedMaterial);
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

    private void FetchRiddleAndShow(string category, string target, System.Collections.Generic.List<string> options, GamePhase nextPhase)
    {
        uiManager.ShowLoading();
        riddleGenerator.GetRiddle(target, category, riddle =>
        {
            State.CurrentRiddle = riddle;
            uiManager.ShowGuessPanel(category, riddle, options, chosen =>
            {
                // Store the player's guess
                switch (category)
                {
                    case "Clothing": State.GuessedClothing = chosen; break;
                    case "Color":    State.GuessedColor    = chosen; break;
                    case "Material": State.GuessedMaterial = chosen; break;
                }

                // Check if the guess is correct
                if (chosen != target)
                {
                    // Wrong answer — instant death
                    GoToPhase(GamePhase.DeathScreen);
                }
                else
                {
                    GoToPhase(nextPhase);
                }
            });
        });
    }

    // Called from FinalJudgment UI
    public void OnPlayerFlatters()  => GoToPhase(GamePhase.WinScreen);
    public void OnPlayerTruth()     => GoToPhase(GamePhase.DeathScreen);
    public void OnPlayAgain()       => StartGame();
}
