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
    [SerializeField]
    private GameObject _speechBubble;
    [SerializeField]
    private TMP_Text _speechBubbleText;
    [SerializeField]
    TypewriterEffect _speechBubbleTypewriterEffect;
    public Transform optionsContainer;   // scatter area for draggable tags

    [Header("Backgrounds")]
    [SerializeField]
    private GameObject _nakedKingGO; 
    [SerializeField]
    private GameObject _silhouetteGO;
    
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
    [SerializeField]
    private GameObject _loadingPanel;
    [SerializeField]
    private GameObject _revealPanel;
    [SerializeField]
    private GameObject _finalJudgmentPanel;
    [SerializeField]
    private GameObject _deathPanel;
    [SerializeField]
    private GameObject _creditsPanel;
    
    [SerializeField]
    private DialogueContainer _dialogueContainer;

    [Header("Main Menu")]
    [SerializeField]
    private Button _mainMenuStartButton;
	[SerializeField]
	private Button _creditsButton;
	
	[Header("Credits")]
	[SerializeField]
	private Button _backButton;

    [Header("Intro")]
    [SerializeField]
    private TypewriterEffect _introTextTypewriterEffect;
    [SerializeField]
    private Button _introNextSlideButton;

    [Header("Reveal")]
    public Button revealContinueButton;

    [Header("Judgment")]
    public Button flatterButton;

    [SerializeField]
    private TMP_Text _flatterButtonLabel;
    [SerializeField]
    private Button _truthButton;

    [Header("Death")]
    [SerializeField]
    private Button _playAgainOnDeathButton;
    
    void SetKingSpeechText(string text, Action onDone = null)
    {
	    _speechBubble.SetActive(true);
	    AudioManager.Instance?.PlayKingTalk();
	    _speechBubbleTypewriterEffect.charsPerSecond = 22f;
	    _speechBubbleTypewriterEffect.TypeWrite(text, () =>
	    {
		    AudioManager.Instance?.StopKingTalk();
		    onDone?.Invoke();
	    });
    }
    
    void HideAllOverlays()
    {
        _mainMenuPanel?.SetActive(false);
        _introPanel?.SetActive(false);
        _loadingPanel?.SetActive(false);
        _revealPanel?.SetActive(false);
        _finalJudgmentPanel?.SetActive(false);
        _deathPanel?.SetActive(false);
        _creditsPanel?.SetActive(false);
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
        
        _creditsButton.onClick.RemoveAllListeners();
        _creditsButton.onClick.AddListener(() =>
        {
	        AudioManager.Instance?.PlayButtonClick();
	        _creditsPanel.SetActive(true);
        });
        
        _backButton.onClick.RemoveAllListeners();
        _backButton.onClick.AddListener(() =>
        {
	        AudioManager.Instance?.PlayButtonClick();
	        _creditsPanel.SetActive(false);
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
        if (_silhouetteGO != null)
        {
	        _silhouetteGO.SetActive(true);
        }

        if (_nakedKingGO != null)
        {
	        _nakedKingGO.SetActive(false);
        }
        ClearOptions();
    }
    
    public void ShowGuessPanel(RiddleKind riddleKind, string riddle, List<string> options, Action<string> onChosen)
    {
        HideAllOverlays();
        stagePanel?.SetActive(true);

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
        SetKingSpeechText(riddle, () =>
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
        
        var allCorrect = riddleData.AreAllAnswersCorrect();

        // Hide speech bubble during drumroll — clean stage for the reveal
        _speechBubble.SetActive(false);

        // Phase 1: Drumroll → tadaaa → curtains open
        AudioManager.Instance?.PlayDrumrollThenReveal(() =>
        {
            AudioManager.Instance?.PlayCurtainOpen();
            curtainAnimator?.OpenCurtains(() =>
            {
                // Swap silhouette for naked king instantly
                if (_silhouetteGO != null)
                {
	                _silhouetteGO.SetActive(false);
                }

                if (_nakedKingGO != null)
                {
	                _nakedKingGO.SetActive(true);
                }
                AudioManager.Instance?.PlayKingLaugh();
                
                // Phase 2: Reveal panel shows — king says something short
                revealContinueButton.gameObject.SetActive(false);
                _revealPanel.SetActive(true);
                _speechBubble.SetActive(true);
                
                var kingQuote = _dialogueContainer.GetKingRevealText(allCorrect);
                SetKingSpeechText(kingQuote, () =>
                {
                    // King speech stops — now silently append narrator text
                    _speechBubbleText.text += _dialogueContainer.NarratorLine;
                    _speechBubbleText.maxVisibleCharacters = int.MaxValue;
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
        revealContinueButton.gameObject.SetActive(false);
        revealContinueButton.onClick.RemoveAllListeners();
        revealContinueButton.onClick.AddListener(() => GameManager.Instance.GoToFinalQuestion(allCorrect));
        
        SetKingSpeechText(scoreMessage, () =>
        {
            revealContinueButton.gameObject.SetActive(true);
        });
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
        _finalJudgmentPanel.SetActive(true);
        
        // Hide buttons until text is done
        flatterButton.gameObject.SetActive(false);
        _truthButton.gameObject.SetActive(false);
        
        _flatterButtonLabel.text = _dialogueContainer.GetFlatterLabelText(allCorrect);
        var finalQuestion = _dialogueContainer.GetFinalQuestion(allCorrect);
        
        SetKingSpeechText(finalQuestion, () =>
        {
            flatterButton.gameObject.SetActive(true);
            _truthButton.gameObject.SetActive(true);
        });

        flatterButton.onClick.RemoveAllListeners();
        _truthButton.onClick.RemoveAllListeners();
        flatterButton.onClick.AddListener(() =>
        {
	        _speechBubble.SetActive(false);
	        flatterButton.gameObject.SetActive(false);
	        _truthButton.gameObject.SetActive(false);
	        _loadingPanel?.SetActive(true);
	        GameManager.Instance.OnPlayAgainSkipIntro();
        });
        _truthButton.onClick.AddListener(() => GameManager.Instance.OnPlayerTruth());
    }

    public void ShowFinalJudgment() => ShowFinalQuestion(true);
    
    public void ShowDeath()
    {
        HideAllOverlays();
        stagePanel?.SetActive(false);
        _deathPanel.SetActive(true);
        AudioManager.Instance?.PlayDeath();
        
        _playAgainOnDeathButton.onClick.RemoveAllListeners();
        _playAgainOnDeathButton.onClick.AddListener(() => GameManager.Instance.OnPlayAgain());
    }
}
