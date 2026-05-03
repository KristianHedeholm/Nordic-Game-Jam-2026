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
    
    [SerializeField]
    private DialogueContainer _dialogueContainer;

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

    [SerializeField]
    private TMP_Text _flatterButtonLabel;
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
	        if (_currentSlideIndex < _dialogueContainer.AmountOfIntroSlides)
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
	    var slideText = _dialogueContainer.GetIntroSlideText(index);
	    _introTextTypewriterEffect.TypeWrite(slideText);
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

    public void ShowReveal(RiddleData riddleData)
    {
        HideAllOverlays();
        stagePanel?.SetActive(true);
        ClearOptions();
        riddleText.text = "...";
        
        var allCorrect = riddleData.AreAllAnswersCorrect();

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

                AudioManager.Instance?.PlayKingTalk();
                var kingQuote = _dialogueContainer.GetKingRevealText(allCorrect);
                SetKingSpeechText(revealText, kingQuote, () =>
                {
                    // King speech stops — now silently append narrator text
                    revealText.text += _dialogueContainer.NarratorLine;
                    revealText.maxVisibleCharacters = int.MaxValue;
                    StartCoroutine(ShowScoreThenJudgment(riddleData, allCorrect));
                });
            });
        });
    }

    IEnumerator ShowScoreThenJudgment(RiddleData riddleData, bool allCorrect)
    {
        // Brief pause after king speech
        yield return new WaitForSeconds(0.8f);

        // Reveal tracker results one by one with sound
        RevealTrackerResult(RiddleKind.Garment, riddleData);
        yield return new WaitForSeconds(0.6f);
        
        RevealTrackerResult(RiddleKind.Color, riddleData);
        yield return new WaitForSeconds(0.6f);
        
        RevealTrackerResult(RiddleKind.Material, riddleData);
        yield return new WaitForSeconds(0.5f);
        
        var score = riddleData.GetNumberOfCorrectAnswers();

        // Show score in speech bubble
        var scoreMessage = _dialogueContainer.GetScoreMessage(score);
        
        AudioManager.Instance?.PlayKingTalk();
        SetKingSpeechText(revealText, scoreMessage, () =>
        {
            revealContinueButton.gameObject.SetActive(true);
        });

        revealContinueButton.gameObject.SetActive(false);
        revealContinueButton.onClick.RemoveAllListeners();
        revealContinueButton.onClick.AddListener(() => GameManager.Instance.GoToFinalQuestion(allCorrect));
    }
    
    void RevealTrackerResult(RiddleKind riddleKind, RiddleData riddleData)
    {
	    var isCorrect = riddleData.IsAnswerCorrect(riddleKind);
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
        
        _flatterButtonLabel.text = _dialogueContainer.GetFlatterLabelText(allCorrect);
        var finalQuestion = _dialogueContainer.GetFinalQuestion(allCorrect);
        
        SetKingSpeechText(finalText, finalQuestion, () =>
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
