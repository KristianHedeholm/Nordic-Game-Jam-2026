using System;
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
    }

    void ClearOptions()
    {
        foreach (Transform child in optionsContainer)
            Destroy(child.gameObject);
    }

    public void ShowIntro()
    {
        HideAllOverlays();
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

    public void ShowReveal(string color, string clothing, string material)
    {
        HideAllOverlays();
        stagePanel?.SetActive(true);
        ClearOptions();
        categoryLabel.text = "";
        riddleText.text = "...";

        // Animate curtains open, then show reveal text
        curtainAnimator?.OpenCurtains(() =>
        {
            revealPanel.SetActive(true);
            revealText.text =
                $"You guessed: a <b>{color} {material} {clothing}</b>.\n\n" +
                $"The curtain falls...\n\n" +
                $"The King stands before you.\n\n" +
                $"He is wearing... <b>absolutely nothing at all.</b>";
            revealContinueButton.onClick.RemoveAllListeners();
            revealContinueButton.onClick.AddListener(() => GameManager.Instance.GoToPhase(GamePhase.FinalJudgment));
        });
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
        deathText.text =
            "The King's eyes narrow.\n\n" +
            "\"NAKED?!\"\n\n" +
            "\"GUARDS! OFF WITH THEIR HEAD!\"\n\n" +
            "You told the truth.\nIt didn't matter.\n\n" +
            "<i>It never does.</i>";
        deathPlayAgainButton.onClick.RemoveAllListeners();
        deathPlayAgainButton.onClick.AddListener(() => GameManager.Instance.OnPlayAgain());
    }
}
