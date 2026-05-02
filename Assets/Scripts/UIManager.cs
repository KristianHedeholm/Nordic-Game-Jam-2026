using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls all game UI. Wired up by SceneBuilder at runtime.
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("Stage")]
    public GameObject stagePanel;
    public CurtainAnimator curtainAnimator;
    public TMP_Text riddleText;
    public Transform optionsContainer;   // scatter area for draggable tags

    [Header("Backgrounds")]
    public GameObject nakedKingGO;       // shown on reveal
    public GameObject silhouetteGO;      // hidden on reveal
    
    [Header("Drag & Drop")]
    [SerializeField]
    private TagDropZone[] _dropZones; 
    [SerializeField]
    private GameObject _draggableTagPrefab;
    
    [Header("Overlay Panels")]
    [SerializeField]
    private GameObject _mainMenuPanel;
    [SerializeField]
    private GameObject _introPanel;
    public GameObject loadingPanel;
    public GameObject revealPanel;
    public GameObject finalJudgmentPanel;
    public GameObject deathPanel;

    [Header("Main Menu")]
    [SerializeField]
    private Button _mainMenuStartButton;

    [Header("Intro")]
    [SerializeField]
    private TypewriterEffect _introTextTypewriterEffect;
    [SerializeField]
    private Button _introNextSlideButton;

    [Header("Reveal")]
    public TMP_Text revealText;
    public Button revealContinueButton;

    [Header("Judgment")]
    public TMP_Text finalText;
    public Button flatterButton;
    public Button truthButton;

    [Header("Death")]
    public Button _playAgainOnDeathButton;
    
    void SetKingSpeechText(TMP_Text label, string text, Action onDone = null)
    {
	    if (label == null) return;
	    if (!label.TryGetComponent<TypewriterEffect>(out var tw))
	    {
		    tw = label.gameObject.AddComponent<TypewriterEffect>();
	    }
	    tw.charsPerSecond = 22f;
	    tw.TypeWrite(text, () =>
	    {
		    AudioManager.Instance?.StopKingTalk();
		    onDone?.Invoke();
	    });
    }
    
    void HideAllOverlays()
    {
        _mainMenuPanel?.SetActive(false);
        _introPanel?.SetActive(false);
        loadingPanel?.SetActive(false);
        revealPanel?.SetActive(false);
        finalJudgmentPanel?.SetActive(false);
        deathPanel?.SetActive(false);
    }

    void ClearOptions()
    {
        foreach (Transform child in optionsContainer)
            Destroy(child.gameObject);
    }

    public void ResetDropZones()
    {
	    foreach (var dropZone in _dropZones)
	    {
		    dropZone.Reset();
	    }
    }

    // ── INTRO — 3 animated tutorial slides ───────────────────────────────

    private static readonly string[] IntroSlides = {
        "The King has dressed himself in the\n<b>finest outfit in all the land.</b>",
        "He will give you <b>three riddles.</b>\n\nFor each one, guess: <margin-left=\"300\">\n\n• <align=\"left\">The <b>garment</b>\n• <align=\"left\">The <b>colour</b>\n• <align=\"left\">The <b>material</b>",
        "At the end, the truth will be revealed.\n\nChoose your words wisely.\n\n<b>Your head depends on it.</b>"
    };
    
    private int _currentSlideIndex = 0;

    public void ShowIntro()
    {
        HideAllOverlays();
        ResetDropZones();
        if (riddleText != null) riddleText.transform.parent.gameObject.SetActive(true);
        stagePanel?.SetActive(false);
        curtainAnimator?.CloseCurtains();

        // Show title screen — START button switches to tutorial slides
        _mainMenuPanel.SetActive(true);
        _mainMenuStartButton.onClick.RemoveAllListeners();
        _mainMenuStartButton.onClick.AddListener(() =>
        {
            AudioManager.Instance?.PlayButtonClick();
            _mainMenuPanel.SetActive(false);
            _introPanel?.SetActive(true);
            _currentSlideIndex = 0;
            ShowSlide(0);
        });
        
        _introNextSlideButton.onClick.RemoveAllListeners();
        _introNextSlideButton.onClick.AddListener(() =>
        {
	        AudioManager.Instance?.PlayButtonClick();
	        _currentSlideIndex++;
	        if (_currentSlideIndex < IntroSlides.Length)
	        {
		        ShowSlide(_currentSlideIndex);
	        }
	        else
	        {
		        _introPanel?.SetActive(false);
		        GameManager.Instance.GoToPhase(GamePhase.GuessClothing);
	        }
        });
    }

    void ShowSlide(int index)
    {
	    _introTextTypewriterEffect.TypeWrite(IntroSlides[index]);
    }
    

    public void ShowLoading()
    {
        HideAllOverlays();
        stagePanel?.SetActive(true);
        if (silhouetteGO != null) silhouetteGO.SetActive(true);
        if (nakedKingGO  != null) nakedKingGO.SetActive(false);
        ClearOptions();
        if (riddleText != null)
        {
            riddleText.transform.parent.gameObject.SetActive(true);
            riddleText.text = "...";
        }
    }

    // ── GUESS PANEL — riddle first, buttons appear after ─────────────────

    public void ShowGuessPanel(RiddleKind riddleKind, string riddle, List<string> options, Action<string> onChosen)
    {
        HideAllOverlays();
        stagePanel?.SetActive(true);
        if (riddleText != null) riddleText.transform.parent.gameObject.SetActive(true);

        // Activate the correct drop zone, dim the others
        ActivateDropZone(riddleKind, onChosen);

        // Clear old tags
        ClearOptions();

        // Scatter draggable tags
        
        if (_draggableTagPrefab == null)
        {
            Debug.LogError("[UIManager] draggableTagPrefab is null! Tags cannot be spawned.");
        }
        
        var tags = new List<GameObject>();
        for (int idx = 0; idx < options.Count; idx++)
        {
	        var tagGO = Instantiate(_draggableTagPrefab, optionsContainer);
	        tagGO.SetActive(false);
	        var drag = tagGO.GetComponent<DraggableTag>();
	        drag.SetTagLabel(options[idx]);
	        
	        tags.Add(tagGO);
        }
        
        // Type riddle, reveal tags after.
        AudioManager.Instance?.PlayKingTalk();
        SetKingSpeechText(riddleText, riddle, () =>
        {
	        foreach (var tag in tags)
	        {
		        tag?.SetActive(true);
	        }
        });
    }
    
    void ActivateDropZone(RiddleKind riddleKind, Action<string> onChosen)
    {
	    // Dim all, activate current
	    foreach (var dropZone in _dropZones)
	    {
		    dropZone.ActivateForCurrentRiddle(riddleKind);
	    }
	    
	    var zone = GetTagDropZone(riddleKind);
	    if (zone == null)
	    {
		    return;
	    }
	    
	    zone.onAnswered = (chosen) =>
	    {
		    onChosen(chosen);
	    };
    }
    
    private TagDropZone GetTagDropZone(RiddleKind kind)
    {
	    foreach (var dropZone in _dropZones)
	    {
		    if (dropZone.RiddleKind == kind)
		    {
			    return dropZone;
		    }
	    }

	    return null;
    }
    

    // ── REACTION SCREENS ──────────────────────────────────────────────────

    /*public void ShowCorrectAnswerReaction(string kingQuote, Action onContinue)
    {
        HideAllOverlays();
        stagePanel?.SetActive(true);
        reactionPanel.SetActive(true);
        reactionBg.color = new Color(0.05f, 0.25f, 0.08f, 0.92f);
        SetKingSpeechText(reactionText, "<size=60><b>CORRECT!</b></size>\n\n" + kingQuote + "\n\n<size=24><i>~ tap to continue ~</i></size>");
        var btn = reactionPanel.GetComponent<Button>() ?? reactionPanel.AddComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => { reactionPanel.SetActive(false); onContinue?.Invoke(); });
    }

    public void ShowWrongAnswerReaction(string kingInsult, Action onContinue)
    {
        HideAllOverlays();
        stagePanel?.SetActive(true);
        reactionPanel.SetActive(true);
        reactionBg.color = new Color(0.3f, 0.04f, 0.04f, 0.92f);
        SetKingSpeechText(reactionText, "<size=60><b>WRONG!</b></size>\n\n" + kingInsult + "\n\n<size=24><i>~ tap to face your fate ~</i></size>");
        var btn = reactionPanel.GetComponent<Button>() ?? reactionPanel.AddComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => { reactionPanel.SetActive(false); onContinue?.Invoke(); });
    }*/

    // ── REVEAL — phased: curtains → score → judgment ──────────────────────

    public void ShowReveal(GameState state)
    {
        HideAllOverlays();
        stagePanel?.SetActive(true);
        ClearOptions();
        riddleText.text = "...";

        bool allCorrect = state.GuessedClothing == state.TargetClothing &&
                          state.GuessedColor    == state.TargetColor    &&
                          state.GuessedMaterial == state.TargetMaterial;

        // Hide speech bubble during drumroll — clean stage for the reveal
        riddleText.transform.parent.gameObject.SetActive(false);

        // Phase 1: Drumroll → tadaaa → curtains open
        AudioManager.Instance?.PlayDrumrollThenReveal(() =>
        {
            AudioManager.Instance?.PlayCurtainOpen();
            curtainAnimator?.OpenCurtains(() =>
            {
                // Swap silhouette for naked king instantly
                if (silhouetteGO != null)
                {
	                silhouetteGO.SetActive(false);
                }

                if (nakedKingGO != null)
                {
	                nakedKingGO.SetActive(true);
                }
                AudioManager.Instance?.PlayKingLaugh();
                
                // Phase 2: Reveal panel shows — king says something short
                revealContinueButton.gameObject.SetActive(false);
                revealPanel.SetActive(true);

                // King speaks the quote — speech stops when quote ends, then narrator line appends silently
                string kingQuote = allCorrect
                    ? "\"BEHOLD! Am I not the most magnificently dressed monarch you have ever seen?\""
                    : "\"Feast your eyes upon the FINEST outfit ever crafted by mortal hands!\"";
                string narratorLine = "\n<size=20><i>...says the King, obviously wearing <b>absolutely nothing.</b></i></size>";

                AudioManager.Instance?.PlayKingTalk();
                SetKingSpeechText(revealText, kingQuote, () =>
                {
                    // King speech stops — now silently append narrator text
                    revealText.text += narratorLine;
                    revealText.maxVisibleCharacters = int.MaxValue;
                    StartCoroutine(ShowScoreThenJudgment(state, allCorrect));
                });
            });
        });
    }

    IEnumerator ShowScoreThenJudgment(GameState state, bool allCorrect)
    {
        // Brief pause after king speech
        yield return new WaitForSeconds(0.8f);

        // Reveal tracker results one by one with sound
        RevealTrackerResult(RiddleKind.Garment, state.GuessedClothing, state.TargetClothing);
        yield return new WaitForSeconds(0.6f);
        
        RevealTrackerResult(RiddleKind.Color, state.GuessedColor,    state.TargetColor);
        yield return new WaitForSeconds(0.6f);
        
        RevealTrackerResult(RiddleKind.Material, state.GuessedMaterial, state.TargetMaterial);
        yield return new WaitForSeconds(0.5f);

        // Count correct
        int score = 0;
        if (state.GuessedClothing == state.TargetClothing) score++;
        if (state.GuessedColor    == state.TargetColor)    score++;
        if (state.GuessedMaterial == state.TargetMaterial) score++;

        // Show score in speech bubble
        string scoreMsg = score == 3
            ? $"<b>{score}/3</b> correct!\n\"Extraordinary! You truly understand fashion!\""
            : score == 0
            ? $"<b>{score}/3</b> correct.\n\"...I expected more from you.\""
            : $"<b>{score}/3</b> correct.\n\"Hmm. Some potential, perhaps.\"";

        AudioManager.Instance?.PlayKingTalk();
        SetKingSpeechText(revealText, scoreMsg, () =>
        {
            revealContinueButton.gameObject.SetActive(true);
        });

        revealContinueButton.gameObject.SetActive(false);
        revealContinueButton.onClick.RemoveAllListeners();
        revealContinueButton.onClick.AddListener(() => GameManager.Instance.GoToFinalQuestion(allCorrect));
    }
    
    void RevealTrackerResult(RiddleKind riddleKind, string guessed, string correct)
    {
	    var isCorrect = guessed == correct;
	    var zone = GetTagDropZone(riddleKind);
	    if (zone == null)
	    {
		    return;
	    }
	    
	    zone.SetAnswerColor(isCorrect);
	    
	    if (isCorrect)
	    {
		    AudioManager.Instance?.PlayCorrect();
	    }
	    else
	    {
		    AudioManager.Instance?.PlayWrong();
	    }
    }

    // ── FINAL JUDGMENT ────────────────────────────────────────────────────

    public void ShowFinalQuestion(bool allCorrect)
    {
        HideAllOverlays();
        stagePanel?.SetActive(true);
        finalJudgmentPanel.SetActive(true);
        AudioManager.Instance?.PlayKingTalk();
        
        if (finalText != null) finalText.transform.parent.gameObject.SetActive(true);

        // Hide buttons until text is done
        flatterButton.gameObject.SetActive(false);
        truthButton.gameObject.SetActive(false);

        string question = allCorrect
            ? "\"You have <b>magnificent</b> taste!\"\n\"Would you like to admire another one of my spectacular outfits?\""
            : "\"You clearly need more practice.\"\n\"Would you like to try again and improve yourself?\"";

        flatterButton.GetComponentInChildren<TMP_Text>().text = allCorrect
            ? "\"Yes Your Majesty, it would be an honour!\""
            : "\"Yes Your Majesty, please give me another chance!\"";
        truthButton.GetComponentInChildren<TMP_Text>().text = "\"...Why are you wearing nothing?\"";

        SetKingSpeechText(finalText, question, () =>
        {
            flatterButton.gameObject.SetActive(true);
            truthButton.gameObject.SetActive(true);
        });

        flatterButton.onClick.RemoveAllListeners();
        truthButton.onClick.RemoveAllListeners();
        flatterButton.onClick.AddListener(() =>
        {
	        if (finalText != null) finalText.transform.parent.gameObject.SetActive(false);
	        if (riddleText != null) riddleText.transform.parent.gameObject.SetActive(false);
	        flatterButton.gameObject.SetActive(false);
	        truthButton.gameObject.SetActive(false);
	        loadingPanel?.SetActive(true);
	        GameManager.Instance.OnPlayAgainSkipIntro();
        });
        truthButton.onClick.AddListener(() => GameManager.Instance.OnPlayerTruth());
    }

    public void ShowFinalJudgment() => ShowFinalQuestion(true);

    // ── WIN / DEATH ───────────────────────────────────────────────────────

    /*public void ShowWin()
    {
        HideAllOverlays();
        stagePanel?.SetActive(false);
        winPanel.SetActive(true);
        AudioManager.Instance?.PlayWin();
        SetText(winText,
            "The King claps with delight!\n\n" +
            "\"YES! You truly have the finest eyes in all the kingdom!\"\n" +
            "You survive. The King is happy.\nThe kingdom is at peace.\n" +
            "<i>(He is still wearing nothing at all.)</i>");
        winPlayAgainButton.onClick.RemoveAllListeners();
        winPlayAgainButton.onClick.AddListener(() => GameManager.Instance.OnPlayAgain());
    }*/

    public void ShowDeath()
    {
        HideAllOverlays();
        stagePanel?.SetActive(false);
        deathPanel.SetActive(true);
        AudioManager.Instance?.PlayDeath();
        
        _playAgainOnDeathButton.onClick.RemoveAllListeners();
        _playAgainOnDeathButton.onClick.AddListener(() => GameManager.Instance.OnPlayAgain());
    }
}
