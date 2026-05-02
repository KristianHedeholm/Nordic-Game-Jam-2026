using System.Collections.Generic;
using System.Threading.Tasks;
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

    public GameState State { get; private set; } = new GameState();
    
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        
        StartGame();
    }

    public void StartGame()
    {
        State.NewGame();
        GoToPhase(GamePhase.Intro);
        diamond.Init();
        diamond.PrintReplies(State.TargetClothing, State.TargetColor,  State.TargetMaterial);
    }

    public void GoToPhase(GamePhase phase)
    {
        State.Phase = phase;
        var options = new List<string>();
        
        switch (phase)
        {
            case GamePhase.Intro:
                uiManager.ShowIntro();
                break;
            case GamePhase.GuessClothing:
	            options =  GameData.GetOptions(GameData.Clothing, State.TargetClothing);
                FetchRiddleAndShow(RiddleKind.Garment, options, GamePhase.GuessColor);
                break;
            case GamePhase.GuessColor:
	            options = GameData.GetOptions(GameData.Colors, State.TargetColor);
                FetchRiddleAndShow(RiddleKind.Color, options, GamePhase.GuessMaterial);
                break;
            case GamePhase.GuessMaterial:
	            options = GameData.GetOptions(GameData.Materials, State.TargetMaterial);
                FetchRiddleAndShow(RiddleKind.Material, options, GamePhase.Reveal);
                break;
            case GamePhase.Reveal:
                uiManager.ShowReveal(State);
                break;
            case GamePhase.FinalJudgment:
                uiManager.ShowFinalJudgment();
                break;
            /*case GamePhase.WinScreen:
                uiManager.ShowWin();
                break;*/
            case GamePhase.DeathScreen:
                uiManager.ShowDeath();
                break;
        }
    }

    private void FetchRiddleAndShow(RiddleKind riddleKind, List<string> options, GamePhase nextPhase)
    {
        uiManager.ShowLoading();
        var riddle = FetchRiddle(riddleKind);
        State.CurrentRiddle = riddle;
        uiManager.ShowGuessPanel(riddleKind, riddle, options, chosen =>
        {
	        switch (riddleKind)
	        {
		        case RiddleKind.Garment:
			        State.GuessedClothing = chosen;
			        break;
		        case RiddleKind.Color:
			        State.GuessedColor    = chosen;
			        break;
		        case RiddleKind.Material:
			        State.GuessedMaterial = chosen;
			        break;
	        }
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
        State.AllCorrect = allCorrect;
        uiManager.ShowFinalQuestion(allCorrect);
    }

    //public void OnPlayerFlatters()  => GoToPhase(GamePhase.WinScreen);
    public void OnPlayerTruth()     => GoToPhase(GamePhase.DeathScreen);

    /// <summary>Full restart including intro.</summary>
    public void OnPlayAgain() => StartGame();

    /// <summary>Skip intro — close curtains and start a new round of guessing.</summary>
    public async void OnPlayAgainSkipIntro()
    {
        State.NewGame();
        diamond.PrintReplies(State.TargetClothing, State.TargetColor,  State.TargetMaterial);
        uiManager.ResetDropZones();
        uiManager.curtainAnimator?.CloseCurtains();
        await Task.Delay(1000);
        GoToPhase(GamePhase.GuessClothing);
    }
}
