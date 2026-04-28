using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

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
        diamond.SetDiamondName("FashionRoyal");
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
                FetchRiddleAndShow("Clothing", State.TargetClothing, options, GamePhase.GuessColor);
                break;
            case GamePhase.GuessColor:
	            options = GameData.GetOptions(GameData.Colors, State.TargetColor);
                FetchRiddleAndShow("Color", State.TargetColor, options, GamePhase.GuessMaterial);
                break;
            case GamePhase.GuessMaterial:
	            options = GameData.GetOptions(GameData.Materials, State.TargetMaterial);
                FetchRiddleAndShow("Material", State.TargetMaterial, options, GamePhase.Reveal);
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

    private void FetchRiddleAndShow(string category, string target, List<string> options, GamePhase nextPhase)
    {
        uiManager.ShowLoading();
        var riddle = FetchRiddle(category);
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
    
    private string FetchRiddle(string category)
    {
	    var riddle = string.Empty;
	    if (diamond != null)
	    {
		    var diamondKey = GetDiamondKey(category);
		    diamond.Riddles.TryGetValue(diamondKey, out riddle);
	    }

	    return riddle;
    }

    private string GetDiamondKey(string gameKey)
    {
	    var dimoandKey = string.Empty;
	    switch (gameKey)
	    {
		    case "Clothing":
			    dimoandKey = "Type_Riddle";
			    break;
		    
		    case "Color":
			    dimoandKey = "Color_Riddle";
			    break;
		    
		    case "Material":
			    dimoandKey = "Material_Riddle";
			    break;
	    }
	    
	    return dimoandKey;
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
        uiManager.ResetTracker();
        uiManager.kingPoseProud?.Invoke(false);
        if (uiManager.narratorLabel != null) uiManager.narratorLabel.text = "";
        uiManager.curtainAnimator?.CloseCurtains();
        await Task.Delay(1000);
        GoToPhase(GamePhase.GuessClothing);
    }
}
