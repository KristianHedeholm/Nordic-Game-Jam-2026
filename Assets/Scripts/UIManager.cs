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
        stagePanel?.SetActive(false);
        curtainAnimator?.CloseCurtains();
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

        ClearOptions();
        foreach (var option in options)
        {
            var btn = Instantiate(optionButtonPrefab, optionsContainer);
            btn.gameObject.SetActive(true);
            btn.GetComponentInChildren<TMP_Text>().text = option;
            string captured = option;
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => onChosen(captured));
        }
    }

    // ── REACTION SCREENS ──────────────────────────────────────────────────

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

    // ── REVEAL ────────────────────────────────────────────────────────────

    public void ShowReveal(GameState state)
    {
        HideAllOverlays();
        stagePanel?.SetActive(true);
        ClearOptions();
        categoryLabel.text = "";
        riddleText.text = "...";

        // Colour the tracker entries green/red
        RevealTrackerResult(trackerClothing,  "Garment",  state.GuessedClothing,  state.TargetClothing);
        RevealTrackerResult(trackerColor,     "Color",    state.GuessedColor,     state.TargetColor);
        RevealTrackerResult(trackerMaterial,  "Material", state.GuessedMaterial,  state.TargetMaterial);

        bool allCorrect = state.GuessedClothing == state.TargetClothing &&
                          state.GuessedColor    == state.TargetColor    &&
                          state.GuessedMaterial == state.TargetMaterial;

        curtainAnimator?.OpenCurtains(() =>
        {
            revealPanel.SetActive(true);

            if (allCorrect)
            {
                revealText.text =
                    $"*The King claps enthusiastically!*\n\n" +
                    $"\"MAGNIFICENT! A <b>{state.TargetColor} {state.TargetMaterial} {state.TargetClothing}</b>!\"\n\n" +
                    $"\"You shall be my personal <b>Fashion Guru</b>!\"\n\n" +
                    $"He steps forward proudly...\n\n" +
                    $"<i>He is wearing absolutely nothing at all.</i>";
            }
            else
            {
                revealText.text =
                    $"The King peers at your answers...\n\n" +
                    $"Some were right. Some were... not.\n\n" +
                    $"He steps forward to show you his outfit...\n\n" +
                    $"<i>He is wearing absolutely nothing at all.</i>";
            }

            revealContinueButton.onClick.RemoveAllListeners();
            revealContinueButton.onClick.AddListener(() =>
            {
                if (allCorrect)
                    GameManager.Instance.GoToPhase(GamePhase.FinalJudgment);
                else
                    GameManager.Instance.GoToPhase(GamePhase.DeathScreen);
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
        }
        else
        {
            label.text  = $"<b>{category}: {guessed} ✗</b>\n<size=18>({correct})</size>";
            label.color = new Color(1f, 0.3f, 0.3f);
        }
    }

    public void ShowFinalJudgment()
    {
        HideAllOverlays();
        stagePanel?.SetActive(true);
        finalJudgmentPanel.SetActive(true);
        finalText.text =
            "The King beams at you with absolute confidence.\n\n" +
            "\"Well?\" he says. \"What do you think of my magnificent outfit?\"\n\n" +
            "<i>Choose your words very, very carefully.</i>";
        flatterButton.GetComponentInChildren<TMP_Text>().text = "\"Your Majesty, you look absolutely divine!\"";
        truthButton.GetComponentInChildren<TMP_Text>().text   = "\"...You're naked.\"";
        flatterButton.onClick.RemoveAllListeners();
        truthButton.onClick.RemoveAllListeners();
        flatterButton.onClick.AddListener(() => GameManager.Instance.OnPlayerFlatters());
        truthButton.onClick.AddListener(() => GameManager.Instance.OnPlayerTruth());
    }

    public void ShowWin()
    {
        HideAllOverlays();
        stagePanel?.SetActive(false);
        winPanel.SetActive(true);
        winText.text =
            "The King claps with delight!\n\n" +
            "\"YES! You truly have the finest eyes in all the kingdom!\"\n\n" +
            "You survive. The King is happy.\nThe kingdom is at peace.\n\n" +
            "<i>(He is still naked.)</i>";
        winPlayAgainButton.onClick.RemoveAllListeners();
        winPlayAgainButton.onClick.AddListener(() => GameManager.Instance.OnPlayAgain());
    }

    public void ShowDeath()
    {
        HideAllOverlays();
        stagePanel?.SetActive(false);
        deathPanel.SetActive(true);

        var state = GameManager.Instance.State;
        string reason = "";
        if (state.GuessedClothing != null && state.GuessedClothing != state.TargetClothing)
            reason = $"A <b>{state.GuessedClothing}</b>?! The King wears a magnificent <b>{state.TargetClothing}</b>!";
        else if (state.GuessedColor != null && state.GuessedColor != state.TargetColor)
            reason = $"<b>{state.GuessedColor}</b>?! The colour is <b>{state.TargetColor}</b>, you blind fool!";
        else if (state.GuessedMaterial != null && state.GuessedMaterial != state.TargetMaterial)
            reason = $"<b>{state.GuessedMaterial}</b>?! It is obviously <b>{state.TargetMaterial}</b>!";
        else
            reason = "You dared to speak the truth to the King!";

        deathText.text =
            "The King's eyes narrow.\n\n" +
            $"\"{reason}\"\n\n" +
            "\"GUARDS! OFF WITH THEIR HEAD!\"\n\n" +
            "<i>You should have lied.</i>";

        deathPlayAgainButton.onClick.RemoveAllListeners();
        deathPlayAgainButton.onClick.AddListener(() => GameManager.Instance.OnPlayAgain());
    }
}
