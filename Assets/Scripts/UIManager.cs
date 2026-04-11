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
    public Transform optionsContainer;
    public Button optionButtonPrefab;

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

    void HideAllOverlays()
    {
        introPanel?.SetActive(false);
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

    public void ResetTracker()
    {
        if (trackerClothing)  trackerClothing.text  = "Garment: ?";
        if (trackerColor)     trackerColor.text     = "Color: ?";
        if (trackerMaterial)  trackerMaterial.text  = "Material: ?";
        if (trackerClothing)  trackerClothing.color  = Color.white;
        if (trackerColor)     trackerColor.color     = Color.white;
        if (trackerMaterial)  trackerMaterial.color  = Color.white;
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
        target.color = Color.white; // grey until reveal
    }

    public void ShowIntro()
    {
        HideAllOverlays();
        ResetTracker();
        kingPoseProud?.Invoke(false); // reset pose
        stagePanel?.SetActive(false);
        curtainAnimator?.CloseCurtains();
        AudioManager.Instance?.PlayIntroFanfare();
        introPanel.SetActive(true);
        introText.text =
            "The King demands your presence!\n\n" +
            "He has dressed in the finest garments\nin all the land...\n\n" +
            "<i>...or so he believes.</i>\n\n" +
            "Guess his outfit. Answer wisely.\nYour head depends on it.";
        introStartButton.onClick.RemoveAllListeners();
        introStartButton.onClick.AddListener(() => GameManager.Instance.GoToPhase(GamePhase.GuessClothing));
    }

    public void ShowLoading()
    {
        HideAllOverlays();
        stagePanel?.SetActive(true);
        loadingPanel.SetActive(true);
        ClearOptions();
        riddleText.text = "...";
        categoryLabel.text = "";
    }

    public void ShowGuessPanel(string category, string riddle, List<string> options, Action<string> onChosen)
    {
        HideAllOverlays();
        stagePanel?.SetActive(true);
        categoryLabel.text = $"What is the King's <b>{category}</b>?";
        riddleText.text = riddle;
        AudioManager.Instance?.PlayKingTalk();

        ClearOptions();
        foreach (var option in options)
        {
            var btn = Instantiate(optionButtonPrefab, optionsContainer);
            btn.gameObject.SetActive(true);
            btn.GetComponentInChildren<TMP_Text>().text = option;
            string captured = option;
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => {
                AudioManager.Instance?.PlayButtonClick();
                // Crowd reacts — randomly deceiving (50/50 cheer vs boo regardless of answer)
                if (UnityEngine.Random.value > 0.5f) AudioManager.Instance?.PlayCrowdCheerGood();
                else AudioManager.Instance?.PlayCrowdCheerBad();
                onChosen(captured);
            });
        }
    }

    // ── REACTION SCREENS ──────────────────────────────────────────────────

    // Legacy stub — kept for compatibility
    public void ShowCorrectAnswerReaction(string kingQuote, Action onContinue)
    {
        HideAllOverlays();
        stagePanel?.SetActive(true);
        reactionPanel.SetActive(true);
        reactionBg.color = new Color(0.05f, 0.25f, 0.08f, 0.92f);
        reactionText.text = "<size=60><b>CORRECT!</b></size>\n\n" + kingQuote + "\n\n<size=24><i>~ tap to continue ~</i></size>";
        reactionText.color = Color.white;

        // Click anywhere on reaction panel to continue
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
        reactionText.text = "<size=60><b>WRONG!</b></size>\n\n" + kingInsult + "\n\n<size=24><i>~ tap to face your fate ~</i></size>";
        reactionText.color = Color.white;

        var btn = reactionPanel.GetComponent<Button>() ?? reactionPanel.AddComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => { reactionPanel.SetActive(false); onContinue?.Invoke(); });
    }

    // Callback to switch king to proud pose (wired by SceneBuilder)
    public Action<bool> kingPoseProud;

    // ── REVEAL ────────────────────────────────────────────────────────────

    public void ShowReveal(GameState state)
    {
        HideAllOverlays();
        stagePanel?.SetActive(true);
        ClearOptions();
        categoryLabel.text = "";
        riddleText.text = "...";

        bool allCorrect = state.GuessedClothing == state.TargetClothing &&
                          state.GuessedColor    == state.TargetColor    &&
                          state.GuessedMaterial == state.TargetMaterial;

        // Drumroll → tadaaa → curtains open → reveal
        AudioManager.Instance?.PlayDrumrollThenReveal(() =>
        {
            AudioManager.Instance?.PlayCurtainOpen();
            curtainAnimator?.OpenCurtains(() =>
            {
                RevealTrackerResult(trackerClothing, "Garment",  state.GuessedClothing, state.TargetClothing);
                RevealTrackerResult(trackerColor,    "Color",    state.GuessedColor,    state.TargetColor);
                RevealTrackerResult(trackerMaterial, "Material", state.GuessedMaterial, state.TargetMaterial);

                AudioManager.Instance?.PlayKingLaugh();
                kingPoseProud?.Invoke(true); // switch king to proud pose

                revealPanel.SetActive(true);
                revealText.text = allCorrect
                    ? $"*The King claps with wild enthusiasm!*\n\n\"SPECTACULAR! A <b>{state.TargetColor} {state.TargetMaterial} {state.TargetClothing}</b>! You are a <b>genius</b>!\"\n\n\"I have SO many other magnificent outfits...\"\n\n<i>He stands before you. Gloriously wearing nothing at all.</i>"
                    : $"The King narrows his eyes at your answers.\n\n\"Hmm. <i>Disappointing.</i> But I am a <b>generous</b> King.\"\n\n\"Perhaps you simply need more practice...\"\n\n<i>He stands before you. Gloriously wearing nothing at all.</i>";

                revealContinueButton.onClick.RemoveAllListeners();
                revealContinueButton.onClick.AddListener(() =>
                    GameManager.Instance.GoToFinalQuestion(allCorrect));
            });
        });
    }

    void RevealTrackerResult(TMP_Text label, string category, string guessed, string correct)
    {
        if (label == null) return;
        if (guessed == correct)
        {
            label.text  = $"<b>{category}: {guessed} ✓</b>";
            label.color = new Color(0.3f, 1f, 0.4f);
            AudioManager.Instance?.PlayCorrect();
        }
        else
        {
            label.text  = $"<b>{category}: {guessed} ✗</b>\n<size=18>({correct})</size>";
            label.color = new Color(1f, 0.3f, 0.3f);
            AudioManager.Instance?.PlayWrong();
        }
    }

    public void ShowFinalQuestion(bool allCorrect)
    {
        HideAllOverlays();
        stagePanel?.SetActive(true);
        finalJudgmentPanel.SetActive(true);
        AudioManager.Instance?.PlayKingTalk();

        if (allCorrect)
        {
            finalText.text =
                "The King beams at you with absolute pride.\n\n" +
                "\"You have <b>magnificent</b> taste! Would you like to guess\nanother one of my spectacular outfits?\"\n\n" +
                "<i>He is, of course, still wearing nothing at all.</i>";
            flatterButton.GetComponentInChildren<TMP_Text>().text = "\"Yes Your Majesty, it would be an honour!\"";
            truthButton.GetComponentInChildren<TMP_Text>().text   = "\"...Why are you wearing nothing?\"";
        }
        else
        {
            finalText.text =
                "The King sighs dramatically.\n\n" +
                "\"You clearly need more practice appreciating\nmy <b>extraordinary</b> fashion sense.\"\n\n" +
                "\"Would you like to try again and improve yourself?\"\n\n" +
                "<i>He is, of course, still wearing nothing at all.</i>";
            flatterButton.GetComponentInChildren<TMP_Text>().text = "\"Yes Your Majesty, please give me another chance!\"";
            truthButton.GetComponentInChildren<TMP_Text>().text   = "\"...Why are you wearing nothing?\"";
        }

        flatterButton.onClick.RemoveAllListeners();
        truthButton.onClick.RemoveAllListeners();
        flatterButton.onClick.AddListener(() => GameManager.Instance.OnPlayAgain());
        truthButton.onClick.AddListener(() => GameManager.Instance.OnPlayerTruth());
    }

    public void ShowFinalJudgment()
    {
        // Legacy — kept for compatibility, routes to ShowFinalQuestion
        ShowFinalQuestion(true);
    }

    public void ShowWin()
    {
        HideAllOverlays();
        stagePanel?.SetActive(false);
        winPanel.SetActive(true);
        AudioManager.Instance?.PlayWin();
        winText.text =
            "The King claps with delight!\n\n" +
            "\"YES! You truly have the finest eyes in all the kingdom!\"\n\n" +
            "You survive. The King is happy.\nThe kingdom is at peace.\n\n" +
            "<i>(He is still wearing nothing at all.)</i>";
        winPlayAgainButton.onClick.RemoveAllListeners();
        winPlayAgainButton.onClick.AddListener(() => GameManager.Instance.OnPlayAgain());
    }

    public void ShowDeath()
    {
        HideAllOverlays();
        stagePanel?.SetActive(false);
        deathPanel.SetActive(true);
        AudioManager.Instance?.PlayDeath();

        var state = GameManager.Instance.State;

        // Check if death was from asking why he's naked (all guesses already stored)
        bool askedAboutNakedness = state.Phase == GamePhase.FinalJudgment ||
                                   state.Phase == GamePhase.WinScreen ||
                                   (state.GuessedClothing != null && state.GuessedColor != null && state.GuessedMaterial != null &&
                                    state.GuessedClothing == state.TargetClothing &&
                                    state.GuessedColor    == state.TargetColor    &&
                                    state.GuessedMaterial == state.TargetMaterial);

        if (askedAboutNakedness)
        {
            deathText.text =
                "The King's face turns purple with rage.\n\n" +
                "\"NAKED?! How DARE you!\"\n\n" +
                "\"I am wearing the FINEST outfit ever created!\"\n\n" +
                "\"GUARDS! OFF WITH THEIR HEAD!\"\n\n" +
                "<i>Truth is a crime in this kingdom.</i>";
        }
        else
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

            deathText.text =
                "The King's eyes narrow.\n\n" +
                $"\"{reason}\"\n\n" +
                "\"GUARDS! OFF WITH THEIR HEAD!\"\n\n" +
                "<i>You should have lied.</i>";
        }

        deathPlayAgainButton.onClick.RemoveAllListeners();
        deathPlayAgainButton.onClick.AddListener(() => GameManager.Instance.OnPlayAgain());
    }
}
