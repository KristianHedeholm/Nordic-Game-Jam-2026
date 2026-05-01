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
    public TagDropZone dropZoneClothing;
    public TagDropZone dropZoneColor;
    public TagDropZone dropZoneMaterial;
    public GameObject draggableTagPrefab; // spawned at runtime by SceneBuilder
    
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
    private TMP_Text _introText;
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
    public TMP_Text deathText;
    public Button deathPlayAgainButton;

    // ── TYPEWRITER HELPER ─────────────────────────────────────────────────
    
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

    void SetOptionsVisible(bool visible)
    {
        foreach (Transform child in optionsContainer)
            child.gameObject.SetActive(visible);
    }

    // ── TRACKER ───────────────────────────────────────────────────────────

    public void ResetTracker()
    {
        // Clear any locked tags still sitting in drop zones
        ResetDropZone(dropZoneClothing);
        ResetDropZone(dropZoneColor);
        ResetDropZone(dropZoneMaterial);
    }
    
    void ResetDropZone(TagDropZone zone)
    {
	    zone.Reset();
	    zone.SetActive(false);
    }

    // ── INTRO — 3 animated tutorial slides ───────────────────────────────

    private static readonly string[] IntroSlides = {
        "The King has dressed himself in the\n<b>finest outfit in all the land.</b>",
        "He will give you <b>three riddles.</b>\n\nFor each one, guess:\n• The <b>garment</b>\n• The <b>colour</b>\n• The <b>material</b>",
        "At the end, the truth will be revealed.\n\nChoose your words wisely.\n\n<b>Your head depends on it.</b>"
    };

    private int currentSlide = 0;

    public void ShowIntro()
    {
        HideAllOverlays();
        ResetTracker();
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
            currentSlide = 0;
            ShowSlide(currentSlide);
        });
    }

    void ShowSlide(int index)
    {
        if (!_introText.TryGetComponent<TypewriterEffect>(out var tw))
        {
	        tw = _introText.gameObject.AddComponent<TypewriterEffect>();
        }
        tw.charsPerSecond = 22f;
        tw.TypeWrite(IntroSlides[index]);
        
        // Use tutorial button — only wire if it exists
        if(_introNextSlideButton == null)
		{
			return;
		}
        
		_introNextSlideButton.onClick.RemoveAllListeners();
		_introNextSlideButton.onClick.AddListener(() =>
		{
			AudioManager.Instance?.PlayButtonClick();
			if (index < IntroSlides.Length - 1)
				ShowSlide(index + 1);
			else
			{
				_introPanel?.SetActive(false);
				GameManager.Instance.GoToPhase(GamePhase.GuessClothing);
			}
		});
    }
    
    // ── LOADING ───────────────────────────────────────────────────────────

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
        // Force hide loading panel explicitly
        loadingPanel?.SetActive(false);
        stagePanel?.SetActive(true);
        if (riddleText != null) riddleText.transform.parent.gameObject.SetActive(true);

        // Activate the correct drop zone, dim the others
        ActivateDropZone(riddleKind, onChosen);

        // Clear old tags
        ClearOptions();

        // Scatter draggable tags
        
        if (draggableTagPrefab == null)
        {
            Debug.LogError("[UIManager] draggableTagPrefab is null! Tags cannot be spawned.");
        }
        
        var tags = new List<GameObject>();
        for (int idx = 0; idx < options.Count; idx++)
        {
	        var tagGO = Instantiate(draggableTagPrefab, optionsContainer);
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
	    dropZoneClothing?.SetActive(riddleKind == RiddleKind.Garment);
	    dropZoneColor?.SetActive(riddleKind == RiddleKind.Color);
	    dropZoneMaterial?.SetActive(riddleKind == RiddleKind.Material);
	    
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
	    return kind switch
	    {
		    RiddleKind.Garment  => dropZoneClothing,
		    RiddleKind.Color    => dropZoneColor,
		    RiddleKind.Material => dropZoneMaterial,
		    _          => null
	    };
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

        var state = GameManager.Instance.State;
        bool askedAboutNakedness =
            state.GuessedClothing != null && state.GuessedColor != null && state.GuessedMaterial != null &&
            state.GuessedClothing == state.TargetClothing &&
            state.GuessedColor    == state.TargetColor    &&
            state.GuessedMaterial == state.TargetMaterial;

        string deathMsg = askedAboutNakedness
            ? "The King's face turns purple with rage.\n\"Wearing nothing?! How DARE you!\"\n\"I am wearing the FINEST outfit ever created!\"\n\"GUARDS! OFF WITH THEIR HEAD!\"\n<i>Truth is a crime in this kingdom.</i>"
            : BuildWrongDeathMessage(state);

        // No typewriter on death — just show the background image
        if (deathText != null) { deathText.text = ""; }
        deathPlayAgainButton.onClick.RemoveAllListeners();
        deathPlayAgainButton.onClick.AddListener(() => GameManager.Instance.OnPlayAgain());
    }

    string BuildWrongDeathMessage(GameState state)
    {
        string reason = "";
        if (state.GuessedClothing != null && state.GuessedClothing != state.TargetClothing)
            reason = $"A <b>{state.GuessedClothing}</b>?! The King wears a magnificent <b>{state.TargetClothing}</b>!";
        else if (state.GuessedColor != null && state.GuessedColor != state.TargetColor)
            reason = $"<b>{state.GuessedColor}</b>?! The colour is <b>{state.TargetColor}</b>, you blind fool!";
        else if (state.GuessedMaterial != null && state.GuessedMaterial != state.TargetMaterial)
            reason = $"<b>{state.GuessedMaterial}</b>?! It is obviously <b>{state.TargetMaterial}</b>!";
        else
            reason = "Your taste is an insult to the crown!";

        return $"The King's eyes narrow.\n\n\"{reason}\"\n\n\"GUARDS! OFF WITH THEIR HEAD!\"\n\n<i>You should have lied.</i>";
    }
}
