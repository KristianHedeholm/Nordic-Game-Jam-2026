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
    public TMP_Text categoryLabel;
    public Transform optionsContainer;   // scatter area for draggable tags
    public Button optionButtonPrefab;    // kept for fallback

    [Header("Backgrounds")]
    public GameObject revealBackground;  // unused - bedroom always visible
    public GameObject nakedKingGO;       // shown on reveal
    public GameObject silhouetteGO;      // hidden on reveal

    [Header("Narrator")]
    public TMP_Text narratorLabel;

    [Header("Drag & Drop")]
    public TagDropZone dropZoneClothing;
    public TagDropZone dropZoneColor;
    public TagDropZone dropZoneMaterial;
    public GameObject draggableTagPrefab; // spawned at runtime by SceneBuilder

    [Header("Answer Tracker")]
    public TMP_Text trackerClothing;
    public TMP_Text trackerColor;
    public TMP_Text trackerMaterial;

    [Header("Reaction Overlay")]
    public GameObject reactionPanel;
    public TMP_Text reactionText;
    public Image reactionBg;

    [Header("Overlay Panels")]
    public GameObject introPanel;
    public GameObject loadingPanel;
    public GameObject revealPanel;
    public GameObject finalJudgmentPanel;
    public GameObject winPanel;
    public GameObject deathPanel;

    [Header("Intro")]
    public TMP_Text introText;
    public Button introStartButton;

    [Header("Tutorial")]
    public GameObject tutorialPanel;
    public Button tutorialNextButton;

    [Header("Reveal")]
    public TMP_Text revealText;
    public Button revealContinueButton;

    [Header("Judgment")]
    public TMP_Text finalText;
    public Button flatterButton;
    public Button truthButton;

    [Header("Win")]
    public TMP_Text winText;
    public Button winPlayAgainButton;

    [Header("Death")]
    public TMP_Text deathText;
    public Button deathPlayAgainButton;

    // Callback to switch king to proud pose (wired by SceneBuilder)
    public Action<bool> kingPoseProud;

    // Button sprites (set by SceneBuilder at runtime)
    [HideInInspector] public Sprite buttonStartSprite;
    [HideInInspector] public Sprite buttonNextSprite;

    // ── TYPEWRITER HELPER ─────────────────────────────────────────────────
    
    void SetKingSpeechText(TMP_Text label, string text, Action onDone = null)
    {
	    if (label == null) return;
	    var tw = label.GetComponent<TypewriterEffect>() ?? label.gameObject.AddComponent<TypewriterEffect>();
	    tw.charsPerSecond = 22f;
	    tw.TypeWrite(text, () =>
	    {
		    AudioManager.Instance?.StopKingTalk();
		    onDone?.Invoke();
	    });
    }
    
    void HideAllOverlays()
    {
        introPanel?.SetActive(false);
        tutorialPanel?.SetActive(false);
        loadingPanel?.SetActive(false);
        revealPanel?.SetActive(false);
        finalJudgmentPanel?.SetActive(false);
        winPanel?.SetActive(false);
        deathPanel?.SetActive(false);
        reactionPanel?.SetActive(false);
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
        if (trackerClothing)  { trackerClothing.text  = "";  trackerClothing.color  = Color.white; }
        if (trackerColor)     { trackerColor.text     = "";  trackerColor.color     = Color.white; }
        if (trackerMaterial)  { trackerMaterial.text  = "";  trackerMaterial.color  = Color.white; }

        // Clear any locked tags still sitting in drop zones
        ResetDropZone(dropZoneClothing);
        ResetDropZone(dropZoneColor);
        ResetDropZone(dropZoneMaterial);
    }

    void ResetDropZone(TagDropZone zone)
    {
        if (zone == null) return;
        // Destroy any draggable tag children
        foreach (Transform child in zone.transform)
        {
            if (child.GetComponent<DraggableTag>() != null)
                Destroy(child.gameObject);
        }
        // Reset the drop zone state via reflection-free approach
        var dz = zone.GetComponent<TagDropZone>();
        // Re-add component trick: destroy and re-add to reset state
        var cat = dz.category;
        var lbl = dz.answerLabel;
        Destroy(dz);
        var newDz = zone.gameObject.AddComponent<TagDropZone>();
        newDz.category = cat;
        newDz.answerLabel = lbl;
        if (lbl != null) { lbl.text = ""; lbl.gameObject.SetActive(true); }

        // Re-wire in the correct slot
        if (cat == "Clothing") dropZoneClothing = newDz;
        else if (cat == "Color") dropZoneColor = newDz;
        else if (cat == "Material") dropZoneMaterial = newDz;

        newDz.SetActive(false);
    }

    public void UpdateAnswerTracker(string category, string answer, bool correct)
    {
        TMP_Text target = category switch
        {
            "Clothing" => trackerClothing,
            "Color"    => trackerColor,
            "Material" => trackerMaterial,
            _          => null
        };
        if (target == null) return;
        target.text  = $"{category}: {answer}";
        target.color = Color.white;
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
        kingPoseProud?.Invoke(false);
        if (narratorLabel != null) narratorLabel.text = "";
        if (riddleText != null) riddleText.transform.parent.gameObject.SetActive(true);
        stagePanel?.SetActive(false);
        curtainAnimator?.CloseCurtains();

        // Show title screen — START button switches to tutorial slides
        introPanel.SetActive(true);
        introStartButton.onClick.RemoveAllListeners();
        introStartButton.onClick.AddListener(() =>
        {
            AudioManager.Instance?.PlayButtonClick();
            introPanel.SetActive(false);
            tutorialPanel?.SetActive(true);
            currentSlide = 0;
            ShowSlide(currentSlide);
        });
    }

    void ShowSlide(int index)
    {
        if (!introText.TryGetComponent<TypewriterEffect>(out var tw))
        {
	        tw = introText.gameObject.AddComponent<TypewriterEffect>();
        }
        tw.charsPerSecond = 22f;
        tw.TypeWrite(IntroSlides[index]);
        
        // Use tutorial button — only wire if it exists
        if(tutorialNextButton == null)
		{
			return;
		}
        
		tutorialNextButton.onClick.RemoveAllListeners();
		tutorialNextButton.onClick.AddListener(() =>
		{
			AudioManager.Instance?.PlayButtonClick();
			if (index < IntroSlides.Length - 1)
				ShowSlide(index + 1);
			else
			{
				tutorialPanel?.SetActive(false);
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
        if (categoryLabel != null) categoryLabel.text = "";
        if (riddleText != null)
        {
            riddleText.transform.parent.gameObject.SetActive(true);
            riddleText.text = "...";
        }
    }

    // ── GUESS PANEL — riddle first, buttons appear after ─────────────────

    public void ShowGuessPanel(string category, string riddle, List<string> options, Action<string> onChosen)
    {
        HideAllOverlays();
        // Force hide loading panel explicitly
        loadingPanel?.SetActive(false);
        stagePanel?.SetActive(true);
        if (riddleText != null) riddleText.transform.parent.gameObject.SetActive(true);
        if (categoryLabel != null) categoryLabel.text = $"What is the King's <b>{category}</b>?";

        // Activate the correct drop zone, dim the others
        ActivateDropZone(category, onChosen);

        // Clear old tags
        ClearOptions();

        // Scatter draggable tags
        var tags = new List<GameObject>();
        if (draggableTagPrefab == null)
        {
            Debug.LogError("[UIManager] draggableTagPrefab is null! Tags cannot be spawned.");
        }
        else
        {
            var tagSprite = Resources.Load<Sprite>("Art/Theme_Tag");
            float areaW = 1400f;
            float startX = -areaW / 2f + 150f;
            float spacing = areaW / (options.Count + 1);

            for (int idx = 0; idx < options.Count; idx++)
            {
                var tagGO = Instantiate(draggableTagPrefab, optionsContainer);
                tagGO.SetActive(false);
                var rt = tagGO.GetComponent<RectTransform>();
                float x = startX + spacing * (idx + 1) + UnityEngine.Random.Range(-30f, 30f);
                float y = UnityEngine.Random.Range(-60f, 60f);
                rt.anchoredPosition = new Vector2(x, y);

                // Apply Theme_Tag sprite to background image
                var img = tagGO.GetComponent<Image>();
                if (img == null) img = tagGO.AddComponent<Image>();
                img.sprite = tagSprite;
                img.type   = Image.Type.Simple;
                img.preserveAspect = false;
                img.color  = Color.white;

                var drag = tagGO.GetComponent<DraggableTag>();
                if (drag != null) drag.value = options[idx];
                var lbl = tagGO.GetComponentInChildren<TMP_Text>();
                if (lbl != null)
                {
                    lbl.text = options[idx];
                    lbl.enableWordWrapping = false;
                    lbl.enableAutoSizing   = true;
                    lbl.fontSizeMin = 12;
                    lbl.fontSizeMax = 22;
                }
                tags.Add(tagGO);
            }
        }

        // Type riddle, reveal tags after — if typewriter fails, show after 3s fallback
        AudioManager.Instance?.PlayKingTalk();
        bool tagsShown = false;
        void ShowTags() { if (tagsShown) return; tagsShown = true; foreach (var t in tags) t?.SetActive(true); }

        SetKingSpeechText(riddleText, riddle, () => ShowTags());
        StartCoroutine(FallbackShowTags(3f, () => ShowTags()));
    }

    IEnumerator PopIn(GameObject go)
    {
        var img = go.GetComponent<Image>();
        if (img == null) yield break;
        float dur = 0.08f;
        float elapsed = 0f;
        while (elapsed < dur)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / dur);
            var c = img.color; c.a = t; img.color = c;
            yield return null;
        }
        var fc = img.color; fc.a = 1f; img.color = fc;
    }

    IEnumerator FallbackShowTags(float delay, Action show)
    {
        yield return new UnityEngine.WaitForSeconds(delay);
        show?.Invoke();
    }

    void ActivateDropZone(string category, Action<string> onChosen)
    {
        // Dim all, activate current
        dropZoneClothing?.SetActive(category == "Clothing");
        dropZoneColor?.SetActive(category == "Color");
        dropZoneMaterial?.SetActive(category == "Material");

        TagDropZone zone = category switch
        {
            "Clothing" => dropZoneClothing,
            "Color"    => dropZoneColor,
            "Material" => dropZoneMaterial,
            _          => null
        };

        if (zone != null)
        {
            zone.onAnswered = (chosen) =>
            {
                UpdateAnswerTracker(category, chosen, false);
                switch (category)
                {
                    case "Clothing": if (trackerClothing) trackerClothing.text = $"Garment: {chosen}"; break;
                    case "Color":    if (trackerColor)    trackerColor.text    = $"Color: {chosen}";    break;
                    case "Material": if (trackerMaterial) trackerMaterial.text = $"Material: {chosen}"; break;
                }
                onChosen(chosen);
            };
        }
    }

    // ── REACTION SCREENS ──────────────────────────────────────────────────

    public void ShowCorrectAnswerReaction(string kingQuote, Action onContinue)
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
    }

    // ── REVEAL — phased: curtains → score → judgment ──────────────────────

    public void ShowReveal(GameState state)
    {
        HideAllOverlays();
        stagePanel?.SetActive(true);
        ClearOptions();
        if (categoryLabel != null) categoryLabel.text = "";
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
                if (silhouetteGO != null) silhouetteGO.SetActive(false);
                if (nakedKingGO  != null) { nakedKingGO.SetActive(true); StartCoroutine(PopIn(nakedKingGO)); }
                AudioManager.Instance?.PlayKingLaugh();
                kingPoseProud?.Invoke(true);

                // Narrator text — shown as part of reveal bubble text after king speech
                if (narratorLabel != null)
                    narratorLabel.text = "<i>...says the King.\nObviously wearing <b>absolutely nothing.</b></i>";

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
        RevealTrackerResult(trackerClothing, "Garment",  state.GuessedClothing, state.TargetClothing);
        yield return new WaitForSeconds(0.6f);
        RevealTrackerResult(trackerColor,    "Color",    state.GuessedColor,    state.TargetColor);
        yield return new WaitForSeconds(0.6f);
        RevealTrackerResult(trackerMaterial, "Material", state.GuessedMaterial, state.TargetMaterial);
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

    void RevealTrackerResult(TMP_Text label, string category, string guessed, string correct)
    {
        if (label == null) return;

        bool isCorrect = guessed == correct;

        // Colour the drop zone background green or red at full alpha
        TagDropZone zone = category switch
        {
            "Garment"  => dropZoneClothing,
            "Color"    => dropZoneColor,
            "Material" => dropZoneMaterial,
            _          => null
        };
        if (zone != null)
        {
            var img = zone.GetComponent<Image>();
            if (img != null)
                img.color = isCorrect
                    ? new Color(0.15f, 0.75f, 0.25f, 1f)  // green, full alpha
                    : new Color(0.85f, 0.15f, 0.15f, 1f); // red, full alpha
        }

        if (isCorrect)
        {
            label.text  = $"<b>{guessed} ✓</b>";
            label.color = Color.white;
            AudioManager.Instance?.PlayCorrect();
        }
        else
        {
            label.text  = $"<b>{guessed} ✗</b>\n<size=18>→ {correct}</size>";
            label.color = Color.white;
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
