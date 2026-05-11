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

    public RiddleData RiddleData { get; private set; } = new RiddleData();
    
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        
        StartGame();
    }

    private async void StartGame()
    {
        RiddleData.CreateNewRiddleAnswers();
        GoToPhase(GamePhase.Intro);
        diamond.Init();
        try
        {
	        await diamond.GenerateRiddles(RiddleData);
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
                uiManager.ShowReveal(RiddleData);
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
        var riddle = FetchRiddle(riddleKind);
        var options = RiddleData.GetAvailableAnswers(riddleKind);
        uiManager.ShowGuessPanel(riddleKind, riddle, options, chosen =>
        {
	        RiddleData.SetGuessedAnswer(riddleKind, chosen);
	        GoToPhase(nextPhase);
        });
    }
    
    private string FetchRiddle(RiddleKind riddleKind)
    {
	    if (diamond == null)
	    {
		    return string.Empty;
	    }

	    var categorialcal = GetCategoricalOutputFromRiddle(riddleKind);
	    if (!diamond.Riddles.TryGetValue(categorialcal, out var riddle))
	    {
		    return string.Empty;
	    }

	    return riddle;
    }
    
    private CategoricalOutput GetCategoricalOutputFromRiddle(RiddleKind riddleKind)
    {
	    return riddleKind switch
	    {
		    RiddleKind.Garment => CategoricalOutput.TypeRiddle,
		    RiddleKind.Color => CategoricalOutput.ColorRiddle,
		    RiddleKind.Material => CategoricalOutput.MaterialRiddle,
		    _ => CategoricalOutput.TypeRiddle,
	    };
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
        RiddleData.CreateNewRiddleAnswers();
        uiManager.ResetDropZones();
        uiManager.curtainAnimator?.CloseCurtains();
        await diamond.GenerateRiddles(RiddleData);
        
        GoToPhase(GamePhase.GuessClothing);
    }
}
