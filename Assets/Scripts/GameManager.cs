using System;
using RawPowerLabs.DynamicAI;
using UnityEngine;

public enum RiddleKind
{
	Garment,
	Color,
	Material,
}

/// <summary>
/// Central game manager. Drives phase transitions and wires up UI.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("References")]
    public UIManager uiManager;
    public Diamond diamond;

    private RiddleDataCollection RiddleDataCollection = new RiddleDataCollection();
    
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        
        StartGame();
    }

    private async void StartGame()
    {
        RiddleDataCollection.CreateNewRiddleAnswers();
        GoToPhase(GamePhase.Intro);
        diamond.Init();
        try
        {
	        await diamond.GenerateRiddles(RiddleDataCollection);
        }
        catch (Exception e)
        {
	        UnityEngine.Debug.LogException(e);
        }
    }

    public void GoToPhase(GamePhase phase)
    {
        switch (phase)
        {
            case GamePhase.Intro:
                uiManager.ShowIntro();
                break;
            case GamePhase.GuessClothing:
                FetchRiddleAndShow(RiddleKind.Garment, GamePhase.GuessColor);
                break;
            case GamePhase.GuessColor:
                FetchRiddleAndShow(RiddleKind.Color, GamePhase.GuessMaterial);
                break;
            case GamePhase.GuessMaterial:
                FetchRiddleAndShow(RiddleKind.Material, GamePhase.Reveal);
                break;
            case GamePhase.Reveal:
                uiManager.ShowReveal(RiddleDataCollection);
                break;
            case GamePhase.FinalJudgment:
                uiManager.ShowFinalJudgment();
                break;
            case GamePhase.DeathScreen:
                uiManager.ShowDeath();
                break;
        }
    }

    private void FetchRiddleAndShow(RiddleKind riddleKind, GamePhase nextPhase)
    {
        uiManager.ShowLoading();
        var riddle = RiddleDataCollection.GetRiddle(riddleKind);
        var options = RiddleDataCollection.GetAvailableAnswers(riddleKind);
        uiManager.ShowGuessPanel(riddleKind, riddle, options, chosen =>
        {
	        RiddleDataCollection.SetGuessedAnswer(riddleKind, chosen);
	        GoToPhase(nextPhase);
        });
    }
    
    public void GoToFinalQuestion(bool allCorrect)
    {
        uiManager.ShowFinalQuestion(allCorrect);
    }

    //public void OnPlayerFlatters()  => GoToPhase(GamePhase.WinScreen);
    public void OnPlayerTruth()     => GoToPhase(GamePhase.DeathScreen);

    /// <summary>Full restart including intro.</summary>
    public void OnPlayAgain() => StartGame();

    /// <summary>Skip intro — close curtains and start a new round of guessing.</summary>
    public async void OnPlayAgainSkipIntro()
    {
        RiddleDataCollection.CreateNewRiddleAnswers();
        uiManager.ResetDropZones();
        uiManager.curtainAnimator?.CloseCurtains();
        await diamond.GenerateRiddles(RiddleDataCollection);
        
        GoToPhase(GamePhase.GuessClothing);
    }
}
